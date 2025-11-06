using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Proyecto_Analisis_API.Models.Auth;
using Proyecto_Analisis_API.Helpers.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Proyecto_Analisis_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _cfg;
        public AuthController(IConfiguration cfg) => _cfg = cfg;

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email y contraseña son obligatorios.");

            // 1) Buscar usuario por correo
            const string sql = @"
                SELECT TOP(1)
                    U.USU_CODIGO_USUARIO,
                    U.EMP_CODIGO_EMPLEADO,
                    U.CRO_CODIGO_ROL,
                    U.USU_CORREO,
                    U.USU_HASH_PASSWORD,
                    U.USU_ESTADO,
                    U.USU_INTENTOS_FALLIDOS,
                    U.USU_BLOQUEADO_HASTA,
                    E.EMP_NOMBRES, E.EMP_APELLIDO_PATERNO
                FROM dbo.REI_USUARIO U
                LEFT JOIN dbo.REI_EMPLEADO E ON U.EMP_CODIGO_EMPLEADO = E.EMP_CODIGO_EMPLEADO
                WHERE U.USU_CORREO = @EMAIL;";

            int userId = 0, empId = 0, rolId = 0, intentos = 0;
            string? email = null, hash = null, nombres = null, ape = null;
            bool activo = false;
            DateTime? bloqueadoHasta = null;

            using (var cn = new SqlConnection(_cfg["ConnectionStrings:conexion1"]))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@EMAIL", req.Email);
                cn.Open();

                using var rd = cmd.ExecuteReader();
                if (!rd.Read())
                    return Unauthorized("Credenciales inválidas.");

                userId = rd.GetInt32(rd.GetOrdinal("USU_CODIGO_USUARIO"));
                empId = rd.GetInt32(rd.GetOrdinal("EMP_CODIGO_EMPLEADO"));
                rolId = rd.GetInt32(rd.GetOrdinal("CRO_CODIGO_ROL"));
                email = rd.GetString(rd.GetOrdinal("USU_CORREO"));
                hash = rd.GetString(rd.GetOrdinal("USU_HASH_PASSWORD"));
                activo = rd.GetBoolean(rd.GetOrdinal("USU_ESTADO"));
                intentos = rd.IsDBNull(rd.GetOrdinal("USU_INTENTOS_FALLIDOS")) ? 0 : rd.GetInt32(rd.GetOrdinal("USU_INTENTOS_FALLIDOS"));
                bloqueadoHasta = rd.IsDBNull(rd.GetOrdinal("USU_BLOQUEADO_HASTA")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("USU_BLOQUEADO_HASTA"));
                nombres = rd.IsDBNull(rd.GetOrdinal("EMP_NOMBRES")) ? null : rd.GetString(rd.GetOrdinal("EMP_NOMBRES"));
                ape = rd.IsDBNull(rd.GetOrdinal("EMP_APELLIDO_PATERNO")) ? null : rd.GetString(rd.GetOrdinal("EMP_APELLIDO_PATERNO"));
            }

            if (!activo) return Unauthorized("Usuario inactivo.");
            if (bloqueadoHasta.HasValue && bloqueadoHasta > DateTime.UtcNow)
                return Unauthorized("Usuario bloqueado temporalmente. Intenta más tarde.");

            // 2) Verificar contraseña
            if (!PasswordHasher.Verify(req.Password!, hash!))
            {
                int nuevosIntentos = intentos + 1;
                DateTime? nuevoBloqueo = null;

                if (nuevosIntentos >= 5)
                {
                    nuevoBloqueo = DateTime.UtcNow.AddMinutes(15); // bloqueo temporal 15 min
                    nuevosIntentos = 0; // reset tras bloqueo (opcional)
                }

                using var cn2 = new SqlConnection(_cfg["ConnectionStrings:conexion1"]);
                using var cmd2 = new SqlCommand(@"
                    UPDATE dbo.REI_USUARIO
                    SET USU_INTENTOS_FALLIDOS = @I, USU_BLOQUEADO_HASTA = @B
                    WHERE USU_CODIGO_USUARIO = @ID", cn2);
                cmd2.Parameters.AddWithValue("@I", nuevosIntentos);
                cmd2.Parameters.AddWithValue("@B", (object?)nuevoBloqueo ?? DBNull.Value);
                cmd2.Parameters.AddWithValue("@ID", userId);
                cn2.Open();
                cmd2.ExecuteNonQuery();

                return Unauthorized("Credenciales inválidas.");
            }

            // 3) Resetear intentos + último ingreso
            using (var cn3 = new SqlConnection(_cfg["ConnectionStrings:conexion1"]))
            using (var cmd3 = new SqlCommand(@"
                UPDATE dbo.REI_USUARIO
                SET USU_INTENTOS_FALLIDOS = 0, USU_BLOQUEADO_HASTA = NULL, USU_ULTIMO_INGRESO = SYSDATETIME()
                WHERE USU_CODIGO_USUARIO = @ID;", cn3))
            {
                cmd3.Parameters.AddWithValue("@ID", userId);
                cn3.Open();
                cmd3.ExecuteNonQuery();
            }

            // 4) Emitir JWT
            var jwt = _cfg.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenMinutes"] ?? "60"));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email!),
                new Claim("empId", empId.ToString()),
                new Claim(ClaimTypes.Role, rolId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var resp = new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires,
                UsuarioId = userId,
                EmpleadoId = empId,
                RolId = rolId,
                Email = email,
                Nombre = $"{nombres ?? ""} {ape ?? ""}".Trim()
            };

            return Ok(resp);
        }

        [HttpGet("hash/{plain}")]
        public ActionResult<string> HashPreview(string plain)
        {
            var hash = Proyecto_Analisis_API.Helpers.Security.PasswordHasher.Hash(plain);
            return Ok(hash);
        }
    }
}

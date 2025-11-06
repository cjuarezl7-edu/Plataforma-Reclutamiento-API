using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Proyecto_Analisis_API.Class;
using Proyecto_Analisis_API.Models;
using System.Data;

namespace Proyecto_Analisis_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VacanteController : Controller
    {
        private readonly IConfiguration _configuration;

        // ⚠️ Cambia este nombre si en tu DB quedó con el typo: "VAC_OFRECEMINETO"
        private const string COL_OFREC = "VAC_OFRECIMIENTO"; // o "VAC_OFRECEMINETO"

        public VacanteController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Helper para leer nullables sin tanto código repetido
        private static T? Get<T>(SqlDataReader rd, string col)
        {
            var i = rd.GetOrdinal(col);
            var v = rd[col];
            return v == DBNull.Value ? default : (T)v;
        }

        // ============================
        // GET ALL
        // ============================
        [HttpGet]
        public ActionResult<IEnumerable<VacanteListadoModel>> GetAll()
        {
            var vacantes = new List<VacanteListadoModel>();

            string query = $@"
                SELECT
                    A.VAC_CODIGO_VACANTE,
                    B.CAR_NOMBRE                                        AS NOMBRE_AREA,
                    A.VAC_TITULO,
                    A.VAC_DESCRIPCION,
                    A.VAC_REQUISITOS,
                    A.{COL_OFREC},                                      -- ofrecimiento
                    A.VAC_REQUERIMIENTOS,
                    A.VAC_URL_IMAGEN,
                    A.VAC_FECHA_CREACION,
                    A.VAC_FECHA_CIERRE,
                    A.VAC_FECHA_MODIFICACION,
                    C.CEV_NOMBRE                                        AS ESTADO_VACANTE,
                    LTRIM(RTRIM(CONCAT(E.EMP_NOMBRES, ' ', E.EMP_APELLIDO_PATERNO))) AS USUARIO_CREACION
                FROM dbo.REI_VACANTE              AS A
                LEFT JOIN dbo.REI_CAT_AREA        AS B ON A.VAC_CODIGO_AREA       = B.CAR_CODIGO_AREA
                LEFT JOIN dbo.REI_CAT_ESTADO_VACANTE C ON A.CEV_ESTADO_VACANTE    = C.CEV_CODIGO_ESTADO_VACANTE
                LEFT JOIN dbo.REI_USUARIO         AS D ON A.VAC_USUARIO_CREACION  = D.USU_CODIGO_USUARIO
                LEFT JOIN dbo.REI_EMPLEADO        AS E ON D.EMP_CODIGO_EMPLEADO   = E.EMP_CODIGO_EMPLEADO
                ORDER BY A.VAC_FECHA_CREACION DESC;";

            using var connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]);
            using var command = new SqlCommand(query, connection) { CommandType = CommandType.Text };
            connection.Open();

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new VacanteListadoModel
                {
                    VAC_CODIGO_VACANTE = reader.GetInt32(reader.GetOrdinal("VAC_CODIGO_VACANTE")),
                    NOMBRE_AREA = Get<string>(reader, "NOMBRE_AREA"),
                    VAC_TITULO = Get<string>(reader, "VAC_TITULO"),
                    VAC_DESCRIPCION = Get<string>(reader, "VAC_DESCRIPCION"),
                    VAC_REQUISITOS = Get<string>(reader, "VAC_REQUISITOS"),
                    VAC_OFRECIMIENTO = Get<string>(reader, COL_OFREC),           // ofrecimiento
                    VAC_REQUERIMIENTOS = Get<string>(reader, "VAC_REQUERIMIENTOS"),
                    VAC_URL_IMAGEN = Get<string>(reader, "VAC_URL_IMAGEN"),
                    VAC_FECHA_CREACION = Get<DateTime?>(reader, "VAC_FECHA_CREACION"),
                    VAC_FECHA_CIERRE = Get<DateTime?>(reader, "VAC_FECHA_CIERRE"),
                    VAC_FECHA_MODIFICACION = Get<DateTime?>(reader, "VAC_FECHA_MODIFICACION"),
                    ESTADO_VACANTE = Get<string>(reader, "ESTADO_VACANTE"),
                    USUARIO_CREACION = Get<string>(reader, "USUARIO_CREACION"),
                };
                vacantes.Add(item);
            }

            return Ok(vacantes);
        }

        // ============================
        // POST (CREATE)
        // ============================
        [HttpPost]
        public ActionResult Insert([FromBody] VacanteModel model)
        {
            if (model == null) return BadRequest("El modelo no puede ser nulo.");

            // Nota: Usamos SYSDATETIME() para la fecha de creación
            string query = $@"
                INSERT INTO dbo.REI_VACANTE
                (
                  VAC_CODIGO_AREA,
                  VAC_TITULO,
                  VAC_DESCRIPCION,
                  VAC_REQUISITOS,
                  {COL_OFREC},                -- ofrecimiento
                  VAC_REQUERIMIENTOS,
                  VAC_URL_IMAGEN,
                  VAC_FECHA_CREACION,
                  VAC_FECHA_CIERRE,
                  CEV_ESTADO_VACANTE,
                  VAC_USUARIO_CREACION
                )
                VALUES
                (
                  @VAC_CODIGO_AREA,
                  @VAC_TITULO,
                  @VAC_DESCRIPCION,
                  @VAC_REQUISITOS,
                  @VAC_OFREC,                 -- ofrecimiento
                  @VAC_REQUERIMIENTOS,
                  @VAC_URL_IMAGEN,
                  SYSDATETIME(),
                  @VAC_FECHA_CIERRE,
                  @CEV_ESTADO_VACANTE,
                  @VAC_USUARIO_CREACION
                );
                
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            try
            {
                int newId;
                using var connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]);
                using var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@VAC_CODIGO_AREA", model.VAC_CODIGO_AREA);
                command.Parameters.AddWithValue("@VAC_TITULO", (object?)model.VAC_TITULO ?? DBNull.Value);
                command.Parameters.AddWithValue("@VAC_DESCRIPCION", (object?)model.VAC_DESCRIPCION ?? DBNull.Value);
                command.Parameters.AddWithValue("@VAC_REQUISITOS", (object?)model.VAC_REQUISITOS ?? DBNull.Value);
                command.Parameters.AddWithValue("@VAC_OFREC", (object?)model.VAC_OFRECIMIENTO ?? DBNull.Value);
                command.Parameters.AddWithValue("@VAC_REQUERIMIENTOS", (object?)model.VAC_REQUERIMIENTOS ?? DBNull.Value);
                command.Parameters.AddWithValue("@VAC_URL_IMAGEN", (object?)model.VAC_URL_IMAGEN ?? DBNull.Value);

                if (model.VAC_FECHA_CIERRE == default) command.Parameters.AddWithValue("@VAC_FECHA_CIERRE", DBNull.Value);
                else command.Parameters.AddWithValue("@VAC_FECHA_CIERRE", model.VAC_FECHA_CIERRE);

                command.Parameters.AddWithValue("@CEV_ESTADO_VACANTE", model.CEV_ESTADO_VACANTE);
                command.Parameters.AddWithValue("@VAC_USUARIO_CREACION", (object?)model.VAC_USUARIO_CREACION ?? DBNull.Value);

                connection.Open();
                newId = (int)command.ExecuteScalar();

                return Ok(new { message = "Vacante creada correctamente.", id = newId });
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error SQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        // ============================
        // GET BY ID
        // ============================
        [HttpGet("{id:int}")]
        public ActionResult<VacanteListadoModel> GetById(int id)
        {
            VacanteListadoModel? vacante = null;

            string query = $@"
                SELECT
                    A.VAC_CODIGO_VACANTE,
                    A.VAC_CODIGO_AREA,
                    A.VAC_TITULO,
                    A.VAC_DESCRIPCION,
                    A.VAC_REQUISITOS,
                    A.{COL_OFREC},                    -- ofrecimiento
                    A.VAC_REQUERIMIENTOS,
                    A.VAC_URL_IMAGEN,
                    A.VAC_FECHA_CREACION,
                    A.VAC_FECHA_CIERRE,
                    A.VAC_FECHA_MODIFICACION,
                    A.CEV_ESTADO_VACANTE,
                    LTRIM(RTRIM(CONCAT(E.EMP_NOMBRES, ' ', E.EMP_APELLIDO_PATERNO))) AS USUARIO_CREACION
                FROM dbo.REI_VACANTE              AS A
                LEFT JOIN dbo.REI_CAT_AREA        AS B ON A.VAC_CODIGO_AREA       = B.CAR_CODIGO_AREA
                LEFT JOIN dbo.REI_CAT_ESTADO_VACANTE C ON A.CEV_ESTADO_VACANTE    = C.CEV_CODIGO_ESTADO_VACANTE
                LEFT JOIN dbo.REI_USUARIO         AS D ON A.VAC_USUARIO_CREACION  = D.USU_CODIGO_USUARIO
                LEFT JOIN dbo.REI_EMPLEADO        AS E ON D.EMP_CODIGO_EMPLEADO   = E.EMP_CODIGO_EMPLEADO
                WHERE A.VAC_CODIGO_VACANTE = @ID;";

            using var connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ID", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                vacante = new VacanteListadoModel
                {
                    VAC_CODIGO_VACANTE = reader.GetInt32(reader.GetOrdinal("VAC_CODIGO_VACANTE")),
                    VAC_CODIGO_AREA = reader.GetInt32(reader.GetOrdinal("VAC_CODIGO_AREA")),
                    VAC_TITULO = Get<string>(reader, "VAC_TITULO"),
                    VAC_DESCRIPCION = Get<string>(reader, "VAC_DESCRIPCION"),
                    VAC_REQUISITOS = Get<string>(reader, "VAC_REQUISITOS"),
                    VAC_OFRECIMIENTO = Get<string>(reader, COL_OFREC),
                    VAC_REQUERIMIENTOS = Get<string>(reader, "VAC_REQUERIMIENTOS"),
                    VAC_URL_IMAGEN = Get<string>(reader, "VAC_URL_IMAGEN"),
                    VAC_FECHA_CREACION = Get<DateTime?>(reader, "VAC_FECHA_CREACION"),
                    VAC_FECHA_CIERRE = Get<DateTime?>(reader, "VAC_FECHA_CIERRE"),
                    VAC_FECHA_MODIFICACION = Get<DateTime?>(reader, "VAC_FECHA_MODIFICACION"),
                    CEV_ESTADO_VACANTE = reader.GetInt32(reader.GetOrdinal("CEV_ESTADO_VACANTE")),
                    USUARIO_CREACION = Get<string>(reader, "USUARIO_CREACION"),
                };
            }

            return vacante is null
                ? NotFound($"Vacante con id {id} no encontrada.")
                : Ok(vacante);
        }

        // ============================
        // PUT (UPDATE)
        // ============================
        [HttpPut("{id:int}")]
        public ActionResult UpdateAdo(int id, [FromBody] VacanteModel model)
        {
            if (model == null || id != model.VAC_CODIGO_VACANTE)
                return BadRequest("Datos inválidos.");

            string query = $@"
                UPDATE dbo.REI_VACANTE
                SET
                  VAC_CODIGO_AREA          = @VAC_CODIGO_AREA,
                  VAC_TITULO               = @VAC_TITULO,
                  VAC_DESCRIPCION          = @VAC_DESCRIPCION,
                  VAC_REQUISITOS           = @VAC_REQUISITOS,
                  {COL_OFREC}              = @VAC_OFREC,            -- ofrecimiento
                  VAC_REQUERIMIENTOS       = @VAC_REQUERIMIENTOS,
                  VAC_URL_IMAGEN           = @VAC_URL_IMAGEN,
                  VAC_FECHA_CIERRE         = @VAC_FECHA_CIERRE,
                  CEV_ESTADO_VACANTE       = @CEV_ESTADO_VACANTE,
                  VAC_USUARIO_MODIFICACION = @VAC_USUARIO_MODIFICACION,
                  VAC_FECHA_MODIFICACION   = SYSDATETIME()
                WHERE VAC_CODIGO_VACANTE = @VAC_CODIGO_VACANTE;";

            using var cn = new SqlConnection(_configuration["ConnectionStrings:conexion1"]);
            using var cmd = new SqlCommand(query, cn);

            cmd.Parameters.AddWithValue("@VAC_CODIGO_VACANTE", id);
            cmd.Parameters.AddWithValue("@VAC_CODIGO_AREA", model.VAC_CODIGO_AREA);
            cmd.Parameters.AddWithValue("@VAC_TITULO", (object?)model.VAC_TITULO ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VAC_DESCRIPCION", (object?)model.VAC_DESCRIPCION ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VAC_REQUISITOS", (object?)model.VAC_REQUISITOS ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VAC_OFREC", (object?)model.VAC_OFRECIMIENTO ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VAC_REQUERIMIENTOS", (object?)model.VAC_REQUERIMIENTOS ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VAC_URL_IMAGEN", (object?)model.VAC_URL_IMAGEN ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VAC_FECHA_CIERRE", model.VAC_FECHA_CIERRE == default ? DBNull.Value : model.VAC_FECHA_CIERRE);
            cmd.Parameters.AddWithValue("@CEV_ESTADO_VACANTE", model.CEV_ESTADO_VACANTE);
            cmd.Parameters.AddWithValue("@VAC_USUARIO_MODIFICACION", (object?)model.VAC_USUARIO_MODIFICACION ?? DBNull.Value);

            cn.Open();
            var rows = cmd.ExecuteNonQuery();
            if (rows == 0) return NotFound($"Vacante con id {id} no encontrada.");
            return Ok("Vacante actualizada correctamente.");
        }

        // ============================
        // DELETE
        // ============================
        [HttpDelete("{id:int}")]
        public ActionResult DeleteAdo(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            const string query = @"DELETE FROM dbo.REI_VACANTE WHERE VAC_CODIGO_VACANTE = @ID";

            using var cn = new SqlConnection(_configuration["ConnectionStrings:conexion1"]);
            using var cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@ID", id);

            cn.Open();
            var rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound($"No se encontró la vacante con id {id}.");

            return Ok("Vacante eliminada correctamente.");
        }
    }
}

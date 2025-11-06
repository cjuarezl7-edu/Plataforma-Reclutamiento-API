using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Proyecto_Analisis_API.Models;

namespace Proyecto_Analisis_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostulacionesController : Controller
    {
        private readonly IConfiguration _configuration;

        public PostulacionesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("postulaciones")]
        public ActionResult<IEnumerable<VacanteListadoModel>> GetPostulaciones()
        {
            var list = new List<VacanteListadoModel>();
            string sql = @"
                        SELECT
                          A.VAC_CODIGO_VACANTE,
                          B.CAR_NOMBRE            AS NOMBRE_AREA,
                          A.VAC_TITULO,
                          A.VAC_DESCRIPCION,
                          A.VAC_FECHA_CREACION,
                          A.VAC_FECHA_CIERRE,
                          C.CEV_NOMBRE            AS ESTADO_VACANTE,
                          LTRIM(RTRIM(CONCAT(E.EMP_NOMBRES, ' ', E.EMP_APELLIDO_PATERNO))) AS USUARIO_CREACION
                        FROM dbo.REI_VACANTE A
                        LEFT JOIN dbo.REI_CAT_AREA B              ON A.VAC_CODIGO_AREA = B.CAR_CODIGO_AREA
                        LEFT JOIN dbo.REI_CAT_ESTADO_VACANTE C    ON A.CEV_ESTADO_VACANTE = C.CEV_CODIGO_ESTADO_VACANTE
                        LEFT JOIN dbo.REI_USUARIO D               ON A.VAC_USUARIO_CREACION = D.USU_CODIGO_USUARIO
                        LEFT JOIN dbo.REI_EMPLEADO E              ON D.EMP_CODIGO_EMPLEADO = E.EMP_CODIGO_EMPLEADO
                        WHERE A.CEV_ESTADO_VACANTE = 1
                        ORDER BY A.VAC_FECHA_CREACION DESC;";

            using var cn = new SqlConnection(_configuration["ConnectionStrings:conexion1"]);
            using var cmd = new SqlCommand(sql, cn);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new VacanteListadoModel
                {
                    VAC_CODIGO_VACANTE = rd.GetInt32(rd.GetOrdinal("VAC_CODIGO_VACANTE")),
                    NOMBRE_AREA = rd.IsDBNull(rd.GetOrdinal("NOMBRE_AREA")) ? null : rd.GetString(rd.GetOrdinal("NOMBRE_AREA")),
                    VAC_TITULO = rd.IsDBNull(rd.GetOrdinal("VAC_TITULO")) ? null : rd.GetString(rd.GetOrdinal("VAC_TITULO")),
                    VAC_DESCRIPCION = rd.IsDBNull(rd.GetOrdinal("VAC_DESCRIPCION")) ? null : rd.GetString(rd.GetOrdinal("VAC_DESCRIPCION")),
                    VAC_FECHA_CREACION = rd.IsDBNull(rd.GetOrdinal("VAC_FECHA_CREACION")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("VAC_FECHA_CREACION")),
                    VAC_FECHA_CIERRE = rd.IsDBNull(rd.GetOrdinal("VAC_FECHA_CIERRE")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("VAC_FECHA_CIERRE")),
                    ESTADO_VACANTE = rd.IsDBNull(rd.GetOrdinal("ESTADO_VACANTE")) ? null : rd.GetString(rd.GetOrdinal("ESTADO_VACANTE")),
                    USUARIO_CREACION = rd.IsDBNull(rd.GetOrdinal("USUARIO_CREACION")) ? null : rd.GetString(rd.GetOrdinal("USUARIO_CREACION"))
                });
            }

            return Ok(list);
        }



    }
}

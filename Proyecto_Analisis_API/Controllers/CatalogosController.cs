using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Proyecto_Analisis_API.Models;

namespace Proyecto_Analisis_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CatalogosController : Controller
    {
        private readonly IConfiguration _configuration;

        public CatalogosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/Catalogo/areas
        [HttpGet("areas")]
        public ActionResult<IEnumerable<CatalogoItemModel>> GetAreas()
        {
            var items = new List<CatalogoItemModel>();
            const string sql = @"
            SELECT CAR_CODIGO_AREA AS Id, CAR_NOMBRE AS Nombre
            FROM dbo.REI_CAT_AREA
            WHERE CAR_ESTADO = 1
            ORDER BY CAR_NOMBRE;";

            using var cn = new SqlConnection(_configuration["ConnectionStrings:conexion1"]);
            using var cmd = new SqlCommand(sql, cn);
            cn.Open();

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                items.Add(new CatalogoItemModel
                {
                    Id = rd.GetInt32(rd.GetOrdinal("Id")),
                    Nombre = rd.IsDBNull(rd.GetOrdinal("Nombre")) ? null : rd.GetString(rd.GetOrdinal("Nombre"))
                });
            }
            return Ok(items);
        }

        // GET: api/Catalogos/estados-vacante
        [HttpGet("estados-vacante")]
        public ActionResult<IEnumerable<CatalogoItemModel>> GetEstadosVacante()
        {
            var items = new List<CatalogoItemModel>();
            const string sql = @"
            SELECT CEV_CODIGO_ESTADO_VACANTE AS Id, CEV_NOMBRE AS Nombre
            FROM dbo.REI_CAT_ESTADO_VACANTE
            WHERE CEV_ESTADO = 1
            ORDER BY CEV_NOMBRE;";

            using var cn = new SqlConnection(_configuration["ConnectionStrings:conexion1"]);
            using var cmd = new SqlCommand(sql, cn);
            cn.Open();

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                items.Add(new CatalogoItemModel
                {
                    Id = rd.GetInt32(rd.GetOrdinal("Id")),
                    Nombre = rd.IsDBNull(rd.GetOrdinal("Nombre")) ? null : rd.GetString(rd.GetOrdinal("Nombre"))
                });
            }
            return Ok(items);
        }
    }
}

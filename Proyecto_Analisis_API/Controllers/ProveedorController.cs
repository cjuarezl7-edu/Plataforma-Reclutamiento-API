using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Proyecto_Analisis_API.Class;
using Proyecto_Analisis_API.Models;
using System.Collections.Generic;

namespace Proyecto_Analisis_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedorController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProveedorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Método para insertar un nuevo proveedor (CREATE)
        [HttpPost]
        public ActionResult Insert(ProveedorModel proveedor)
        {
            if (proveedor == null)
            {
                return BadRequest("El modelo no puede ser nulo.");
            }

            ClassConect classconect = new ClassConect();

            // Obtener la fecha y hora actual
            DateTime FechaActual = DateTime.Now;
            string Usuario = "Usuario Test";

            string query = @"INSERT INTO COM_PROVEEDOR (PRO_NOMBRE, PRO_DIRECCION, PRO_NIT, PRO_TELEFONO, PRO_CORREO, PRO_FECHA_CREACION, PRO_USUARIO_CREACION) 
                            VALUES (@PRO_NOMBRE, @PRO_DIRECCION, @PRO_NIT, @PRO_TELEFONO, @PRO_CORREO, @PRO_FECHA_CREACION, @PRO_USUARIO_CREACION);";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PRO_NOMBRE", proveedor.PRO_NOMBRE),
                new SqlParameter("@PRO_DIRECCION", proveedor.PRO_DIRECCION),
                new SqlParameter("@PRO_NIT", proveedor.PRO_NIT),
                new SqlParameter("@PRO_TELEFONO", proveedor.PRO_TELEFONO),
                new SqlParameter("@PRO_CORREO", proveedor.PRO_CORREO),
                new SqlParameter("@PRO_FECHA_CREACION", FechaActual),
                new SqlParameter("@PRO_USUARIO_CREACION", Usuario)
            };

            var resultado = classconect.CUDQuery(query, parameters, _configuration["ConnectionStrings:conexion1"]);

            if (resultado)
            {
                return Ok("Registro insertado correctamente.");
            }
            else
            {
                return BadRequest("Error al insertar el registro.");
            }
        }

        // Método para obtener todos los proveedores (READ - GET ALL)
        [HttpGet]
        public ActionResult<IEnumerable<ProveedorModel>> GetAll()
        {
            List<ProveedorModel> proveedores = new List<ProveedorModel>();
            //string query = @"SELECT PRO_PROVEEDOR, PRO_NOMBRE, PRO_DIRECCION, PRO_NIT, PRO_TELEFONO, PRO_CORREO, PRO_FECHA_CREACION, PRO_USUARIO_CREACION FROM COM_PROVEEDOR";
            string query = @"SELECT PRO_PROVEEDOR, PRO_NOMBRE, PRO_DIRECCION, PRO_NIT, PRO_TELEFONO, PRO_CORREO, PRO_FECHA_CREACION, PRO_USUARIO_CREACION FROM COM_PROVEEDOR";

            using (SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ProveedorModel proveedor = new ProveedorModel
                    {
                        PRO_PROVEEDOR = (int)reader["PRO_PROVEEDOR"],
                        PRO_NOMBRE = reader["PRO_NOMBRE"].ToString(),
                        PRO_DIRECCION = reader["PRO_DIRECCION"].ToString(),
                        PRO_NIT = reader["PRO_NIT"].ToString(),
                        PRO_TELEFONO = reader["PRO_TELEFONO"].ToString(),
                        PRO_CORREO = reader["PRO_CORREO"].ToString(),
                        PRO_FECHA_CREACION = reader["PRO_FECHA_CREACION"].ToString(),
                        PRO_USUARIO_CREACION = reader["PRO_USUARIO_CREACION"].ToString()
                    };
                    proveedores.Add(proveedor);
                }
            }
            return Ok(proveedores);
        }

        // Método para obtener un proveedor por ID (READ - GET by ID)
        [HttpGet("{id}")]
        public ActionResult<ProveedorModel> GetById(int id)
        {
            ProveedorModel proveedor = null;
            string query = @"SELECT PRO_PROVEEDOR, PRO_NOMBRE, PRO_DIRECCION, PRO_NIT, PRO_TELEFONO, PRO_CORREO FROM COM_PROVEEDOR WHERE PRO_PROVEEDOR = @PRO_PROVEEDOR";

            using (SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PRO_PROVEEDOR", id);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    proveedor = new ProveedorModel
                    {
                        PRO_PROVEEDOR = (int)reader["PRO_PROVEEDOR"],
                        PRO_NOMBRE = reader["PRO_NOMBRE"].ToString(),
                        PRO_DIRECCION = reader["PRO_DIRECCION"].ToString(),
                        PRO_NIT = reader["PRO_NIT"].ToString(),
                        PRO_TELEFONO = reader["PRO_TELEFONO"].ToString(),
                        PRO_CORREO = reader["PRO_CORREO"].ToString()
                    };
                }
            }

            if (proveedor == null)
            {
                return NotFound("Proveedor no encontrado.");
            }
            return Ok(proveedor);
        }

        // Método para actualizar un proveedor (UPDATE)
        [HttpPut("{id}")]
        public ActionResult Update(int id, ProveedorModel proveedor)
        {
            if (proveedor == null || id != proveedor.PRO_PROVEEDOR)
            {
                return BadRequest("Datos inválidos.");
            }

            ClassConect classconect = new ClassConect();

            string query = @"UPDATE COM_PROVEEDOR SET PRO_NOMBRE = @PRO_NOMBRE, PRO_DIRECCION = @PRO_DIRECCION, 
                             PRO_NIT = @PRO_NIT, PRO_TELEFONO = @PRO_TELEFONO, PRO_CORREO = @PRO_CORREO
                             WHERE PRO_PROVEEDOR = @PRO_PROVEEDOR";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PRO_NOMBRE", proveedor.PRO_NOMBRE),
                new SqlParameter("@PRO_DIRECCION", proveedor.PRO_DIRECCION),
                new SqlParameter("@PRO_NIT", proveedor.PRO_NIT),
                new SqlParameter("@PRO_TELEFONO", proveedor.PRO_TELEFONO),
                new SqlParameter("@PRO_CORREO", proveedor.PRO_CORREO),
                new SqlParameter("@PRO_PROVEEDOR", proveedor.PRO_PROVEEDOR)
            };

            var resultado = classconect.CUDQuery(query, parameters, _configuration["ConnectionStrings:conexion1"]);

            if (resultado)
            {
                return Ok("Registro actualizado correctamente.");
            }
            else
            {
                return BadRequest("Error al actualizar el registro.");
            }
        }

        // Método para eliminar un proveedor (DELETE)
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            ClassConect classconect = new ClassConect();

            string query = @"DELETE FROM COM_PROVEEDOR WHERE PRO_PROVEEDOR = @PRO_PROVEEDOR";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PRO_PROVEEDOR", id)
            };

            var resultado = classconect.CUDQuery(query, parameters, _configuration["ConnectionStrings:conexion1"]);

            if (resultado)
            {
                return Ok("Registro eliminado correctamente.");
            }
            else
            {
                return BadRequest("Error al eliminar el registro.");
            }
        }
    }
}

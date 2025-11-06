using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PdfSharpCore.Drawing;
using Proyecto_Analisis_API.Class;
using Proyecto_Analisis_API.Models;
using System.Collections.Generic;
using PdfSharpCore.Pdf;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using System.IO;

namespace Proyecto_Analisis_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrdenController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Método para insertar una nueva orden de compra (CREATE)
        [HttpPost]
        public ActionResult Insert(OrdenModel orden)
        {
            if (orden == null)
            {
                return BadRequest("El modelo no puede ser nulo.");
            }

            ClassConect classconect = new ClassConect();

            // Obtener la fecha y hora actual
            DateTime FechaActual = DateTime.Now;
            string UsuarioCreador = "Usuario Test"; // Cambia este valor por el usuario autenticado
            string UsuarioAprobador = orden.ORD_APROBADOPOR ?? ""; // Puede estar vacío si aún no ha sido aprobada

            string query = @"INSERT INTO COM_ORDENES (ORD_PROVEEDOR, ORD_FECHAORDEN, ORD_MONTOTOTAL, ORD_ESTADO, ORD_CREADOPOR, ORD_APROBADOPOR) 
                            VALUES (@ORD_PROVEEDOR, @ORD_FECHAORDEN, @ORD_MONTOTOTAL, @ORD_ESTADO, @ORD_CREADOPOR, @ORD_APROBADOPOR);";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ORD_PROVEEDOR", orden.ORD_PROVEEDOR),
                new SqlParameter("@ORD_FECHAORDEN", FechaActual),
                new SqlParameter("@ORD_MONTOTOTAL", orden.ORD_MONTOTOTAL),
                new SqlParameter("@ORD_ESTADO", orden.ORD_ESTADO),
                new SqlParameter("@ORD_CREADOPOR", UsuarioCreador),
                new SqlParameter("@ORD_APROBADOPOR", UsuarioAprobador)
            };

            var resultado = classconect.CUDQuery(query, parameters, _configuration["ConnectionStrings:conexion1"]);

            if (resultado)
            {
                return Ok("Orden de compra insertada correctamente.");
            }
            else
            {
                return BadRequest("Error al insertar la orden de compra.");
            }
        }

        // Método para obtener todas las órdenes de compra (READ - GET ALL)
        [HttpGet]
        public ActionResult<IEnumerable<OrdenModel>> GetAll()
        {
            List<OrdenModel> ordenes = new List<OrdenModel>();
            string query = @"SELECT ORD_ORDENID, ORD_PROVEEDOR, ORD_FECHAORDEN, ORD_MONTOTOTAL, ORD_ESTADO, ORD_CREADOPOR, ORD_APROBADOPOR FROM COM_ORDENES";

            using (SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    OrdenModel orden = new OrdenModel
                    {
                        ORD_ORDENID = (int)reader["ORD_ORDENID"],
                        ORD_PROVEEDOR = reader["ORD_PROVEEDOR"].ToString(),
                        ORD_FECHAORDEN = (DateTime)reader["ORD_FECHAORDEN"],
                        ORD_MONTOTOTAL = (decimal)reader["ORD_MONTOTOTAL"],
                        ORD_ESTADO = reader["ORD_ESTADO"].ToString(),
                        ORD_CREADOPOR = reader["ORD_CREADOPOR"].ToString(),
                        ORD_APROBADOPOR = reader["ORD_APROBADOPOR"].ToString()
                    };
                    ordenes.Add(orden);
                }
            }
            return Ok(ordenes);
        }



        [HttpGet("FiltrarPorFecha")]
        public ActionResult<IEnumerable<OrdenModel>> GetByDateRangeAndProveedor(string fechaInicio, string fechaFin, string proveedor = "Todos")
        {
            if (string.IsNullOrEmpty(fechaInicio) || string.IsNullOrEmpty(fechaFin))
            {
                return BadRequest("Debe proporcionar ambas fechas: fechaInicio y fechaFin.");
            }

            List<OrdenModel> ordenes = new List<OrdenModel>();

            // Consulta SQL con filtro opcional por proveedor
            string query = @"SELECT ORD_ORDENID, ORD_PROVEEDOR, ORD_FECHAORDEN, ORD_MONTOTOTAL, ORD_ESTADO, ORD_CREADOPOR, ORD_APROBADOPOR 
                     FROM COM_ORDENES 
                     WHERE ORD_FECHAORDEN BETWEEN @FechaInicio AND @FechaFin";

            // Si no se selecciona "Todos", añadimos la cláusula para filtrar por proveedor
            if (!string.IsNullOrEmpty(proveedor) && proveedor != "Todos")
            {
                query += " AND ORD_PROVEEDOR = @Proveedor";
            }

            using (SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FechaInicio", DateTime.Parse(fechaInicio));
                command.Parameters.AddWithValue("@FechaFin", DateTime.Parse(fechaFin));

                if (!string.IsNullOrEmpty(proveedor) && proveedor != "Todos")
                {
                    command.Parameters.AddWithValue("@Proveedor", proveedor);
                }

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    OrdenModel orden = new OrdenModel
                    {
                        ORD_ORDENID = (int)reader["ORD_ORDENID"],
                        ORD_PROVEEDOR = reader["ORD_PROVEEDOR"].ToString(),
                        ORD_FECHAORDEN = (DateTime)reader["ORD_FECHAORDEN"],
                        ORD_MONTOTOTAL = (decimal)reader["ORD_MONTOTOTAL"],
                        ORD_ESTADO = reader["ORD_ESTADO"].ToString(),
                        ORD_CREADOPOR = reader["ORD_CREADOPOR"].ToString(),
                        ORD_APROBADOPOR = reader["ORD_APROBADOPOR"].ToString()
                    };
                    ordenes.Add(orden);
                }
            }

            return Ok(ordenes);
        }



        // Método para obtener órdenes de compra filtradas por rango de fechas (NEW - GET by date range)
        //[HttpGet("FiltrarPorFecha")]
        //public ActionResult<IEnumerable<OrdenModel>> GetByDateRange(string fechaInicio, string fechaFin)
        //{
        //    if (string.IsNullOrEmpty(fechaInicio) || string.IsNullOrEmpty(fechaFin))
        //    {
        //        return BadRequest("Debe proporcionar ambas fechas: fechaInicio y fechaFin.");
        //    }

        //    List<OrdenModel> ordenes = new List<OrdenModel>();

        //    // Consulta SQL que filtra por el rango de fechas
        //    string query = @"SELECT ORD_ORDENID, ORD_PROVEEDOR, ORD_FECHAORDEN, ORD_MONTOTOTAL, ORD_ESTADO, ORD_CREADOPOR, ORD_APROBADOPOR 
        //             FROM COM_ORDENES 
        //             WHERE ORD_FECHAORDEN BETWEEN @FechaInicio AND @FechaFin";

        //    using (SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]))
        //    {
        //        SqlCommand command = new SqlCommand(query, connection);
        //        command.Parameters.AddWithValue("@FechaInicio", DateTime.Parse(fechaInicio));
        //        command.Parameters.AddWithValue("@FechaFin", DateTime.Parse(fechaFin));

        //        connection.Open();
        //        SqlDataReader reader = command.ExecuteReader();

        //        while (reader.Read())
        //        {
        //            OrdenModel orden = new OrdenModel
        //            {
        //                ORD_ORDENID = (int)reader["ORD_ORDENID"],
        //                ORD_PROVEEDOR = reader["ORD_PROVEEDOR"].ToString(),
        //                ORD_FECHAORDEN = (DateTime)reader["ORD_FECHAORDEN"],
        //                ORD_MONTOTOTAL = (decimal)reader["ORD_MONTOTOTAL"],
        //                ORD_ESTADO = reader["ORD_ESTADO"].ToString(),
        //                ORD_CREADOPOR = reader["ORD_CREADOPOR"].ToString(),
        //                ORD_APROBADOPOR = reader["ORD_APROBADOPOR"].ToString()
        //            };
        //            ordenes.Add(orden);
        //        }
        //    }

        //    return Ok(ordenes);
        //}


        // Método para obtener una orden de compra por ID (READ - GET by ID)
        [HttpGet("{id}")]
        public ActionResult<OrdenModel> GetById(int id)
        {
            OrdenModel orden = null;
            string query = @"SELECT ORD_ORDENID, ORD_PROVEEDOR, ORD_FECHAORDEN, ORD_MONTOTOTAL, ORD_ESTADO, ORD_COMENTARIO, ORD_CREADOPOR, ORD_APROBADOPOR 
                             FROM COM_ORDENES WHERE ORD_ORDENID = @ORD_ORDENID";

            using (SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ORD_ORDENID", id);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    orden = new OrdenModel
                    {
                        ORD_ORDENID = (int)reader["ORD_ORDENID"],
                        ORD_PROVEEDOR = reader["ORD_PROVEEDOR"].ToString(),
                        ORD_FECHAORDEN = (DateTime)reader["ORD_FECHAORDEN"],
                        ORD_MONTOTOTAL = (decimal)reader["ORD_MONTOTOTAL"],
                        ORD_ESTADO = reader["ORD_ESTADO"].ToString(),
                        ORD_CREADOPOR = reader["ORD_CREADOPOR"].ToString(),
                        ORD_APROBADOPOR = reader["ORD_APROBADOPOR"].ToString()
                    };
                }
            }

            if (orden == null)
            {
                return NotFound("Orden de compra no encontrada.");
            }
            return Ok(orden);
        }

        // Método para actualizar una orden de compra (UPDATE)
        [HttpPut("{id}")]
        public ActionResult Update(int id, OrdenModel orden)
        {
            if (orden == null || id != orden.ORD_ORDENID)
            {
                return BadRequest("Datos inválidos.");
            }

            ClassConect classconect = new ClassConect();

            string query = @"UPDATE COM_ORDENES SET ORD_PROVEEDOR = @ORD_PROVEEDOR, ORD_MONTOTOTAL = @ORD_MONTOTOTAL, ORD_ESTADO = @ORD_ESTADO, 
                             ORD_APROBADOPOR = @ORD_APROBADOPOR WHERE ORD_ORDENID = @ORD_ORDENID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ORD_PROVEEDOR", orden.ORD_PROVEEDOR),
                new SqlParameter("@ORD_MONTOTOTAL", orden.ORD_MONTOTOTAL),
                new SqlParameter("@ORD_ESTADO", orden.ORD_ESTADO),
                new SqlParameter("@ORD_APROBADOPOR", orden.ORD_APROBADOPOR ?? ""), // Puede estar vacío si no ha sido aprobada
                new SqlParameter("@ORD_ORDENID", orden.ORD_ORDENID)
            };

            var resultado = classconect.CUDQuery(query, parameters, _configuration["ConnectionStrings:conexion1"]);

            if (resultado)
            {
                return Ok("Orden de compra actualizada correctamente.");
            }
            else
            {
                return BadRequest("Error al actualizar la orden de compra.");
            }
        }

        // Método para eliminar una orden de compra (DELETE)
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            ClassConect classconect = new ClassConect();

            string query = @"DELETE FROM COM_ORDENES WHERE ORD_ORDENID = @ORD_ORDENID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ORD_ORDENID", id)
            };

            var resultado = classconect.CUDQuery(query, parameters, _configuration["ConnectionStrings:conexion1"]);

            if (resultado)
            {
                return Ok("Orden de compra eliminada correctamente.");
            }
            else
            {
                return BadRequest("Error al eliminar la orden de compra.");
            }
        }

        [HttpGet("DescargarPDF/{id}")]
        public IActionResult DescargarPDF(int id)
        {
            // Obtener los datos de la orden y su detalle
            var orden = ObtenerOrden(id);
            var detallesOrden = ObtenerDetallesOrden(id);

            if (orden == null)
            {
                return NotFound("Orden no encontrada");
            }

            // Crear un documento PDF
            PdfDocument pdf = new PdfDocument();
            PdfPage page = pdf.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Definir fuentes y estilos
            XFont titleFont = new XFont("Arial", 20, XFontStyle.Bold);
            XFont headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            XFont bodyFont = new XFont("Arial", 10);
            XBrush headerBrush = XBrushes.White;
            XBrush headerBrush2 = XBrushes.Black;
            XBrush bodyBrush = XBrushes.Black;

            // Fondo del encabezado
            gfx.DrawRectangle(XBrushes.LightGray, 0, 0, page.Width, 40);
            gfx.DrawString($"Detalle de la Orden #{orden.ORD_ORDENID}", titleFont, headerBrush2, new XRect(0, 10, page.Width, 40), XStringFormats.TopCenter);

            // Información de la orden
            gfx.DrawString($"Proveedor: {orden.ORD_PROVEEDOR}", headerFont, bodyBrush, new XRect(20, 50, page.Width - 40, 20), XStringFormats.TopLeft);
            gfx.DrawString($"Monto Total: {orden.ORD_MONTOTOTAL:C}", headerFont, bodyBrush, new XRect(20, 70, page.Width - 40, 20), XStringFormats.TopLeft);
            gfx.DrawString($"Estado: {orden.ORD_ESTADO}", headerFont, bodyBrush, new XRect(20, 90, page.Width - 40, 20), XStringFormats.TopLeft);
            gfx.DrawString($"Generado Por: {orden.ORD_CREADOPOR}", headerFont, bodyBrush, new XRect(20, 110, page.Width - 40, 20), XStringFormats.TopLeft);
            gfx.DrawString($"Autorizado Por: {orden.ORD_APROBADOPOR}", headerFont, bodyBrush, new XRect(20, 130, page.Width - 40, 20), XStringFormats.TopLeft);

            // Encabezado de la tabla de detalles
            gfx.DrawRectangle(XBrushes.DarkBlue, 20, 150, page.Width - 40, 25);
            gfx.DrawString("Producto", headerFont, headerBrush, new XRect(30, 155, page.Width - 40, 25), XStringFormats.TopLeft);
            gfx.DrawString("Cantidad", headerFont, headerBrush, new XRect(200, 155, page.Width - 40, 25), XStringFormats.TopLeft);
            gfx.DrawString("Precio Unidad", headerFont, headerBrush, new XRect(300, 155, page.Width - 40, 25), XStringFormats.TopLeft);
            gfx.DrawString("Subtotal", headerFont, headerBrush, new XRect(450, 155, page.Width - 40, 25), XStringFormats.TopLeft);

            // Añadir los detalles de la orden
            int yPoint = 180;
            decimal totalGeneral = 0; // Variable para almacenar la suma de los subtotales
            bool gris = true; // Para alternar el color de fondo de las filas

            foreach (var detalle in detallesOrden)
            {
                // Alternar fondo de las filas
                var backgroundBrush = gris ? XBrushes.LightGray : XBrushes.White;
                gris = !gris; // Cambiar el valor de gris para la siguiente fila

                gfx.DrawRectangle(backgroundBrush, 20, yPoint, page.Width - 40, 25);

                gfx.DrawString(detalle.DET_PRODUCTO, bodyFont, bodyBrush, new XRect(30, yPoint + 5, page.Width - 40, 25), XStringFormats.TopLeft);
                gfx.DrawString(detalle.DET_CANTIDAD.ToString(), bodyFont, bodyBrush, new XRect(200, yPoint + 5, page.Width - 40, 25), XStringFormats.TopLeft);
                gfx.DrawString($"{detalle.DET_PRECIOUNITARIO:C}", bodyFont, bodyBrush, new XRect(300, yPoint + 5, page.Width - 40, 25), XStringFormats.TopLeft);
                gfx.DrawString($"{detalle.DET_SUBTOTAL:C}", bodyFont, bodyBrush, new XRect(450, yPoint + 5, page.Width - 40, 25), XStringFormats.TopLeft);

                // Sumar el subtotal al total general
                totalGeneral += detalle.DET_SUBTOTAL;

                yPoint += 25; // Avanzar para la siguiente fila
            }

            // Añadir línea final con el total a pagar
            gfx.DrawLine(XPens.Black, 20, yPoint + 10, page.Width - 20, yPoint + 10); // Dibujar una línea
            gfx.DrawString($"Total a Pagar: {totalGeneral:C}", headerFont, bodyBrush, new XRect(380, yPoint + 20, page.Width - 40, 25), XStringFormats.TopLeft);

            // Guardar el documento en un MemoryStream y devolver como archivo
            using (var memoryStream = new MemoryStream())
            {
                pdf.Save(memoryStream);
                return File(memoryStream.ToArray(), "application/pdf", $"orden_{id}.pdf");
            }
        }

        [HttpGet("DescargarExcel/{id}")]
        public IActionResult DescargarExcel(int id)
        {
            // Obtener los datos de la orden y su detalle
            var orden = ObtenerOrden(id);
            var detallesOrden = ObtenerDetallesOrden(id);

            if (orden == null)
            {
                return NotFound("Orden no encontrada");
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Detalle de Orden");

                // Encabezado principal "Detalle de la Orden" con estilo (fondo gris claro y texto negro)
                worksheet.Cell(1, 1).Value = $"Detalle de la Orden #{orden.ORD_ORDENID}";
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.Black;  // Letras negras
                worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;  // Fondo gris claro
                worksheet.Range(1, 1, 1, 4).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Información de la orden (sin fondo azul)
                worksheet.Cell(3, 1).Value = "Proveedor:";
                worksheet.Cell(4, 1).Value = "Monto Total:";
                worksheet.Cell(5, 1).Value = "Estado:";
                worksheet.Cell(6, 1).Value = "Generado Por:";
                worksheet.Cell(7, 1).Value = "Autorizado Por:";
                worksheet.Cell(3, 2).Value = orden.ORD_PROVEEDOR;
                worksheet.Cell(4, 2).Value = orden.ORD_MONTOTOTAL.ToString("C");
                worksheet.Cell(5, 2).Value = orden.ORD_ESTADO;
                worksheet.Cell(6, 2).Value = orden.ORD_CREADOPOR;
                worksheet.Cell(7, 2).Value = orden.ORD_APROBADOPOR;

                // Encabezados de la tabla de detalles con fondo azul y texto blanco
                worksheet.Cell(9, 1).Value = "Producto";
                worksheet.Cell(9, 2).Value = "Cantidad";
                worksheet.Cell(9, 3).Value = "Precio Unidad";
                worksheet.Cell(9, 4).Value = "Subtotal";

                worksheet.Range(9, 1, 9, 4).Style.Font.Bold = true;
                worksheet.Range(9, 1, 9, 4).Style.Font.FontColor = XLColor.White;
                worksheet.Range(9, 1, 9, 4).Style.Fill.BackgroundColor = XLColor.DarkBlue;

                // Añadir detalles de la orden
                int currentRow = 10;
                decimal totalGeneral = 0;
                foreach (var detalle in detallesOrden)
                {
                    worksheet.Cell(currentRow, 1).Value = detalle.DET_PRODUCTO;
                    worksheet.Cell(currentRow, 2).Value = detalle.DET_CANTIDAD;
                    worksheet.Cell(currentRow, 3).Value = detalle.DET_PRECIOUNITARIO;
                    worksheet.Cell(currentRow, 4).Value = detalle.DET_SUBTOTAL;

                    totalGeneral += detalle.DET_SUBTOTAL;
                    currentRow++;
                }

                // Añadir total a pagar con estilo azul
                worksheet.Cell(currentRow, 3).Value = "Total a Pagar:";
                worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 3).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.DarkBlue;

                worksheet.Cell(currentRow, 4).Value = totalGeneral.ToString("C");
                //worksheet.Cell(currentRow, 4).Style.Font.Bold = true;
                //worksheet.Cell(currentRow, 4).Style.Font.FontColor = XLColor.White;
                //worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.DarkBlue;

                // Ajustar ancho de columnas automáticamente
                worksheet.Columns().AdjustToContents();

                // Guardar el archivo Excel en un MemoryStream
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"orden_{id}.xlsx");
                }
            }
        }

        private OrdenModel ObtenerOrden(int id)
        {
            OrdenModel orden = null;

            string query = @"SELECT ORD_ORDENID, ORD_PROVEEDOR, ORD_FECHAORDEN, ORD_MONTOTOTAL, ORD_ESTADO, ORD_CREADOPOR, ORD_APROBADOPOR 
                     FROM COM_ORDENES 
                     WHERE ORD_ORDENID = @id";

            using (SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    orden = new OrdenModel
                    {
                        ORD_ORDENID = (int)reader["ORD_ORDENID"],
                        ORD_PROVEEDOR = reader["ORD_PROVEEDOR"].ToString(),
                        ORD_FECHAORDEN = (DateTime)reader["ORD_FECHAORDEN"],
                        ORD_MONTOTOTAL = (decimal)reader["ORD_MONTOTOTAL"],
                        ORD_ESTADO = reader["ORD_ESTADO"].ToString(),
                        ORD_CREADOPOR = reader["ORD_CREADOPOR"].ToString(),
                        ORD_APROBADOPOR = reader["ORD_APROBADOPOR"].ToString()
                    };
                }
            }

            return orden;
        }

        private List<DetalleOrdenModel> ObtenerDetallesOrden(int ordenId)
        {
            List<DetalleOrdenModel> detallesOrden = new List<DetalleOrdenModel>();

            string query = @"SELECT DET_DETALLEORDENID, DET_PRODUCTO, DET_CANTIDAD, DET_PRECIOUNITARIO, DET_SUBTOTAL 
                     FROM COM_DETALLESORDEN 
                     WHERE DET_ORDEN = @ordenId";

            using (SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:conexion1"]))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ordenId", ordenId);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    detallesOrden.Add(new DetalleOrdenModel
                    {
                        DET_DETALLEORDENID = (int)reader["DET_DETALLEORDENID"],
                        DET_PRODUCTO = reader["DET_PRODUCTO"].ToString(),
                        DET_CANTIDAD = (int)reader["DET_CANTIDAD"],
                        DET_PRECIOUNITARIO = (decimal)reader["DET_PRECIOUNITARIO"],
                        DET_SUBTOTAL = (decimal)reader["DET_SUBTOTAL"]
                    });
                }
            }

            return detallesOrden;
        }


    }
}


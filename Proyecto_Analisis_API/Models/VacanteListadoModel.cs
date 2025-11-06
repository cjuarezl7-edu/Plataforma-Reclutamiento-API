namespace Proyecto_Analisis_API.Models
{
    public class VacanteListadoModel
    {
        public int VAC_CODIGO_VACANTE { get; set; }
        public int? VAC_CODIGO_AREA { get; set; }
        public string? NOMBRE_AREA { get; set; }

        public string? VAC_TITULO { get; set; }
        public string? VAC_DESCRIPCION { get; set; }
        public string? VAC_REQUISITOS { get; set; }
        public string? VAC_OFRECIMIENTO { get; set; } // o VAC_OFRECEMINETO si así quedó la columna
        public string? VAC_REQUERIMIENTOS { get; set; }
        public string? VAC_URL_IMAGEN { get; set; }

        public DateTime? VAC_FECHA_CREACION { get; set; }
        public DateTime? VAC_FECHA_CIERRE { get; set; }
        public DateTime? VAC_FECHA_MODIFICACION { get; set; }

        public int? CEV_ESTADO_VACANTE { get; set; }
        public string? ESTADO_VACANTE { get; set; } // cuando traes el nombre (GetAll)
        public string? USUARIO_CREACION { get; set; }
    }
}

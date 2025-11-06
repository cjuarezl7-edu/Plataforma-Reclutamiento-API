namespace Proyecto_Analisis_API.Models
{
    public class VacanteModel
    {
        public int VAC_CODIGO_VACANTE { get; set; }
        public int VAC_CODIGO_AREA { get; set; }

        public string? VAC_TITULO { get; set; }
        public string? VAC_DESCRIPCION { get; set; }

        // NUEVOS CAMPOS
        public string? VAC_REQUISITOS { get; set; }
        public string? VAC_OFRECIMIENTO { get; set; } // <-- si tu columna es VAC_OFRECEMINETO, cámbialo aquí
        public string? VAC_REQUERIMIENTOS { get; set; }
        public string? VAC_URL_IMAGEN { get; set; }

        public DateTime VAC_FECHA_CREACION { get; set; }
        public DateTime? VAC_FECHA_CIERRE { get; set; }  // en tabla es NULL → nullable
        public DateTime? VAC_FECHA_MODIFICACION { get; set; }  // nueva → nullable

        public int CEV_ESTADO_VACANTE { get; set; }

        public int? VAC_USUARIO_CREACION { get; set; }  // en tabla es NULL → nullable
        public int? VAC_USUARIO_MODIFICACION { get; set; }  // nueva → nullable
    }
}

namespace Proyecto_Analisis_API.Models
{
    public class OrdenModel
    {
        public int ORD_ORDENID { get; set; }
        public string ORD_PROVEEDOR { get; set; }
        public DateTime ORD_FECHAORDEN { get; set; }
        public decimal ORD_MONTOTOTAL { get; set; }
        public string ORD_ESTADO { get; set; }
        public string ORD_CREADOPOR { get; set; }
        public string ORD_APROBADOPOR { get; set; }

    }
}

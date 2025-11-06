namespace Proyecto_Analisis_API.Models
{
    public class DetalleOrdenModel
    {
        public int DET_DETALLEORDENID { get; set; }       
        public string DET_ORDEN { get; set; }
        public string DET_PRODUCTO { get; set; }
        public int DET_CANTIDAD { get; set; }
        public decimal DET_PRECIOUNITARIO { get; set; }
        public decimal DET_SUBTOTAL { get; set; }
    }
}

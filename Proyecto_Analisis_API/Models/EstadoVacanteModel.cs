namespace Proyecto_Analisis_API.Models
{
    public class EstadoVacanteModel
    {
        public int CEV_CODIGO_ESTADO_VACANTE { get; set; }
        public string? CEV_NOMBRE { get; set; }
        public string? CEV_COMENTARIO { get; set; }
        public Boolean CEV_ESTADO { get; set; }
        public int CEV_USUARIO_CREACION { get; set; }
        public DateTime CEV_FECHA_CREACION { get; set; }
    }
}

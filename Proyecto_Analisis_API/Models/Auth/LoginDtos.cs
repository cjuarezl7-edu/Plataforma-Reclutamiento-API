namespace Proyecto_Analisis_API.Models.Auth
{
    public class LoginDtos
    {
    }
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public int UsuarioId { get; set; }
        public int EmpleadoId { get; set; }
        public int RolId { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
    }
}

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat11_AuthSetPassword.DTOs{
    public class SetPasswordRequestDto
    {
        public string? Email { get; set; }  // Optional: for updating email
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}









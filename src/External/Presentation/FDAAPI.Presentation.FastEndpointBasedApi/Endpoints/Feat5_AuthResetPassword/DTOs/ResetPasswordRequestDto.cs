namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat5_AuthResetPassword.DTOs
{
    public class ResetPasswordRequestDto
    {
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

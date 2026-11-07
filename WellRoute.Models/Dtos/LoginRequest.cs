using System.ComponentModel.DataAnnotations;

namespace WellRoute.Models.Dtos
{
	public class LoginRequest
	{
		[Required(ErrorMessage = "El correo es obligatorio.")]
		[EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "La contraseña es obligatoria.")]
		public string Password { get; set; } = string.Empty;
	}
}
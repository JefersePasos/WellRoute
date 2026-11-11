using System.ComponentModel.DataAnnotations;

namespace WellRoute.Web.Models;

public sealed class CambiarContrasenaViewModel
{
    [Required(ErrorMessage = "Ingresa tu contraseña temporal.")]
    [StringLength(1024, ErrorMessage = "La contraseña temporal es demasiado larga.")]
    [Display(Name = "Contraseña temporal")]
    public string ContrasenaActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa una contraseña nueva.")]
    [StringLength(128, MinimumLength = 12,
        ErrorMessage = "Usa entre 12 y 128 caracteres.")]
    [Display(Name = "Nueva contraseña")]
    public string ContrasenaNueva { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la contraseña nueva.")]
    [Compare(nameof(ContrasenaNueva), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string Confirmacion { get; set; } = string.Empty;
}
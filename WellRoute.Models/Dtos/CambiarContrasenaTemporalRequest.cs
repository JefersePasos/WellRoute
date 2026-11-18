using System.ComponentModel.DataAnnotations;

namespace WellRoute.Models.Dtos;

public sealed class CambiarContrasenaTemporalRequest
{
    [Required]
    [StringLength(1024)]
    public string ContrasenaActual { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 12)]
    public string ContrasenaNueva { get; set; } = string.Empty;
}

public enum EstadoCambioContrasena
{
    Exitoso,
    DatosInvalidos,
    CuentaNoDisponible,
    CuentaBloqueada,
    CambioNoPendiente
}
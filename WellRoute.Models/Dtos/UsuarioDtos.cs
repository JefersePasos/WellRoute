using System.ComponentModel.DataAnnotations;

namespace WellRoute.Models.Dtos;

public sealed class UsuarioListaDto
{
    public int UsuarioID { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public DateTime? FechaUltimoLogin { get; init; }
    public List<string> Roles { get; init; } = new();
}

public sealed class UsuarioDetalleDto
{
    public int UsuarioID { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public bool Activo { get; init; }
    public bool DebeCambiarContrasena { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaUltimoLogin { get; init; }
    public List<int> RolIds { get; init; } = new();
}

public sealed class GuardarUsuarioRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefono { get; set; }

    public List<int> RolIds { get; set; } = new();
}

// Se devuelve al crear un usuario o restablecer su contraseña. La contraseña solo se muestra esa vez.
public sealed class ContrasenaTemporalDto
{
    public int UsuarioID { get; init; }
    public string Email { get; init; } = string.Empty;
    public string ContrasenaTemporal { get; init; } = string.Empty;
}

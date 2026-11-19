using System.ComponentModel.DataAnnotations;

namespace WellRoute.Models.Dtos;

public sealed class RolListaDto
{
    public int RolID { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public bool Activo { get; init; }
    public int CantidadUsuarios { get; init; }
    public int CantidadPermisos { get; init; }
}

public sealed class RolDetalleDto
{
    public int RolID { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public bool Activo { get; init; }
    public List<int> PermisoIds { get; init; } = new();
}

public sealed class PermisoDto
{
    public int PermisoID { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
}

public sealed class GuardarRolRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    public List<int> PermisoIds { get; set; } = new();
}

using System.ComponentModel.DataAnnotations;

namespace WellRoute.Models.Dtos;

// Categorías disponibles para un destino (coinciden con el formato de intercambio entre grupos).
public static class CategoriasDestino
{
    public static readonly string[] Todas = { "Cultura", "Playa", "Aventura", "Gastronomía" };
}

// Lo que necesita el collage de la portada y la lista de gestión.
public sealed class DestinoListaDto
{
    public int DestinoID { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Pais { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public string ImagenUrl { get; init; } = string.Empty;
    public bool Activo { get; init; }
}

public sealed class DestinoDetalleDto
{
    public int DestinoID { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Pais { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string ImagenUrl { get; init; } = string.Empty;
    public string? VideoUrl { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
}

public sealed class GuardarDestinoRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país es obligatorio.")]
    [StringLength(100)]
    public string Pais { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(50)]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La URL de la imagen es obligatoria.")]
    [StringLength(500)]
    public string ImagenUrl { get; set; } = string.Empty;

    [StringLength(500)]
    public string? VideoUrl { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WellRoute.Models.Dtos;

namespace WellRoute.Web.Models;

public sealed class DestinoFormViewModel
{
    public int? DestinoID { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "País")]
    public string Pais { get; set; } = string.Empty;

    [Required(ErrorMessage = "Elige una categoría.")]
    [Display(Name = "Categoría")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(2000, ErrorMessage = "Máximo 2000 caracteres.")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La URL de la imagen es obligatoria.")]
    [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    [Url(ErrorMessage = "Escribe una URL válida que empiece con http:// o https://.")]
    [Display(Name = "URL de la imagen")]
    public string ImagenUrl { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    [Url(ErrorMessage = "Escribe una URL válida que empiece con http:// o https://.")]
    [Display(Name = "URL del video (opcional)")]
    public string? VideoUrl { get; set; }

    [BindNever]
    [ValidateNever]
    public string[] CategoriasDisponibles { get; set; } = CategoriasDestino.Todas;
}

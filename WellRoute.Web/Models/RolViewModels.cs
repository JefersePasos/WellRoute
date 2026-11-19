using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WellRoute.Models.Dtos;

namespace WellRoute.Web.Models;

public sealed class RolFormViewModel
{
    public int? RolID { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Máximo 255 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public List<int> PermisoIds { get; set; } = new();

    [BindNever]
    [ValidateNever]
    public List<PermisoDto> PermisosDisponibles { get; set; } = new();

    // El rol Administrador es de solo lectura.
    [BindNever]
    [ValidateNever]
    public bool EsAdministrador { get; set; }
}

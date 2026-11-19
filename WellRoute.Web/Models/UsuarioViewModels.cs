using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WellRoute.Models.Dtos;

namespace WellRoute.Web.Models;

public sealed class UsuarioFormViewModel
{
    public int? UsuarioID { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    public List<int> RolIds { get; set; } = new();

    [BindNever]
    [ValidateNever]
    public List<RolListaDto> RolesDisponibles { get; set; } = new();
}

public sealed class ContrasenaTemporalViewModel
{
    public string Titulo { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;
}

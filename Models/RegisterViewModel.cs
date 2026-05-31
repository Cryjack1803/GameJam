using System.ComponentModel.DataAnnotations;

namespace Tarea_01.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ingresa tu nombre completo.")]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu correo.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo valido.")]
    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa una contrasena.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contrasena")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contrasena.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contrasenas no coinciden.")]
    [Display(Name = "Confirmar contrasena")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
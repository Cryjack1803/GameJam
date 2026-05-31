using System.ComponentModel.DataAnnotations;

namespace Tarea_01.Models;

public class ContactMessageViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingresa tu nombre.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu correo.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo valido.")]
    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona el area de contacto.")]
    [Display(Name = "Area")]
    public string Area { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona una prioridad.")]
    [Display(Name = "Prioridad")]
    public string Priority { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el asunto.")]
    [StringLength(80, ErrorMessage = "El asunto no debe superar los 80 caracteres.")]
    [Display(Name = "Asunto")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Describe tu mensaje.")]
    [StringLength(500, ErrorMessage = "El mensaje no debe superar los 500 caracteres.")]
    [Display(Name = "Mensaje")]
    public string Message { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }
}
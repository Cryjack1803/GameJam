using Microsoft.AspNetCore.Identity;

namespace Tarea_01.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
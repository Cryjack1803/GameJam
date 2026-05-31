namespace Tarea_01.Models;

public class SessionViewModel
{
    public string UserName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string AuthenticatedAt { get; set; } = string.Empty;

    public DateTime? StartedAt { get; set; }

    public bool IsPersistent { get; set; }
}
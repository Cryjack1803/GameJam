namespace Tarea_01.Models;

public class ContactPageViewModel
{
    public ContactMessageViewModel Form { get; set; } = new();

    public List<ContactMessageViewModel> Messages { get; set; } = new();

    public int TotalMessages { get; set; }

    public int HighPriorityMessages { get; set; }

    public int ClientMessages { get; set; }
}
using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;

namespace Tarea_01.Controllers;

public class ContactController : Controller
{
    private static readonly List<ContactMessageViewModel> Messages = new();

    [HttpGet]
    public IActionResult Index()
    {
        return View(BuildViewModel(new ContactMessageViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ContactMessageViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(BuildViewModel(form));
        }

        form.Id = Messages.Count + 1;
        form.SentAt = DateTime.Now;
        Messages.Insert(0, form);

        TempData["ContactMessage"] = "Tu mensaje fue enviado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private static ContactPageViewModel BuildViewModel(ContactMessageViewModel form)
    {
        return new ContactPageViewModel
        {
            Form = form,
            Messages = Messages.Take(5).ToList(),
            TotalMessages = Messages.Count,
            HighPriorityMessages = Messages.Count(message => message.Priority == "Alta"),
            ClientMessages = Messages.Count(message => message.Area == "Cliente")
        };
    }
}
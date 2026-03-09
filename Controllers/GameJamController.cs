using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;
using System.Collections.Generic;
using System.Linq;

namespace Tarea_01.Controllers
{
    public class GameJamController : Controller
    {
        // Simulación de almacenamiento temporal
        private static List<GameJamViewModel> personasRegistradas = new List<GameJamViewModel>();

        [HttpGet]
        public IActionResult Register()
        {
            return View(new GameJamViewModel());
        }

        [HttpPost]
        public IActionResult Register(GameJamViewModel model)
        {
            if (ModelState.IsValid)
            {
                personasRegistradas.Add(model);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult List()
        {
            return View(personasRegistradas);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Practica02MediWebMVC.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // Solo el administrador gestiona usuarios y configuraciones
    [Authorize(Roles = "administrador")]
    public IActionResult Administracion()
    {
        return View();
    }

    // Médicos y administradores pueden ver expedientes y consultas
    [Authorize(Roles = "administrador, medico")]
    public IActionResult Consultas()
    {
        return View();
    }

    // Recepcionistas y administradores gestionan la agenda diaria
    [Authorize(Roles = "administrador, recepcionista")]
    public IActionResult Recepcion()
    {
        return View();
    }
}
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica02MediWebMVC.Data;
using Practica02MediWebMVC.Models.ViewModels;

namespace Practica02MediWebMVC.Controllers;

public class CuentaController : Controller
{
    private readonly UserRepository _userRepository;

    public CuentaController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userRepository.GetByEmailAsync(model.Email);

        if (user is null || !user.Active)
        {
            // Mensaje genérico por seguridad
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }

        bool passwordOk;
        try
        {
            passwordOk = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
        }
        catch
        {
            passwordOk = false;
        }

        if (!passwordOk)
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }

        // Creación de Claims con los datos del usuario y su rol
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            // Convertimos el rol a minúsculas (ej. "Administrador" -> "administrador")
            new(ClaimTypes.Role, user.Role.ToLowerInvariant())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult Ping()
    {
        return NoContent(); // Mantiene viva la sesión mientras haya actividad
    }

    [AllowAnonymous]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
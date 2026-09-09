using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;
using Practica02MediWebMVC.Data;
using Practica02MediWebMVC.Models;

namespace Practica02MediWebMVC.Controllers;

[Authorize]
public class CitasController : Controller
{
    private readonly CitaRepository _repository;

    public CitasController(CitaRepository repository)
    {
        _repository = repository;
    }

    // READ: Todos los usuarios autenticados pueden ver el listado.
    public async Task<IActionResult> Index()
    {
        var citas = await _repository.GetAllAsync();
        return View(citas);
    }

    public async Task<IActionResult> Details(int id)
    {
        var cita = await _repository.GetByIdAsync(id);
        if (cita is null) return NotFound();
        return View(cita);
    }

    // CREATE: Administrador y Recepcionista (equivalente a Capataz).
    [Authorize(Roles = "administrador, recepcionista")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CargarCatalogosAsync();
        // Asignamos el estado 1 (Ej: "Programada") por defecto
        return View(new Cita { EstadoId = 1, FechaHora = DateTime.Now });
    }

    [Authorize(Roles = "administrador, recepcionista")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cita cita)
    {
        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync(cita);
            return View(cita);
        }

        try
        {
            int id = await _repository.CreateAsync(cita);
            TempData["Mensaje"] = "Cita registrada correctamente.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            // En caso de error general de base de datos
            ModelState.AddModelError("", "Ocurrió un error al guardar la cita: " + ex.Message);
            await CargarCatalogosAsync(cita);
            return View(cita);
        }
    }

    // UPDATE: Administrador y Médico (equivalente a Veterinario).
    [Authorize(Roles = "administrador, medico")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var cita = await _repository.GetByIdAsync(id);
        if (cita is null) return NotFound();

        await CargarCatalogosAsync(cita);
        return View(cita);
    }

    [Authorize(Roles = "administrador, medico")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cita cita)
    {
        if (id != cita.CitaId) return BadRequest();

        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync(cita);
            return View(cita);
        }

        try
        {
            bool actualizado = await _repository.UpdateAsync(cita);
            if (!actualizado) return NotFound();

            TempData["Mensaje"] = "Cita actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Ocurrió un error al actualizar: " + ex.Message);
            await CargarCatalogosAsync(cita);
            return View(cita);
        }
    }

    // DELETE: Solamente Administrador.
    [Authorize(Roles = "administrador")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var cita = await _repository.GetByIdAsync(id);
        if (cita is null) return NotFound();
        return View(cita);
    }

    [Authorize(Roles = "administrador")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            bool eliminado = await _repository.DeleteAsync(id);
            if (!eliminado) return NotFound();

            TempData["Mensaje"] = "Cita eliminada correctamente.";
        }
        catch (MySqlException ex) when (ex.Number == 1451) // Captura error de llave foránea
        {
            TempData["Error"] = "No se puede eliminar porque existen registros relacionados.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Método auxiliar para cargar los dropdowns (ViewBags)
    private async Task CargarCatalogosAsync(Cita? cita = null)
    {
        ViewBag.Pacientes = new SelectList(
            await _repository.GetPacientesAsync(),
            "Id", "Nombre", cita?.PacienteId);

        ViewBag.Medicos = new SelectList(
            await _repository.GetMedicosAsync(),
            "Id", "Nombre", cita?.MedicoId);

        ViewBag.Consultorios = new SelectList(
            await _repository.GetConsultoriosAsync(),
            "Id", "Nombre", cita?.ConsultorioId);

        ViewBag.Estados = new SelectList(
            await _repository.GetEstadosAsync(),
            "Id", "Nombre", cita?.EstadoId);
    }
}
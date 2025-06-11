using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Services;

namespace ProyectoFinal.Controllers.Roles
{
    [Authorize(Policy = "CanManageRoles")]
    [Route("RoleUI")]
    public class RoleUIController : Controller
    {
        private readonly RoleService _roleService;

        public RoleUIController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("Index")] // Agregamos "Index" para ser más explícitos si es necesario
        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return View(roles);
        }

        // Acción para eliminar un rol (soft delete) desde el formulario en la tabla
        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "No se pudo encontrar el rol para eliminar.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Rol desactivado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // --- LAS ACCIONES ManageUserRoles (GET Y POST) HAN SIDO ELIMINADAS ---
        // Toda esa funcionalidad ahora es manejada por el modal en la vista Index
        // y las llamadas a tu API Controller (RolesController).
    }
}

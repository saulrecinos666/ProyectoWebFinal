﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Departments.Dto;

namespace ProyectoFinal.Controllers.Departaments
{
    [ApiController]
    [Route("api/departament")]
    [Authorize]
    public class DepartamentsController : BaseController
    {
        private readonly DbCitasMedicasContext _context;

        public DepartamentsController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDepartaments()
        {
            var departaments = await _context.Departments
                .Select(d => new ResponseDepartmentDto
                {
                    DepartmentCode = d.DepartmentCode,
                    DepartmentName = d.DepartmentName
                })
                .ToListAsync();

            return Ok(departaments);
        }
    }
}

using EmployeeCrudApi.Data;
using EmployeeCrudApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;

namespace EmployeeCrudApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<List<Employee>> GetAll()
        {
            return await _context.Employees.ToListAsync();
        }

        [HttpGet]
        public async Task<Employee> GetById(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        [HttpPost]
public async Task<IActionResult> Create([FromBody] Employee employee)
{
    // Verificar si el nombre contiene números
    if (Regex.IsMatch(employee.Name, @"\d"))
    {
        return BadRequest("El nombre no puede contener números.");
    }

    // Verificar si la longitud del nombre es menor a 2 caracteres
    if (employee.Name.Length < 2)
    {
        return BadRequest("El nombre debe tener al menos 2 caracteres.");
    }

    // Verificar si la longitud del nombre excede los 100 caracteres
    if (employee.Name.Length > 100)
    {
        return BadRequest("El nombre no puede tener más de 100 caracteres.");
    }

    // Formatear el nombre correctamente
    employee.Name = FormatName(employee.Name);

    // Verificar si ya existe un empleado con el mismo nombre (insensible a mayúsculas)
    var existingEmployee = await _context.Employees
        .AnyAsync(e => EF.Functions.Like(e.Name, employee.Name));

    if (existingEmployee)
    {
        return BadRequest("El nombre del empleado ya existe.");
    }

    // Establecer la fecha de creación
    employee.CreatedDate = DateTime.Now;

    // Añadir el nuevo empleado
    await _context.Employees.AddAsync(employee);
    await _context.SaveChangesAsync();

    return Ok(employee);
}

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Employee employee)
        {
            // Verificar si el nombre contiene números
            if (Regex.IsMatch(employee.Name, @"\d"))
            {
                return BadRequest("El nombre no puede contener números.");
            }

            // Verificar si la longitud del nombre es menor a 2 caracteres
            if (employee.Name.Length < 2)
            {
                return BadRequest("El nombre debe tener al menos 2 caracteres.");
            }

            // Verificar si la longitud del nombre excede los 100 caracteres
            if (employee.Name.Length > 100)
            {
                return BadRequest("El nombre no puede tener más de 100 caracteres.");
            }

            // Formatear el nombre correctamente
            employee.Name = FormatName(employee.Name);

            // Verificar si ya existe un empleado con el mismo nombre (exceptuando el actual)
            var existingEmployee = await _context.Employees
                .AnyAsync(e => e.Name == employee.Name && e.Id != employee.Id);

            if (existingEmployee)
            {
                return BadRequest("El nombre del empleado ya existe.");
            }

            var employeeToUpdate = await _context.Employees.FindAsync(employee.Id);

            if (employeeToUpdate == null)
            {
                return NotFound("El empleado no existe.");
            }

            employeeToUpdate.Name = employee.Name;
            await _context.SaveChangesAsync();

            return Ok(employeeToUpdate);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var employeeToDelete = await _context.Employees.FindAsync(id);

            if (employeeToDelete == null)
            {
                return NotFound("El empleado no existe.");
            }

            _context.Remove(employeeToDelete);
            await _context.SaveChangesAsync();

            return Ok("Empleado eliminado correctamente.");
        }

        // Función auxiliar para formatear el nombre
        private string FormatName(string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return fullName;

            // Formatear cada parte del nombre con la primera letra en mayúscula
            var firstNames = parts.Take(parts.Length - 1)
                                  .Select(p => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.ToLower()));

            // Convertir el apellido (última parte) a mayúsculas
            var lastName = parts.Last().ToUpper();

            // Unir todas las partes
            return string.Join(' ', firstNames) + " " + lastName;
        }
    }
}

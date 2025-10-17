using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Data;
using WarehouseAPI.Core.Data.Repositories;
using WarehouseAPI.Core.Models.DTOs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SimpleAuthorize("Director")] 
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeRepository _employeeRepository;
        private readonly ApplicationDbContext _context; 

        public EmployeesController(EmployeeRepository employeeRepository, ApplicationDbContext context)
        {
            _employeeRepository = employeeRepository;
            _context = context; 
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var employees = await _employeeRepository.GetAllAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return NotFound();
            return employee;
        }
        
        [HttpPost]
        //[Authorize(Roles = "Storekeeper")]
        public async Task<ActionResult<Employee>> PostEmployee(CreateEmployeeDto createDto)
        {
            // Check if login already exists
            if (await _context.Employees.AnyAsync(e => e.Login == createDto.Login))
            {
                return BadRequest("Employee with this login already exists.");
            }

            // Check if role exists
            var role = await _context.Roles.FindAsync(createDto.RoleId);
            if (role == null)
            {
                return BadRequest("Role not found.");
            }

            var employee = new Employee
            {
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                IsActive = createDto.IsActive,
                Login = createDto.Login,
                PasswordHash = createDto.PasswordHash,
                RoleId = createDto.RoleId
            };

            var createdEmployee = await _employeeRepository.CreateAsync(employee);
            return CreatedAtAction("GetEmployee", new { id = createdEmployee.Id }, createdEmployee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.Id) return BadRequest();
            await _employeeRepository.UpdateAsync(employee);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var result = await _employeeRepository.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
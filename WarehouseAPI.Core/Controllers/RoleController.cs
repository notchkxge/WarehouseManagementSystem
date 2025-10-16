using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Data;
using WarehouseAPI.Core.Models.DTOs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase{
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RolesController(ApplicationDbContext context, IConfiguration configuration){
            _context = context;
            _configuration = configuration;
        }

        // =======================
        // ROLE MANAGEMENT ENDPOINTS
        // =======================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles(){
            try{
                var roles = await _context.Roles.ToListAsync();
                return Ok(roles);
            }
            catch (Exception ex){
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<Role>> CreateRole([FromBody] Role role){
            try{
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                return Ok(role);
            }
            catch (Exception ex){
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id){
            try{
                var role = await _context.Roles.FindAsync(id);
                if (role == null) return NotFound();
                return Ok(role);
            }
            catch (Exception ex){
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // =======================
        // AUTHENTICATION ENDPOINTS
        // =======================

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto){
            var employee = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Login == loginDto.Login && e.IsActive);

            if (employee == null){
                return Unauthorized("Invalid login or employee is not active");
            }

            if (employee.PasswordHash != loginDto.Password){
                return Unauthorized("Invalid password");
            }

            var token = GenerateJwtToken(employee);
            var response = new LoginResponseDto{
                Token = token,
                Employee = new EmployeeDto{
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Login = employee.Login,
                    Role = employee.Role.Name,
                    IsActive = employee.IsActive
                }
            };

            return Ok(response);
        }

        [HttpGet("current-user")]
        public async Task<ActionResult<EmployeeDto>> GetCurrentUser(){
            return Ok(new{ Message = "Current user endpoint" });
        }

        /*
        [HttpGet("test")]
        public ActionResult Test(){
            return Ok(new{ message = "RolesController is working!" });
        }
*/
        private string GenerateJwtToken(Employee employee){
            return $"mock-token-{employee.Id}-{employee.Role.Name}-{Guid.NewGuid()}";
        }
    }
}
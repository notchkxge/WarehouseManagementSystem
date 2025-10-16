namespace WarehouseAPI.Core.Models.DTOs;

public class EmployeeDto{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Login { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
}
namespace WarehouseAPI.Core.Models.DTOs;

public class LoginResponseDto{
    public string Token { get; set; }
    public EmployeeDto Employee { get; set; }
}
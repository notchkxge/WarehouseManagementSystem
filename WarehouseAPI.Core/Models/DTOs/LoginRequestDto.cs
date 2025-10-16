namespace WarehouseAPI.Core.Models.DTOs;

public class LoginRequestDto{
    public string Token{ get; set; }
    public EmployeeDto Employee{ get; set; }
}
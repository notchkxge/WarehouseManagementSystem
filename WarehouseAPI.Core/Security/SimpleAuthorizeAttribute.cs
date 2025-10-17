using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

public class SimpleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _allowedRoles;

    public SimpleAuthorizeAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authorization token required" });
            return;
        }

        try
        {
            // Decode the token
            var tokenBytes = Convert.FromBase64String(token);
            var tokenData = Encoding.UTF8.GetString(tokenBytes);
            var parts = tokenData.Split(':');
            
            if (parts.Length != 3)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid token format" });
                return;
            }

            var role = parts[1];
            
            if (!_allowedRoles.Contains(role))
            {
                // Use 403 Forbidden with ObjectResult instead of ForbidResult
                context.Result = new ObjectResult(new { message = "Access forbidden: insufficient permissions" }) 
                { 
                    StatusCode = 403 
                };
                return;
            }

            // Store user info for use in controllers
            context.HttpContext.Items["EmployeeId"] = int.Parse(parts[0]);
            context.HttpContext.Items["Role"] = role;
        }
        catch
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid token" });
        }
    }
}
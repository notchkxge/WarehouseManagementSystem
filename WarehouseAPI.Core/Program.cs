using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Data;
using WarehouseAPI.Core.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0)) 
    ));

builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<WarehouseRepository>();
builder.Services.AddScoped<StorageLocationRepository>();
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<InventoryRepository>();
builder.Services.AddScoped<DocumentRepository>();
builder.Services.AddScoped<DocumentLineRepository>();
builder.Services.AddScoped<RoleRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DatabaseSeeder.Seed(context);
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DatabaseSeeder.Seed(context);
}

app.MapControllers();

app.Run();
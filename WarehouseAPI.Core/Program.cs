using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Data;
using WarehouseAPI.Core.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<WarehouseRepository>();
builder.Services.AddScoped<StorageLocationRepository>();
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<ProductBalanceRepository>();
builder.Services.AddScoped<DocumentRepository>();
builder.Services.AddScoped<DocumentLineRepository>();
builder.Services.AddScoped<DocumentStatusRepository>();
builder.Services.AddScoped<DocumentTypeRepository>();

var app = builder.Build();

app.MapControllers();

app.Run();
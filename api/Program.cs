using api.Data;
using api.Endpoints;
using api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add Authorization
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

// Add HttpClient
builder.Services.AddHttpClient();

// Add Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DarajaService>();
builder.Services.AddScoped<InventoryService>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => "App is running").WithName("Health").WithOpenApi();
app.MapUserEndpoints();
app.MapDarajaEndpoints();
app.MapInventoryEndpoints();

app.Run();


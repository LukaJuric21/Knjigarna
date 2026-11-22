using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Services;

var builder = WebApplication.CreateBuilder(args);

// Povezava s SQLite bazo.
builder.Services.AddDbContext<KnjigarnaContext>(options =>
    options.UseSqlite("Data Source=knjigarna.db"));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<KnjigaService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
using Microsoft.EntityFrameworkCore;
using WellRoute.DataAccess.Context;

var builder = WebApplication.CreateBuilder(args);

// Agregar DbContext
var connectionString = builder.Configuration.GetConnectionString("WellRouteConnection");
builder.Services.AddDbContext<WellRouteDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllers();
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
app.UseAuthorization();
app.MapControllers();

app.Run();
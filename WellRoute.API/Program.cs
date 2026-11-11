using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WellRoute.API;
using WellRoute.Arquitecture.Auditoria;
using WellRoute.Arquitecture.Auth;
using WellRoute.Arquitecture.Security;
using WellRoute.DataAccess.Context;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);



// Agregar DbContext
var connectionString = builder.Configuration.GetConnectionString("WellRouteConnection");
builder.Services.AddDbContext<WellRouteDbContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.Seccion));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Seccion));

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Historial de accesos y bitácora (issue #7)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IContextoSolicitud, ContextoSolicitud>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

var jwt = builder.Configuration.GetSection(JwtOptions.Seccion).Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
	throw new InvalidOperationException("Falta la clave Jwt:Key en user-secrets o es demasiado corta (mínimo 32 caracteres).");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.MapInboundClaims = false;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwt.Issuer,
			ValidateAudience = true,
			ValidAudience = jwt.Audience,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero,
			NameClaimType = "name",
			RoleClaimType = "role"
		};
	});

builder.Services.AddAuthorization(opciones =>
	{
		opciones.DefaultPolicy = new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.RequireClaim(ClaimNombres.DebeCambiarContrasena, "false")
			.Build();

		opciones.AddPolicy("CambioContrasenaTemporal", politica => politica
			.RequireAuthenticatedUser()
			.RequireClaim(ClaimNombres.DebeCambiarContrasena, "true"));
	});

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
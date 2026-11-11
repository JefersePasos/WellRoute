using Microsoft.AspNetCore.Authentication.Cookies;
using WellRoute.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var urlApi = builder.Configuration["WellRouteApi:BaseUrl"];
if (!Uri.TryCreate(urlApi, UriKind.Absolute, out var baseApi) ||
    baseApi.Scheme != Uri.UriSchemeHttps)
{
    throw new InvalidOperationException(
        "Configura WellRouteApi:BaseUrl con la dirección HTTPS de la API.");
}

builder.Services.AddHttpClient<AuthApiClient>(cliente =>
{
    cliente.BaseAddress = baseApi;
    cliente.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    // Sesión local de la Web para el token del primer ingreso.
    opciones.IdleTimeout = TimeSpan.FromMinutes(15);
    opciones.Cookie.Name = ".WellRoute.Cambio";
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opciones.Cookie.SameSite = SameSiteMode.Lax;
    opciones.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/Login";
        opciones.Cookie.Name = ".WellRoute.Auth";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.SlidingExpiration = false;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

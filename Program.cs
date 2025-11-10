using HyMFacturan.Components;
using HyMFacturan.Components.Data;       // <-- NECESARIO
using HyMFacturan.Components.Servicios;  // <-- NECESARIO

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- AQUÍ REGISTRAMOS AMBOS SERVICIOS ---

// 1. Registramos el servicio "real" que trabaja
builder.Services.AddScoped<ServicioFacturas>();

// 2. Registramos el "intermediario" que lo usa
//    Blazor automáticamente le inyectará ServicioFacturas
builder.Services.AddScoped<ServicioControlador>();

// ------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // <-- Importante
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
using Microsoft.EntityFrameworkCore;
using Libranet.Data; // Importa el DbContext
using Libranet.Models; // Importa los modelos

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Agregamos los servicios necesarios para la autenticación.
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        // Le decimos que el esquema de autenticación se llama "CookieAuth".
        options.Cookie.Name = "CookieAuth";

        // Le indicamos cuál es la página de login.
        // Si un usuario no autenticado intenta acceder a una página protegida,
        // será redirigido aquí automáticamente.
        options.LoginPath = "/Account/Login";
    });

builder.Services.AddDbContext<Libranet.Data.LibranetContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// --- SECCIÓN PARA SEMBRAR DATOS ---
// Este bloque de código se ejecutará una sola vez al iniciar la aplicación
// para asegurarse de que el usuario admin exista en la base de datos.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LibranetContext>();
        // Nos aseguramos de que la base de datos esté creada.
        context.Database.EnsureCreated();

        // Verificamos si ya existe algún admin en la base de datos.
        if (!context.Admins.Any())
        {
            // Si no hay admins, creamos uno.
            context.Admins.Add(new Admin
            {
                Username = "admin",
                // ¡IMPORTANTE! Guardamos la contraseña hasheada, no el texto plano.
                // La contraseña original es "admin123".
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
            });
            // Guardamos los cambios en la base de datos.
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar la base de datos.");
    }
}
// --- FIN DE LA SECCIÓN ---

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
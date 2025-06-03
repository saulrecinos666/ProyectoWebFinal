using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.Middlewares.Auth;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Users;
using StackExchange.Redis;
using ProyectoFinal.Hubs;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies; // ¡NUEVO USING!

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5278", "http://192.168.1.31:5278")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});

builder.Services.AddSignalR().AddStackExchangeRedis("localhost:6379", options =>
{
    options.Configuration.ChannelPrefix = "ChatApp";
});

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(configuration);
});

// *** MODIFICACIÓN CLAVE: AÑADIR AUTENTICACIÓN BASADA EN COOKIES ***
builder.Services.AddAuthentication(options =>
{
    // Establece un esquema por defecto, puedes elegir JWT o Cookies.
    // Si quieres que las páginas sean protegidas por cookies por defecto, usa CookieAuthenticationDefaults.AuthenticationScheme.
    // Si quieres que las APIs sean por JWT por defecto, puedes dejarlo así o especificar en [Authorize(AuthenticationSchemes = "...")]
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Por defecto para vistas
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Para desafíos de autenticación en vistas
})
.AddCookie(options =>
{
    options.Cookie.Name = "CitasMedicasAppCookie"; // Nombre de tu cookie de autenticación
    options.LoginPath = "/Home/Login"; // Ruta a la que redirigir si no está autenticado (para vistas)
    options.AccessDeniedPath = "/Home/AccessDenied"; // Opcional: para acceso denegado
    options.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(builder.Configuration["JwtSettings:ExpiresInMinutes"])); // Coincidir con la expiración de tu JWT
    options.SlidingExpiration = true; // Renueva la cookie si el usuario está activo
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
        ClockSkew = TimeSpan.FromMinutes(2)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/chathub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
        // ELIMINAR O COMENTAR EL EVENTO OnChallenge DE AQUÍ (Tu middleware lo maneja)
    };
});

builder.Services.AddDbContext<DbCitasMedicasContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Orden del pipeline de middleware: CRÍTICO ---

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);

// Autenticación y Autorización: ¡ESTE ORDEN ES MANDATORIO!
app.UseAuthentication(); // Debe venir antes de UseAuthorization
app.UseAuthorization();  // Debe venir después de UseAuthentication

// Tu TokenValidationMiddleware: Su lógica se ajustará para trabajar con cookies.
// Ahora se ejecutará *después* de que UseAuthentication haya intentado autenticar
// por cookies o JWT.
app.UseMiddleware<TokenValidationMiddleware>();


app.MapHub<ChatHub>("/chathub");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
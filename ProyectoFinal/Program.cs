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
using Microsoft.AspNetCore.Authentication.Cookies;
using Parcial3.Services; // Aseg�rate de que este namespace es el correcto para tu ReportService y RoleService
using ProyectoFinal.Services; // �NUEVO! Este using para RoleService si lo moviste aqu�
using Microsoft.AspNetCore.Http; // �NUEVO! Necesario para IHttpContextAccessor

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
                    .AllowAnyOrigin(); // Considera usar .AllowAnyOrigin() para desarrollo o con un filtro m�s estricto
                                        // .AllowCredentials(); // AllowCredentials no se puede usar con AllowAnyOrigin() en producci�n
        });
});

// Obt�n la cadena de conexi�n de Redis desde la configuraci�n
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

Console.WriteLine($"DEBUG: Redis Connection String used by SignalR: {redisConnectionString}");

builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString, options =>
{
    options.Configuration.ChannelPrefix = "ChatApp";
});

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Esta inyecci�n de IConnectionMultiplexer tambi�n usar� la misma cadena de conexi�n
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(configuration);
});

// Configuraci�n de autenticaci�n (Cookies y JWT Bearer)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Por defecto para vistas
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Para desaf�os de autenticaci�n en vistas
})
.AddCookie(options =>
{
    options.Cookie.Name = "CitasMedicasAppCookie"; // Nombre de tu cookie de autenticaci�n
    options.LoginPath = "/Home/Login"; // Ruta a la que redirigir si no est� autenticado (para vistas)
    options.AccessDeniedPath = "/Home/AccessDenied"; // Opcional: para acceso denegado
    options.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(builder.Configuration["JwtSettings:ExpiresInMinutes"])); // Coincidir con la expiraci�n de tu JWT
    options.SlidingExpiration = true; // Renueva la cookie si el usuario est� activo
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
    };
});

builder.Services.AddDbContext<DbCitasMedicasContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de tu ReportService
builder.Services.AddScoped<ReportService>();

// --- �NUEVO! Registro de IHttpContextAccessor y RoleService ---
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Necesario para RoleService
builder.Services.AddScoped<RoleService>(); // Registro de tu nuevo RoleService
// --- FIN: Registro de IHttpContextAccessor y RoleService ---

// Configuraci�n de Pol�ticas de Autorizaci�n
builder.Services.AddAuthorization(options =>
{
    // Pol�ticas basadas en Roles (usando ClaimTypes.Role)
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("RequireDoctorRole", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("RequirePatientRole", policy => policy.RequireRole("Paciente"));
    options.AddPolicy("RequireSecretaryRole", policy => policy.RequireRole("Secretaria")); // Ejemplo si tienes este rol

    // Pol�ticas basadas en Permisos (usando el tipo de Claim "Permission")
    options.AddPolicy("CanViewAppointments", policy => policy.RequireClaim("Permission", "can_view_appointments"));
    options.AddPolicy("CanManageAppointments", policy => policy.RequireClaim("Permission", "can_manage_appointments"));
    options.AddPolicy("CanManageDoctors", policy => policy.RequireClaim("Permission", "can_manage_doctors"));
    options.AddPolicy("CanManageInstitutions", policy => policy.RequireClaim("Permission", "can_manage_institutions"));
    options.AddPolicy("CanManageSpecialties", policy => policy.RequireClaim("Permission", "can_manage_specialties"));
    options.AddPolicy("CanManagePatients", policy => policy.RequireClaim("Permission", "can_manage_patients"));
    options.AddPolicy("CanManageUsers", policy => policy.RequireClaim("Permission", "can_manage_users"));
    options.AddPolicy("CanGenerateReports", policy => policy.RequireClaim("Permission", "can_generate_reports"));
    options.AddPolicy("CanViewLoginHistory", policy => policy.RequireClaim("Permission", "can_view_login_history"));
    options.AddPolicy("CanAccessAppointments", policy => policy.RequireClaim("Permission", "can_access_appointments_section"));
    options.AddPolicy("CanAccessPatients", policy => policy.RequireClaim("Permission", "can_access_patients_section"));
    options.AddPolicy("CanAccessDoctors", policy => policy.RequireClaim("Permission", "can_access_doctors_section"));
    options.AddPolicy("CanAccessUsers", policy => policy.RequireClaim("Permission", "can_access_users_section"));

    // �NUEVA POL�TICA PARA GESTI�N DE ROLES!
    options.AddPolicy("CanManageRoles", policy => policy.RequireClaim("Permission", "can_manage_roles"));

    // Pol�ticas combinadas
    options.AddPolicy("AdminOrCanManageUsers", policy => policy.RequireAssertion(context =>
        context.User.IsInRole("Administrador") || context.User.HasClaim("Permission", "can_manage_users")
    ));
});


builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Orden del pipeline de middleware: CR�TICO ---

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);

// Autenticaci�n y Autorizaci�n: �ESTE ORDEN ES MANDATORIO!
app.UseAuthentication(); // Debe venir antes de UseAuthorization
app.UseAuthorization();  // Debe venir despu�s de UseAuthentication

// Tu TokenValidationMiddleware: Su l�gica se ajustar� para trabajar con cookies.
// Ahora se ejecutar� *despu�s* de que UseAuthentication haya intentado autenticar
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
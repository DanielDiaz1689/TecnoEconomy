using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using Ecommerce.Repositorio.DBContext;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Utilidades;
using Ecommerce.Repositorio.Contrato;
using Ecommerce.Repositorio.Implementacion;
using Ecommerce.Servicio.Contrato;
using Ecommerce.Servicio.Implementacion;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Ecommerce.DTO;
using System.Net.Http;
using Microsoft.Extensions.Logging; // Asegúrate de que este using esté

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configuración de Logging Extremadamente Detallada
    builder.Host.ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        logging.SetMinimumLevel(LogLevel.Trace); // Nivel más bajo para ver todo
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);
        logging.AddFilter("System", LogLevel.Warning);
        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
    });

    // Add services to the container.
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

    // Configuración de la cadena de conexión de la base de datos
    builder.Services.AddDbContext<PuntoDeVentaContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("cadenaSQL"));
    });

    // Inyección de dependencias de repositorios
    builder.Services.AddScoped(typeof(IGenericoRepositorio<>), typeof(GenericoRepositorio<>));
    builder.Services.AddScoped<IVentaRepositorio, VentaRepositorio>();

    // Inyección de dependencias de servicios (backend)
    builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();
    builder.Services.AddScoped<IProductoServicio, ProductoServicio>();
    builder.Services.AddScoped<IVentaServicio, VentaServicio>();
    builder.Services.AddScoped<IDashboardServicio, DashboardServicio>();

    //builder.Services.AddScoped<ICompraServicio, CompraServicio>();
    // Registro de IFakeProductoServicio con su HttpClient
    builder.Services.AddHttpClient<IFakeProductoServicio, FakeProductoServicio>(client =>
    {
        client.BaseAddress = new Uri("https://fakestoreapi.com/");
    });

    // Configuración de AutoMapper
    builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

    // Configuración de CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins", policy =>
        {
            policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API V1");
            c.RoutePrefix = "docs";
        });
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseCors("AllowAllOrigins");

    app.MapControllers();

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"\n\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    Console.WriteLine($"EXCEPCIÓN CRÍTICA DURANTE EL INICIO/EJECUCIÓN DEL SERVIDOR:");
    Console.WriteLine($"Tipo: {ex.GetType().Name}");
    Console.WriteLine($"Mensaje: {ex.Message}");
    Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message ?? "N/A"}");
    Console.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n\n");
    Console.WriteLine("Presiona cualquier tecla para cerrar la ventana...");
    Console.ReadKey(); // Espera una pulsación de tecla
    throw; // Relanza la excepción para que Visual Studio aún la capture en su ventana de "Excepciones"
}
finally
{
    // Esto se ejecutará siempre, incluso si hay un throw.
    // Útil para debugging final.
    Console.WriteLine("Aplicación del servidor finalizada.");
    Console.WriteLine("Presiona cualquier tecla para cerrar la ventana (desde finally)...");
    Console.ReadKey();
}

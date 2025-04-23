using MudBlazor.Services;
using Ecommerce.Client.Pages;
using Ecommerce.Components;
using Ecommerce.Repositorio.DBContext;
using Microsoft.EntityFrameworkCore;

using Ecommerce.Utilidades;

using Ecommerce.Repositorio.Contrato;
using Ecommerce.Repositorio.Implementacion;

using Ecommerce.Servicio.Contrato;
using Ecommerce.Servicio.Implementacion;

namespace Ecommerce;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // Add MudBlazor services
        builder.Services.AddMudServices();
        builder.Services.AddDbContext<PuntoDeVentaContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL"));
        });

        builder.Services.AddTransient(typeof(IGenericoRepositorio<>), typeof(GenericoRepositorio<>));
        builder.Services.AddScoped<IVentaRepositorio, VentaRepositorio>();


        builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

        builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();
        //builder.Services.AddScoped<ICategoriaServicio, CategoriaServicio>();
        //builder.Services.AddScoped<IProductoServicio, ProductoServicio>();
        builder.Services.AddScoped<IVentaServicio, VentaServicio>();
        builder.Services.AddScoped<IDashboardServicio, DashboardServicio>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("NuevaPolitica", app =>
            {
                app.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });



        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();



        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseCors("NuevaPolitica");

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapControllers();

        app.MapStaticAssets();
        
        app.MapRazorComponents<AppServer>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();
    }
}

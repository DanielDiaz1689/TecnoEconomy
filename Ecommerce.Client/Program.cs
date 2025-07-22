using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using CurrieTechnologies.Razor.SweetAlert2;
using MudBlazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Ecommerce.Client.Extensiones;
using Ecommerce.Client.Servicios.Contrato;
using Ecommerce.Client.Servicios.Implementacion;
using Ecommerce.Servicio.Contrato;
using Ecommerce.Servicio.Implementacion;
//using Ecommerce.Client.Servicios.Contrato.Implementacion;

namespace Ecommerce.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSweetAlert2();
            builder.Services.AddMudServices();

            // =======================================================================
            // CONFIGURACIÓN DE SERVICIOS DE AUTENTICACIÓN (BLAZOR UI) Y HTTP CLIENT
            // =======================================================================

            // URL BASE DE TU API DE BACKEND (¡VERIFICA ESTE PUERTO EN TU SERVER!)
            var backendApiBaseUrl = "https://localhost:7218/";

            // Registra tu AutenticacionExtension como su tipo concreto.
            // Esto permite que MainLayout la inyecte directamente para llamar a ActualizarEstadoAutenticacion.
            builder.Services.AddScoped<AutenticacionExtension>();

            // Registra tu AutenticacionExtension como la implementación de AuthenticationStateProvider.
            // Esto es fundamental para el funcionamiento del sistema de autorización de Blazor (AuthorizeView, CascadingAuthenticationState).
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<AutenticacionExtension>());

            // Agrega los servicios de autorización necesarios para Blazor WebAssembly UI.
            builder.Services.AddAuthorizationCore();


            // Configuración de HttpClients para tus servicios.
            // YA NO SE USA AuthHttpMessageHandler aquí.
            // Las llamadas a tus APIs del backend no incluirán el token JWT automáticamente.

            // LoginServicio: Conecta a tu API de Backend
            builder.Services.AddHttpClient<ILoginServicio, LoginServicio>(client =>
            {
                client.BaseAddress = new Uri(backendApiBaseUrl);
            });

            // UsuarioServicio: Conecta a tu API de Backend
            builder.Services.AddHttpClient<Servicios.Contrato.IUsuarioServicio, Servicios.Implementacion.UsuarioServicio>(client =>
            {
                client.BaseAddress = new Uri(backendApiBaseUrl);
            });

            builder.Services.AddHttpClient<ICompraServicio, CompraServicio>(client =>
            {
                client.BaseAddress = new Uri(backendApiBaseUrl);
            });
            // ProductoClienteServicio: Conecta a tu API de Backend
            builder.Services.AddHttpClient<IProductoClienteServicio, ProductoClienteServicio>(client =>
            {
                client.BaseAddress = new Uri(backendApiBaseUrl);
            });

            // VentaServicio: Conecta a tu API de Backend
            builder.Services.AddHttpClient<Servicios.Contrato.IVentaServicio, Servicios.Implementacion.VentaServicio>(client =>
            {
                client.BaseAddress = new Uri(backendApiBaseUrl);
            });

            // DashboardServicio: Conecta a tu API de Backend
            builder.Services.AddHttpClient<Servicios.Contrato.IDashboardServicio, Servicios.Implementacion.DashboardServicio>(client =>
            {
                client.BaseAddress = new Uri(backendApiBaseUrl);
            });

            // FakeProductoServicio: Conecta a una API externa, no a tu backend.
            builder.Services.AddHttpClient<IFakeProductoServicio, FakeProductoServicio>(client =>
            {
                client.BaseAddress = new Uri("https://fakestoreapi.com/");
            });

            builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();

            // =======================================================================

            await builder.Build().RunAsync();
        }
    }
}
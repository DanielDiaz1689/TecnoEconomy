using Ecommerce.Client.Servicios.Contrato;
using Ecommerce.DTO;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Ecommerce.Client.Extensiones; 

namespace Ecommerce.Client.Servicios.Implementacion
{
    public class LoginServicio : ILoginServicio
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorageService;
        private readonly AuthenticationStateProvider _authStateProvider;

        public LoginServicio(HttpClient http, ILocalStorageService localStorageService, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _localStorageService = localStorageService;
            _authStateProvider = authStateProvider;
        }

        // Si usas este método, debe guardar el token y actualizar el estado
        public async Task<ResponseDTO<SesionDTO>> ValidarUsuario(LoginDTO modelo)
        {
            var responseDTO = new ResponseDTO<SesionDTO>();
            try
            {
                // Asegúrate que esta URL es correcta para tu endpoint de autenticación
                var httpResponse = await _http.PostAsJsonAsync("api/usuario/autenticar", modelo);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var sesion = await httpResponse.Content.ReadFromJsonAsync<ResponseDTO<SesionDTO>>();

                    if (sesion != null && sesion.EsCorrecto && sesion.Resultado != null)
                    {
                        // **¡CLAVE! Guardar SOLO el token JWT como una cadena.**
                        await _localStorageService.SetItemAsStringAsync("token", sesion.Resultado.Token);

                        // Crear ClaimsPrincipal para notificar a Blazor
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, sesion.Resultado.UsuId.ToString()),
                            new Claim(ClaimTypes.Name, sesion.Resultado.UsuLogin),
                            new Claim(ClaimTypes.Email, sesion.Resultado.UsuCorreo ?? string.Empty),
                            new Claim(ClaimTypes.Role, sesion.Resultado.UsuRol ?? string.Empty)
                        };
                        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

                        // Notificar a AutenticacionExtension con ClaimsPrincipal
                        await ((AutenticacionExtension)_authStateProvider).ActualizarEstadoAutenticacion(claimsPrincipal);

                        responseDTO.EsCorrecto = true;
                        responseDTO.Resultado = sesion.Resultado;
                        responseDTO.Mensaje = "Inicio de sesión exitoso.";
                    }
                    else
                    {
                        responseDTO.EsCorrecto = false;
                        responseDTO.Mensaje = sesion?.Mensaje ?? "Credenciales inválidas o respuesta incompleta.";
                    }
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    responseDTO.EsCorrecto = false;
                    responseDTO.Mensaje = $"Error de red o servidor: {httpResponse.StatusCode} - {errorContent}";
                    Console.Error.WriteLine($"Error HTTP en ValidarUsuario: {httpResponse.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                responseDTO.EsCorrecto = false;
                responseDTO.Mensaje = $"Error inesperado al validar usuario: {ex.Message}";
                Console.Error.WriteLine($"Excepción en ValidarUsuario: {ex.Message}");
            }
            return responseDTO;
        }

        // Si tu frontend llama a este método para el login, también debe guardar el token y actualizar el estado
        public async Task<ResponseDTO<SesionDTO>> Autorizacion(LoginDTO modelo)
        {
            var responseDTO = new ResponseDTO<SesionDTO>();
            try
            {
                // Asegúrate que esta URL es correcta para tu endpoint de autorización
                var httpResponse = await _http.PostAsJsonAsync("api/usuario/autorizacion", modelo);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var sesion = await httpResponse.Content.ReadFromJsonAsync<ResponseDTO<SesionDTO>>();

                    if (sesion != null && sesion.EsCorrecto && sesion.Resultado != null)
                    {
                        // **¡CLAVE! Guardar SOLO el token JWT como una cadena.**
                        await _localStorageService.SetItemAsStringAsync("token", sesion.Resultado.Token);

                        // Crear ClaimsPrincipal para notificar a Blazor
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, sesion.Resultado.UsuId.ToString()),
                            new Claim(ClaimTypes.Name, sesion.Resultado.UsuLogin),
                            new Claim(ClaimTypes.Email, sesion.Resultado.UsuCorreo ?? string.Empty),
                            new Claim(ClaimTypes.Role, sesion.Resultado.UsuRol ?? string.Empty)
                        };
                        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

                        // Notificar a AutenticacionExtension con ClaimsPrincipal
                        await ((AutenticacionExtension)_authStateProvider).ActualizarEstadoAutenticacion(claimsPrincipal);

                        responseDTO.EsCorrecto = true;
                        responseDTO.Resultado = sesion.Resultado;
                        responseDTO.Mensaje = "Inicio de sesión exitoso.";
                    }
                    else
                    {
                        responseDTO.EsCorrecto = false;
                        responseDTO.Mensaje = sesion?.Mensaje ?? "Credenciales inválidas o respuesta incompleta.";
                    }
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    responseDTO.EsCorrecto = false;
                    responseDTO.Mensaje = $"Error de red o servidor: {httpResponse.StatusCode} - {errorContent}";
                    Console.Error.WriteLine($"Error HTTP en Autorizacion: {httpResponse.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                responseDTO.EsCorrecto = false;
                responseDTO.Mensaje = $"Error inesperado durante la autorización: {ex.Message}";
                Console.Error.WriteLine($"Excepción en Autorizacion: {ex.Message}");
            }
            return responseDTO;
        }
    }
}
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims; // Necesario para ClaimsPrincipal, ClaimsIdentity, ClaimTypes
using System.Text.Json; // Necesario para JsonSerializer, JsonElement, etc.
using Ecommerce.DTO; // Necesario para SesionDTO

namespace Ecommerce.Client.Extensiones
{
    // Ahora solo hereda de AuthenticationStateProvider
    public class AutenticacionExtension : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private ClaimsPrincipal _sinInformacion = new ClaimsPrincipal(new ClaimsIdentity());

        public AutenticacionExtension(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        // Método para actualizar el estado de autenticación de Blazor
        // Recibe un ClaimsPrincipal (la forma estándar)
        public async Task ActualizarEstadoAutenticacion(ClaimsPrincipal? claimsPrincipal)
        {
            // La lógica de guardar/eliminar el token ahora se maneja directamente en LoginServicio.
            // Aquí, solo se notifica a Blazor del nuevo estado.
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal ?? _sinInformacion)));

            Console.WriteLine($"DEBUG: Autenticación actualizada. Autenticado: {claimsPrincipal?.Identity?.IsAuthenticated ?? false}");
        }

        // Método para obtener el estado de autenticación (requerido por AuthenticationStateProvider)
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Obtener el token directamente del local storage
                var token = await _localStorage.GetItemAsStringAsync("token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("DEBUG: GetAuthenticationStateAsync - No hay token en LocalStorage. Usuario no autenticado.");
                    return new AuthenticationState(_sinInformacion); // No hay token, no autenticado
                }

                // Decodificar el token y crear ClaimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
                Console.WriteLine($"DEBUG: GetAuthenticationStateAsync - Token encontrado y claims parseados. Autenticado: {claimsPrincipal.Identity?.IsAuthenticated}");

                return new AuthenticationState(claimsPrincipal);
            }
            catch (Exception ex)
            {
                // En caso de error (token corrupto, etc.), limpia el token y devuelve un estado no autenticado
                Console.Error.WriteLine($"ERROR: GetAuthenticationStateAsync - Error al parsear token: {ex.Message}");
                await _localStorage.RemoveItemAsync("token"); // Limpia el token inválido
                return new AuthenticationState(_sinInformacion);
            }
        }

        // Método auxiliar para parsear claims del token JWT
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1]; // Obtiene la parte del payload del token JWT
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null) return claims;

            // Mapear claims estándar y personalizados
            // ClaimTypes.NameIdentifier (sub en JWT)
            if (keyValuePairs.TryGetValue("nameid", out object? nameId)) // Esto es común si el ID es el 'sub' claim
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId.ToString() ?? string.Empty));
                keyValuePairs.Remove("nameid");
            }
            else if (keyValuePairs.TryGetValue("sub", out object? sub)) // Alternativa común para NameIdentifier
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.ToString() ?? string.Empty));
                keyValuePairs.Remove("sub");
            }

            // ClaimTypes.Name (unique_name o name en JWT) - esto será UsuLogin
            if (keyValuePairs.TryGetValue("unique_name", out object? uniqueName))
            {
                claims.Add(new Claim(ClaimTypes.Name, uniqueName.ToString() ?? string.Empty));
                keyValuePairs.Remove("unique_name");
            }
            else if (keyValuePairs.TryGetValue("name", out object? name)) // Si el claim es solo "name"
            {
                claims.Add(new Claim(ClaimTypes.Name, name.ToString() ?? string.Empty));
                keyValuePairs.Remove("name");
            }

            // ClaimTypes.Email (email en JWT)
            if (keyValuePairs.TryGetValue("email", out object? email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email.ToString() ?? string.Empty));
                keyValuePairs.Remove("email");
            }

            // ClaimTypes.Role (role en JWT) - puede ser un string o un array de strings
            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out object? roles))
            {
                if (roles is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in jsonElement.EnumerateArray())
                    {
                        claims.Add(new Claim(ClaimTypes.Role, element.GetString() ?? string.Empty));
                    }
                }
                else if (roles != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString() ?? string.Empty));
                }
                keyValuePairs.Remove(ClaimTypes.Role);
            }

            // Añadir cualquier otro claim no mapeado explícitamente
            foreach (var kvp in keyValuePairs)
            {
                claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
            }
            return claims;
        }

        // Método auxiliar para manejar padding de Base64
        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}

// En tu proyecto Ecommerce.Client/Servicios/Implementacion/ProductoClienteServicio.cs

using Ecommerce.Client.Servicios.Contrato;
using Ecommerce.DTO; // Para ResponseDTO y ProductoDTO
using System.Net.Http; // Para HttpClient
using System.Net.Http.Json; // Para GetFromJsonAsync
using System.Collections.Generic; // Para List
using System.Threading.Tasks; // Para Task
using System; // Para Uri.EscapeDataString
using Microsoft.AspNetCore.Components.Forms; // Para IBrowserFile
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json; // Para JsonSerializerOptions


namespace Ecommerce.Client.Servicios.Implementacion
{
    public class ProductoClienteServicio : IProductoClienteServicio
    {
        private readonly HttpClient _httpClient; // Convención: _httpClient

        public ProductoClienteServicio(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Implementación del método Catalogo
        public async Task<ResponseDTO<List<ProductoDTO>>> Catalogo(string buscar)
        {
            Console.WriteLine("DEBUG (ProductoClienteServicio): Iniciando llamada a /api/Producto/Lista para el catálogo.");
            try
            {
                var url = string.IsNullOrWhiteSpace(buscar)
                    ? "api/Producto/Lista"
                    : $"api/Producto/Lista?buscar={Uri.EscapeDataString(buscar)}";

                Console.WriteLine($"DEBUG (ProductoClienteServicio): URL de la solicitud HTTP: {_httpClient.BaseAddress}{url}");

                // Utiliza GetFromJsonAsync para simplificar la respuesta y la deserialización.
                // Esto lanzará una HttpRequestException si el status code no es 2xx o si la deserialización falla.
                var result = await _httpClient.GetFromJsonAsync<ResponseDTO<List<ProductoDTO>>>(url);

                if (result == null)
                {
                    Console.Error.WriteLine("ERROR (ProductoClienteServicio): Deserialización de respuesta de catálogo resultó en null.");
                    return new ResponseDTO<List<ProductoDTO>> { EsCorrecto = false, Mensaje = "Respuesta de servidor nula o no deserializable." };
                }

                Console.WriteLine($"DEBUG (ProductoClienteServicio): Catálogo recibido. EsCorrecto: {result.EsCorrecto}, Mensaje: {result.Mensaje}, Productos: {result.Resultado?.Count ?? 0}");
                return result; // Devuelve el ResponseDTO
            }
            catch (HttpRequestException httpEx)
            {
                // Captura errores específicos de HTTP (ej. 404, 500, o connection refused)
                Console.Error.WriteLine($"ERROR (ProductoClienteServicio): HttpRequestException en Catalogo: {httpEx.StatusCode} - {httpEx.Message}. (Pudo ser CONNECTION REFUSED)");
                return new ResponseDTO<List<ProductoDTO>>
                {
                    EsCorrecto = false,
                    Mensaje = $"Error de conexión o HTTP: {httpEx.Message}. Asegúrate que el backend está corriendo y accesible.",
                    Resultado = null
                };
            }
            catch (Exception ex)
            {
                // Captura otras excepciones (ej. problemas de deserialización inesperados)
                Console.Error.WriteLine($"ERROR (ProductoClienteServicio): Excepción general en Catalogo: {ex.Message}.");
                return new ResponseDTO<List<ProductoDTO>>
                {
                    EsCorrecto = false,
                    Mensaje = $"Excepción al cargar catálogo: {ex.Message}",
                    Resultado = null
                };
            }
        }

        // --- Otros métodos del ProductoClienteServicio (mantén los que necesites) ---
        // Asegúrate de que todos usan 'ResponseDTO.Resultado' y no 'ResponseDTO.Valor'

        public async Task<ResponseDTO<bool>> ImportarDesdeFakeStore()
        {
            var responseDTO = new ResponseDTO<bool>();

            try
            {
                // Asegúrate que esta URL es la correcta para tu backend que importa de FakeStore
                var response = await _httpClient.GetAsync("api/Producto/ImportarDesdeFakeStore");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    responseDTO.EsCorrecto = false;
                    responseDTO.Mensaje = $"Error HTTP {(int)response.StatusCode}: {errorContent}";
                    return responseDTO;
                }

                var json = await response.Content.ReadAsStringAsync();
                responseDTO = JsonSerializer.Deserialize<ResponseDTO<bool>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ResponseDTO<bool> { EsCorrecto = false, Mensaje = "Respuesta nula del servidor." };
            }
            catch (Exception ex)
            {
                responseDTO.EsCorrecto = false;
                responseDTO.Mensaje = $"Excepción al importar: {ex.Message}";
            }
            return responseDTO;
        }

        public async Task<ResponseDTO<ProductoDTO>> Crear(ProductoDTO modelo)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Producto/Crear", modelo);
            var result = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductoDTO>>();

            return result ?? new ResponseDTO<ProductoDTO>
            {
                EsCorrecto = false,
                Mensaje = "Error al crear el producto"
            };
        }

        public async Task<ResponseDTO<ProductoDTO>> Editar(ProductoDTO modelo)
        {
            var response = await _httpClient.PutAsJsonAsync("api/Producto/Editar", modelo);
            var result = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductoDTO>>();

            return result ?? new ResponseDTO<ProductoDTO>
            {
                EsCorrecto = false,
                Mensaje = "Error al editar el producto"
            };
        }

        public async Task<ResponseDTO<bool>> Eliminar(int Id)
        {
            return await _httpClient.DeleteFromJsonAsync<ResponseDTO<bool>>($"api/Producto/Eliminar/{Id}")
                ?? new ResponseDTO<bool>
                {
                    EsCorrecto = false,
                    Mensaje = "No se pudo eliminar el producto"
                };
        }

        public async Task<ResponseDTO<ProductoDTO>> Obtener(int Id)
        {
            return await _httpClient.GetFromJsonAsync<ResponseDTO<ProductoDTO>>($"api/Producto/Obtener/{Id}")
                ?? new ResponseDTO<ProductoDTO>
                {
                    EsCorrecto = false,
                    Mensaje = "No se pudo obtener el producto"
                };
        }

        public async Task<ResponseDTO<List<ProductoDTO>>> Lista(string buscar)
        {
            // Este método 'Lista' también existe en el servidor.
            // Asegúrate de que el frontend llama al método correcto.
            // Si Catalogo() es el que quieres usar para el listado público,
            // entonces esta implementación de Lista() quizás no sea necesaria aquí.
            var url = string.IsNullOrWhiteSpace(buscar)
                ? "api/Producto/Lista"
                : $"api/Producto/Lista?buscar={Uri.EscapeDataString(buscar)}";

            return await _httpClient.GetFromJsonAsync<ResponseDTO<List<ProductoDTO>>>(url)
                ?? new ResponseDTO<List<ProductoDTO>>
                {
                    EsCorrecto = false,
                    Mensaje = "No se obtuvo respuesta del servidor"
                };
        }

        public async Task<bool> AumentarSaldo(int idProducto, decimal cantidad)
        {
            // Asume que tienes un endpoint en tu API para manejar el aumento de saldo
            // Por ejemplo, api/Producto/AumentarSaldo
            var data = new { IdProducto = idProducto, Cantidad = cantidad };
            var response = await _httpClient.PostAsJsonAsync("api/Producto/AumentarSaldo", data);

            if (response.IsSuccessStatusCode)
            {
                var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO<bool>>();
                return responseDTO?.EsCorrecto ?? false; // Retorna si la operación fue exitosa según el backend
            }
            return false;
        }

        public async Task<ResponseDTO<string>> SubirImagen(IBrowserFile archivo)
        {
            try
            {
                var contenido = new MultipartFormDataContent();
                var stream = archivo.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);

                contenido.Add(content: fileContent, name: "archivo", fileName: archivo.Name);

                var response = await _httpClient.PostAsync("api/Upload/Imagen", contenido);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<ResponseDTO<string>>();
                    return resultado ?? new ResponseDTO<string> { EsCorrecto = false, Mensaje = "Sin respuesta del servidor" };
                }
                else
                {
                    return new ResponseDTO<string> { EsCorrecto = false, Mensaje = "Error al subir la imagen" };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO<string> { EsCorrecto = false, Mensaje = ex.Message };
            }
        }
    }
}



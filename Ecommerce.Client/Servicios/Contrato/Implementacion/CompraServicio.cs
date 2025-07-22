using System.Net.Http.Json; // Necesario para PostAsJsonAsync y ReadFromJsonAsync
using Ecommerce.DTO; // Necesario para CompraDTO y ResponseDTO
using Ecommerce.Client.Servicios.Contrato; // Necesario para ICompraServicio

namespace Ecommerce.Client.Servicios.Implementacion
{
    public class CompraServicio : ICompraServicio
    {
        private readonly HttpClient _httpClient;

        public CompraServicio(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResponseDTO<CompraDTO>> Registrar(CompraDTO modelo)
        {
            // Realiza la solicitud POST al endpoint de la API.
            // Se envía el 'modelo' completo, que es un CompraDTO.
            var response = await _httpClient.PostAsJsonAsync("api/compra/registrar", modelo);

            // Si la respuesta HTTP no es exitosa (ej. 400 Bad Request, 500 Internal Server Error)
            if (!response.IsSuccessStatusCode)
            {
                // Lee el contenido del error para depuración
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error HTTP en CompraServicio.Registrar: {response.StatusCode} - {errorMessage}");

                // Devuelve un ResponseDTO<CompraDTO> indicando el error.
                // Es importante que el tipo de retorno aquí sea CompraDTO, no bool.
                return new ResponseDTO<CompraDTO>
                {
                    EsCorrecto = false,
                    Mensaje = $"Error de API: {errorMessage}",
                    Resultado = null // En caso de error, el resultado es nulo
                };
            }

            // Si la respuesta HTTP es exitosa, deserializa el contenido.
            // Se espera que el backend devuelva un ResponseDTO que contenga un CompraDTO.
            var result = await response.Content.ReadFromJsonAsync<ResponseDTO<CompraDTO>>();

            // Retorna el resultado. El operador '!' indica que el valor no será nulo.
            return result!;
        }

        // Si tienes otros métodos en ICompraServicio, también deben implementarse aquí.
        // Ejemplo (si tu interfaz los tiene):
        // public Task<ResponseDTO<List<CompraDTO>>> Lista(string buscar = "")
        // {
        //     throw new NotImplementedException();
        // }

        // public Task<ResponseDTO<CompraDTO>> Obtener(int id)
        // {
        //     throw new NotImplementedException();
        // }
    }
}
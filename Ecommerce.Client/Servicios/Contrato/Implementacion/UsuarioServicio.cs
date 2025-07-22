using Ecommerce.Client.Servicios.Contrato;

using Ecommerce.DTO;

using System.Net.Http.Json;





namespace Ecommerce.Client.Servicios.Implementacion

{

    public class UsuarioServicio : IUsuarioServicio

    {

        private readonly HttpClient _httpClient;



        public UsuarioServicio(HttpClient httpClient)

        {

            _httpClient = httpClient;

        }



        public async Task<ResponseDTO<List<UsuarioDTO>>> Lista(string rol, string buscar)

        {

            return await _httpClient.GetFromJsonAsync<ResponseDTO<List<UsuarioDTO>>>(

              $"api/usuario/Lista?rol={rol}&buscar={buscar ?? ""}"

            );

        }





        public async Task<ResponseDTO<UsuarioDTO>> Obtener(int id)

        {

            return await _httpClient.GetFromJsonAsync<ResponseDTO<UsuarioDTO>>(

              $"api/usuario/Obtener/{id}"

            );

        }



        public async Task<ResponseDTO<UsuarioDTO>> Crear(UsuarioDTO modelo)

        {

            var respuesta = await _httpClient.PostAsJsonAsync("api/usuario/Crear", modelo);

            return await respuesta.Content.ReadFromJsonAsync<ResponseDTO<UsuarioDTO>>();

        }



        public async Task<ResponseDTO<bool>> Editar(UsuarioDTO modelo)

        {

            var respuesta = await _httpClient.PutAsJsonAsync("api/usuario/Editar", modelo);

            return await respuesta.Content.ReadFromJsonAsync<ResponseDTO<bool>>();

        }



        public async Task<ResponseDTO<bool>> Eliminar(int id)

        {

            var respuesta = await _httpClient.DeleteAsync($"api/usuario/Eliminar/{id}"); // este lo cambie

            var contenido = await respuesta.Content.ReadFromJsonAsync<ResponseDTO<bool>>();



            return contenido ?? new ResponseDTO<bool> { EsCorrecto = false, Mensaje = "Error al procesar la respuesta" };

        }



        public async Task<ResponseDTO<UsuarioDTO>> ObtenerPorCedula(string cedula)

        {

            var response = await _httpClient.GetFromJsonAsync<ResponseDTO<UsuarioDTO>>($"api/usuario/por-cedula/{cedula}");

            return response!;

        }





        public async Task<ResponseDTO<SesionDTO>> Autorizacion(LoginDTO modelo)

        {

            var respuesta = await _httpClient.PostAsJsonAsync("api/Usuario/Autorizacion", modelo);

            return await respuesta.Content.ReadFromJsonAsync<ResponseDTO<SesionDTO>>();

        }

    }

}

//using Ecommerce.DTO;
//using Ecommerce.Client.Servicios.Contrato;
//using System.Net.Http.Json;


//namespace Ecommerce.Client.Servicios.Implementacion
//{
//    public class CategoriaServicio :ICategoriaServicio 
//    {
//        private readonly HttpClient _httpclient;

//        public CategoriaServicio(HttpClient httpClient)
//        {
//            _httpclient = httpClient;
//        }


//        public async Task<ResponseDTO<CategoriaDTO>> Crear(CategoriaDTO modelo)
//        {
//            var response = await _httpclient.PostAsJsonAsync("Categoria/Crear", modelo);

//            var result = await response.Content.ReadFromJsonAsync<ResponseDTO<CategoriaDTO>>();

//            return result!;
//        }

//        public async Task<ResponseDTO<CategoriaDTO>> Editar(CategoriaDTO modelo)
//        {
//            var response = await _httpclient.PostAsJsonAsync("Categoria/Editar", modelo);

//            var result = await response.Content.ReadFromJsonAsync<ResponseDTO<CategoriaDTO>>();

//            return result!;
//        }

//        public async Task<ResponseDTO<bool>> Eliminar(int Id)
//        {
//            return await _httpclient.DeleteFromJsonAsync<ResponseDTO<bool>>($"Categoria/Eliminar/{Id}");
//        }

//        public async Task<ResponseDTO<List<UsuarioDTO>>> Lista(string rol, string buscar)
//        {
//            return await _httpclient.GetFromJsonAsync<ResponseDTO<List<CategoriaDTO>>>($"Categoria/Lista/{buscar}");
//        }

//        public async Task<ResponseDTO<UsuarioDTO>> Obtener(int Id)
//        {
//            return await _httpclient.GetFromJsonAsync<ResponseDTO<CategoriaDTO>>($"Categoria/Obtener/{Id}");
//        }
//    }
//}

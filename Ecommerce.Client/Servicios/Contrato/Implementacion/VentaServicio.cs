using Ecommerce.DTO;
using Ecommerce.Client.Servicios.Contrato;
using System.Net.Http.Json;

namespace Ecommerce.Client.Servicios.Implementacion
{
    public class VentaServicio : IVentaServicio
    {
        private readonly HttpClient _httpclient;

        public VentaServicio(HttpClient httpClient)
        {
            _httpclient = httpClient;
        }

        public async Task<ResponseDTO<VentaDTO>> Registrar(VentaDTO modelo)
        {
            var response = await _httpclient.PostAsJsonAsync("api/Venta/Registrar", modelo);

            var result = await response.Content.ReadFromJsonAsync<ResponseDTO<VentaDTO>>();

            return result!;
        }
    }
}

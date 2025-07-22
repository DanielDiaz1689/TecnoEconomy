using Ecommerce.DTO;
using Ecommerce.Client.Servicios.Contrato;
using System.Net.Http.Json;

namespace Ecommerce.Client.Servicios.Implementacion
{
    public class DashboardServicio : IDashboardServicio
    {
        private readonly HttpClient _httpclient;

        public DashboardServicio(HttpClient httpClient)
        {
            _httpclient = httpClient;
        }

        public async Task<ResponseDTO<DashboardDTO>> Resumen()
        {
            return await _httpclient.GetFromJsonAsync<ResponseDTO<DashboardDTO>>($"Dashboard/Resumen");
        }
    }
}

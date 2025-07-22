//using System.Net.Http.Headers;
//using Blazored.LocalStorage;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Ecommerce.Client.Extensiones
//{
//    public class AuthHttpMessageHandler : DelegatingHandler
//    {
//        private readonly ILocalStorageService _localStorage;

//        public AuthHttpMessageHandler(ILocalStorageService localStorage)
//        {
//            _localStorage = localStorage;
//        }

//        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            // ¡AÑADE ESTA LÍNEA DE LOGGING!
//            Console.WriteLine($"DEBUG (AuthHandler): Intentando enviar solicitud a: {request.RequestUri}");

//            var token = await _localStorage.GetItemAsStringAsync("token");

//            if (!string.IsNullOrWhiteSpace(token))
//            {
//                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
//                Console.WriteLine($"DEBUG (AuthHandler): Token JWT añadido a la solicitud para: {request.RequestUri}");
//            }
//            else
//            {
//                Console.WriteLine($"DEBUG (AuthHandler): No hay token JWT para la solicitud a: {request.RequestUri}");
//            }

//            return await base.SendAsync(request, cancellationToken);
//        }
//    }
//}
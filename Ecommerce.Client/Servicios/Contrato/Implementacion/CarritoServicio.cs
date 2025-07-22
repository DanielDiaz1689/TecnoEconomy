using Blazored.LocalStorage;
using Ecommerce.DTO;
using Ecommerce.Client.Servicios.Contrato;
using static System.Net.WebRequestMethods;

namespace Ecommerce.Client.Servicios.Implementacion
{
    public class CarritoServicio : ICarritoServicio
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly ISyncLocalStorageService _syncLocalStorageService;

        public CarritoServicio(
            ILocalStorageService localStorageService,
            ISyncLocalStorageService syncLocalStorageService)
        {
            _localStorageService = localStorageService;
            _syncLocalStorageService = syncLocalStorageService;
        }

        public event Action MostrarItems;

        public async Task AgregarCarrito(CarritoDTO modelo)
        {
            var carrito = await _localStorageService.GetItemAsync<List<CarritoDTO>>("carrito") ?? new List<CarritoDTO>();

            var encontrado = carrito.FirstOrDefault(c => c.Producto.PrId == modelo.Producto.PrId);
            if (encontrado != null)
                carrito.Remove(encontrado);

            carrito.Add(modelo);
            await _localStorageService.SetItemAsync("carrito", carrito);

            MostrarItems?.Invoke();
        }

        public int CantidadProductos()
        {
            var carrito = _syncLocalStorageService.GetItem<List<CarritoDTO>>("carrito");
            return carrito?.Count ?? 0;
        }

        public async Task<List<CarritoDTO>> DevolverCarrito()
        {
            return await _localStorageService.GetItemAsync<List<CarritoDTO>>("carrito") ?? new List<CarritoDTO>();
        }

        public async Task EliminarCarrito(int PrId)
        {
            var carrito = await _localStorageService.GetItemAsync<List<CarritoDTO>>("carrito");
            if (carrito != null)
            {
                var elemento = carrito.FirstOrDefault(c => c.Producto.PrId == PrId);
                if (elemento != null)
                {
                    carrito.Remove(elemento);
                    await _localStorageService.SetItemAsync("carrito", carrito);
                    MostrarItems?.Invoke();
                }
            }
        }

        public async Task VaciarCarrito()
        {
            await _localStorageService.RemoveItemAsync("carrito");

        }


        public async Task LimpiarCarrito()
        {
            await _localStorageService.RemoveItemAsync("carrito");
            MostrarItems?.Invoke();
        }
    }
}

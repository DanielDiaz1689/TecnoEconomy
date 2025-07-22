using Ecommerce.DTO;

namespace Ecommerce.Client.Servicios.Contrato
{
    public interface ICarritoServicio

        //permite mostrar el cambio de numeros para la cantidad de productos
    {
        event Action MostrarItems;

        int CantidadProductos();

        Task AgregarCarrito(CarritoDTO modelo);

        Task EliminarCarrito(int PrId);

        Task<List<CarritoDTO>> DevolverCarrito();

        Task LimpiarCarrito();

        Task VaciarCarrito();


    }
}

using Ecommerce.DTO;
using Microsoft.AspNetCore.Components.Forms;

namespace Ecommerce.Client.Servicios.Contrato
{
    public interface IProductoClienteServicio
    {
        Task<ResponseDTO<List<ProductoDTO>>> Catalogo(string buscar);
        Task<ResponseDTO<ProductoDTO>> Crear(ProductoDTO modelo);
        Task<ResponseDTO<ProductoDTO>> Obtener(int id);
        Task<ResponseDTO<ProductoDTO>> Editar(ProductoDTO modelo);
        Task<ResponseDTO<List<ProductoDTO>>> Lista(string buscar);
        Task<ResponseDTO<bool>> Eliminar(int id);

        Task<ResponseDTO<string>> SubirImagen(IBrowserFile archivo);

        Task<ResponseDTO<bool>> ImportarDesdeFakeStore();  //Task<ResponseDTO<bool>> ImportarDesdeFakeStore();

        Task<bool> AumentarSaldo(int idProducto, decimal cantidad);
    }
}

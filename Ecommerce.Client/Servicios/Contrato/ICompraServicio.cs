using Ecommerce.DTO;

namespace Ecommerce.Client.Servicios.Contrato
{
    public interface ICompraServicio
    {
        Task<ResponseDTO<CompraDTO>> Registrar(CompraDTO modelo);
    }

}

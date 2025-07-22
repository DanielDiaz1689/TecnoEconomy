using Ecommerce.DTO;
using System.Threading.Tasks;

namespace Ecommerce.Client.Servicios.Contrato
{
    public interface ILoginServicio
    {
        Task<ResponseDTO<SesionDTO>> ValidarUsuario(LoginDTO modelo);

        Task<ResponseDTO<SesionDTO>> Autorizacion(LoginDTO modelo);
    }
}

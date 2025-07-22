using AutoMapper;
using Microsoft.AspNetCore.Mvc;
// Usings que ya tienes y son necesarios:
using Ecommerce.DTO; // Para CompraDTO, CompraDetalleDTO, etc.
// using System.Security.Claims; 
using Ecommerce.Servicio.Contrato;
using BackendServices = Ecommerce.Servicio.Contrato; // Alias para el espacio de nombres de servicios de backend (Usuario)
using ClientServices = Ecommerce.Client.Servicios.Contrato; // Alias para el espacio de nombres de servicios de cliente (Compra, Producto)

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompraController : ControllerBase
    {
        // 🔴 Cualificar explícitamente el tipo para cada inyección
        private readonly ClientServices.ICompraServicio _compraServicio;    // Viene de ClientServices
        private readonly IProductoServicio _productoServicio;// Viene de ClientServices
        private readonly BackendServices.IUsuarioServicio _usuarioServicio;   // Viene de BackendServices
        private readonly IMapper _mapper;

        public CompraController(
            ClientServices.ICompraServicio compraServicio,    // Cualificado
            IProductoServicio productoServicio, // Cualificado
            BackendServices.IUsuarioServicio usuarioServicio,   // Cualificado
            IMapper mapper)
        {
            _compraServicio = compraServicio;
            _productoServicio = productoServicio;
            _usuarioServicio = usuarioServicio;
            _mapper = mapper;
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] CompraDTO modelo)
        {
            var response = new ResponseDTO<CompraDTO>();

            try
            {
                if (modelo.CompraTerId == null || modelo.CompraTerId == 0)
                {
                    response.EsCorrecto = false;
                    response.Mensaje = "Debe seleccionar un proveedor (tercero) válido.";
                    return Ok(response);
                }

                var proveedor = await _usuarioServicio.Obtener((int)modelo.CompraTerId);
                if (proveedor == null)
                {
                    response.EsCorrecto = false;
                    response.Mensaje = "El proveedor (tercero) no existe en el sistema.";
                    return Ok(response);
                }

                if (modelo.TblCompraDetalle == null || !modelo.TblCompraDetalle.Any())
                {
                    response.EsCorrecto = false;
                    response.Mensaje = "La compra debe contener al menos un producto.";
                    return Ok(response);
                }

                foreach (var detalle in modelo.TblCompraDetalle)
                {

                    var producto = await _productoServicio.Obtener(detalle.ComdetPrId);
                    if (producto == null)
                    {
                        response.EsCorrecto = false;
                        response.Mensaje = $"Producto con ID {detalle.ComdetPrId} no encontrado.";
                        return Ok(response);
                    }

                    var cantidad = detalle.CompdetCantidad ?? 0;
                    var valorUnd = detalle.CompdetValorUnd ?? 0;
                    var porcIva = detalle.CompdetPorcIva ?? 0;

                    detalle.CompdetSubtotal = cantidad * valorUnd;
                    detalle.CompdetValorIva = detalle.CompdetSubtotal * (porcIva / 100);
                    detalle.CompdetTotal = detalle.CompdetSubtotal + detalle.CompdetValorIva;
                }

                modelo.CompraSubtotal = modelo.TblCompraDetalle.Sum(x => x.CompdetSubtotal ?? 0);
                modelo.CompraValorIva = modelo.TblCompraDetalle.Sum(x => x.CompdetValorIva ?? 0);
                modelo.CompraTotal = modelo.TblCompraDetalle.Sum(x => x.CompdetTotal ?? 0);
                modelo.CompraFecha ??= DateTime.Now;
                modelo.CompraNegoId ??= 1;

                // Para CompraUsuId:
                // Si la persona que registra la compra es el usuario autenticado:
                // string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // if (int.TryParse(userId, out int currentUserId))
                // {
                //     modelo.CompraUsuId = currentUserId;
                // }
                // else
                // {
                //     response.EsCorrecto = false;
                //     response.Mensaje = "No se pudo identificar al usuario que registra la compra.";
                //     // Considera usar un código de estado 401 Unauthorized si es un requisito de autenticación.
                //     return Unauthorized(response);
                // }
                // Si CompraUsuId es opcional en tu DB o se maneja de otra forma:
                modelo.CompraUsuId = null; // O establece un valor por defecto si es necesario en la DB y no viene de auth


                
                var responseCompra = await _compraServicio.Registrar(modelo);

                
                if (responseCompra == null || !responseCompra.EsCorrecto || responseCompra.Resultado == null)
                {
                    response.EsCorrecto = false;
                    response.Mensaje = responseCompra?.Mensaje ?? "No se pudo registrar la compra.";
                    return Ok(response);
                }

                var compraRegistrada = responseCompra.Resultado; 

                foreach (var detalle in modelo.TblCompraDetalle)
                {
                   
                    if (detalle.CompdetCantidad.HasValue && detalle.ComdetPrId > 0)
                    {
                        await _productoServicio.AumentarSaldo(detalle.ComdetPrId, detalle.CompdetCantidad.Value);
                    }
                }

                response.EsCorrecto = true;
                response.Resultado = compraRegistrada;
            }
            catch (Exception ex)
            {
                response.EsCorrecto = false;
                response.Mensaje = $"Error al registrar la compra: {ex.Message}";
                // Aquí deberías loguear 'ex' completo para propósitos de depuración.
            }

            return Ok(response);
        }
    }
}

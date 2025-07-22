using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Servicio.Contrato;
using Ecommerce.DTO;
using System.Text.Json;
using Ecommerce.Modelo;
using AutoMapper;

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {
        private readonly IVentaServicio _ventaServicio;
        private readonly IProductoServicio _productoServicio;
        private readonly IUsuarioServicio _usuarioServicio;
        private readonly IMapper _mapper;

        public VentaController(IVentaServicio ventaServicio, IProductoServicio productoServicio, IUsuarioServicio usuarioservicio, IMapper mapper)
        {
            _ventaServicio = ventaServicio;
            _productoServicio = productoServicio;
            _usuarioServicio = usuarioservicio;
            _mapper = mapper;
        }


        [HttpPost("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] VentaDTO modelo)
        {
            System.Diagnostics.Debug.WriteLine("JSON recibido:");
            System.Diagnostics.Debug.WriteLine(JsonSerializer.Serialize(modelo, new JsonSerializerOptions { WriteIndented = true }));

            var response = new ResponseDTO<VentaRespuestaDTO>();

            try
            {
                if (modelo.VentaUsuId == 0)
                {
                    response.EsCorrecto = false;
                    response.Mensaje = "Debe proporcionar un ID de usuario válido para la venta.";
                    return Ok(response);
                }

                var usuario = await _usuarioServicio.Obtener((int)modelo.VentaUsuId);
                if (usuario == null)
                {
                    response.EsCorrecto = false;
                    response.Mensaje = "El usuario que realiza la venta no existe.";
                    return Ok(response);
                }

                // Verifica stock y calcula valores de cada detalle
                foreach (var detalle in modelo.TblVentaDetalle)
                {
                    var producto = await _productoServicio.Obtener(detalle.VendetPrId ?? 0);

                    if (producto == null || detalle.VendetCantidad == null || producto.Resultado?.PrSaldo < detalle.VendetCantidad.Value)
                    {
                        response.EsCorrecto = false;
                        response.Mensaje = $"Stock insuficiente para el producto con ID {detalle.VendetPrId}";
                        return Ok(response);
                    }

                    var cantidad = detalle.VendetCantidad ?? 0;
                    var valorUnd = detalle.VendetValorUnd ?? 0;
                    var porcIva = detalle.VendetPorcIva ?? 0;

                    detalle.VendetSubtotal = cantidad * valorUnd;
                    detalle.VendetValorIva = detalle.VendetSubtotal * (porcIva / 100);
                    detalle.VendetTotal = detalle.VendetSubtotal + detalle.VendetValorIva;
                }

                modelo.VentaSubtotal = modelo.TblVentaDetalle.Sum(d => d.VendetSubtotal ?? 0);
                modelo.VentaValorIva = modelo.TblVentaDetalle.Sum(d => d.VendetValorIva ?? 0);
                modelo.VentaTotal = modelo.TblVentaDetalle.Sum(d => d.VendetTotal ?? 0);

                modelo.VentaEstado ??= "PAGADO";
                modelo.VentaFecha ??= DateTime.Now;
                modelo.VentaNegoId ??= 1;
                modelo.VentaTerId ??= 1;

                var ventaRegistrada = await _ventaServicio.Registrar(modelo);

                if (ventaRegistrada == null)
                {
                    response.EsCorrecto = false;
                    response.Mensaje = "No se pudo registrar la venta. El servicio devolvió null.";
                    return Ok(response);
                }

                foreach (var detalle in modelo.TblVentaDetalle)
                {
                    if (detalle.VendetCantidad is not null)
                    {
                        await _productoServicio.ActualizarSaldo(detalle.VendetPrId ?? 0, detalle.VendetCantidad.Value);
                    }
                }

                // Proyección limpia sin ciclos
                var ventaRespuesta = await _ventaServicio.ObtenerVentaRespuestaDTO(ventaRegistrada.VentaId);

                response.EsCorrecto = true;
                response.Resultado = ventaRespuesta;
            }
            catch (Exception ex)
            {
                response.EsCorrecto = false;
                response.Mensaje = ex.Message;
            }

            return Ok(response);
        }


    }
}



using Microsoft.AspNetCore.Mvc;
using Ecommerce.Servicio.Contrato;
using Ecommerce.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] 
    public class ProductoController : ControllerBase
    {
        private readonly IProductoServicio _productoServicio;

        public ProductoController(IProductoServicio productoServicio)
        {
            _productoServicio = productoServicio;
        }

        
        [HttpGet("Lista")]
        // [AllowAnonymous] // ELIMINADO O COMENTADO
        public async Task<IActionResult> Lista([FromQuery] string buscar = "")
        {
            Console.WriteLine($"DEBUG (Backend ProductoController): Recibida solicitud para /api/Producto/Lista con buscar='{buscar}'");
            try
            {
                if (buscar == "NA") buscar = "";

                var servicioResponse = await _productoServicio.Lista(buscar);

                if (servicioResponse == null)
                {
                    Console.Error.WriteLine("ERROR (Backend ProductoController): El servicio de producto devolvió una respuesta nula.");
                    return StatusCode(500, new ResponseDTO<List<ProductoDTO>>
                    {
                        EsCorrecto = false,
                        Mensaje = "El servicio de producto devolvió una respuesta nula."
                    });
                }

                return Ok(servicioResponse);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR (Backend ProductoController): Excepción en Lista: {ex.Message}");
                return StatusCode(500, new ResponseDTO<List<ProductoDTO>>
                {
                    EsCorrecto = false,
                    Mensaje = ex.Message,
                    Resultado = null
                });
            }
        }

        [HttpPost("AumentarSaldo")]
        public async Task<IActionResult> AumentarSaldo([FromBody] Dictionary<string, object> data)
        {
            var response = new ResponseDTO<bool>();
            try
            {
                if (!data.TryGetValue("IdProducto", out var idProductoObj) ||
                    !data.TryGetValue("Cantidad", out var cantidadObj) ||
                    !int.TryParse(idProductoObj?.ToString(), out int idProducto) ||
                    !decimal.TryParse(cantidadObj?.ToString(), out decimal cantidad))
                {
                    response.EsCorrecto = false;
                    response.Mensaje = "Datos de entrada inválidos para AumentarSaldo.";
                    return BadRequest(response);
                }

                // Llamar al servicio de backend para aumentar el saldo
                var exito = await _productoServicio.AumentarSaldo(idProducto, cantidad);

                response.EsCorrecto = exito;
                response.Resultado = exito;
                response.Mensaje = exito ? "Saldo aumentado correctamente." : "Error al aumentar el saldo o producto no encontrado.";
            }
            catch (Exception ex)
            {
                response.EsCorrecto = false;
                response.Mensaje = $"Error al aumentar saldo: {ex.Message}";
            }
            return Ok(response);
        }



        // GET: /api/Producto/ImportarDesdeFakeStore
        [HttpGet("ImportarDesdeFakeStore")]
        // [Authorize(Roles = "administrador")] // ELIMINADO O COMENTADO
        public async Task<IActionResult> ImportarDesdeFakeStore()
        {
            var response = new ResponseDTO<bool>();
            try
            {
                var servicioResponse = await _productoServicio.ImportarDesdeFakeStore(10, "electronics");
                response.EsCorrecto = servicioResponse.EsCorrecto;
                response.Mensaje = servicioResponse.Mensaje;
                response.Resultado = servicioResponse.Resultado;
            }
            catch (Exception ex)
            {
                response.EsCorrecto = false;
                response.Mensaje = $"Error al importar desde FakeStore: {ex.Message}";
            }
            return Ok(response);
        }

        // GET: /api/Producto/Obtener/{Id}
        [HttpGet("Obtener/{Id:int}")]
        // [AllowAnonymous] // ELIMINADO O COMENTADO
        public async Task<IActionResult> Obtener(int Id)
        {
            try
            {
                var servicioResponse = await _productoServicio.Obtener(Id);
                return Ok(servicioResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<ProductoDTO>
                {
                    EsCorrecto = false,
                    Mensaje = ex.Message
                });
            }
        }

        // POST: /api/Producto/Crear
        [HttpPost("Crear")]
        // [Authorize(Roles = "administrador")] // ELIMINADO O COMENTADO
        public async Task<IActionResult> Crear([FromBody] ProductoDTO modelo)
        {
            // Logging de depuración para autorización (mantenido solo como ejemplo, ya no es relevante si [Authorize] se quita)
            if (User.Identity?.IsAuthenticated == true)
            {
                Console.WriteLine($"DEBUG (ProductoController): Usuario autenticado: {User.Identity.Name}");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"DEBUG (ProductoController): Claim: {claim.Type} = {claim.Value}");
                }
                if (User.IsInRole("administrador"))
                {
                    Console.WriteLine("DEBUG (ProductoController): Usuario está en el rol 'administrador'.");
                }
                else
                {
                    Console.WriteLine("DEBUG (ProductoController): Usuario NO está en el rol 'administrador'.");
                }
            }
            else
            {
                Console.WriteLine("DEBUG (ProductoController): Usuario NO autenticado.");
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new ResponseDTO<ProductoDTO>
                {
                    EsCorrecto = false,
                    Mensaje = string.Join(" | ", errores)
                });
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    modelo.PrUsuId = userId;
                }

                var servicioResponse = await _productoServicio.Crear(modelo);

                if (!servicioResponse.EsCorrecto)
                    return BadRequest(servicioResponse);

                return Ok(servicioResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<ProductoDTO>
                {
                    EsCorrecto = false,
                    Mensaje = GetFullErrorMessage(ex)
                });
            }
        }

        // PUT: /api/Producto/Editar
        // MÉTODO QUE NECESITAS MODIFICAR PARA LA EDICIÓN
        [HttpPut("editar")] // O [HttpPut("{id}")] si el ID viene en la ruta
        public async Task<IActionResult> EditarProducto([FromBody] ProductoDTO productoActualizadoDto)
        {
            // 1. Validaciones básicas si no se hacen con Data Annotations
            if (!ModelState.IsValid)
            {
                return BadRequest(new { esCorrecto = false, mensaje = "Datos de producto inválidos." });
            }

            try
            {
                // 2. Lógica para actualizar el producto en la base de datos
                // Aquí llamarías a tu servicio para guardar los cambios.
                // Asumo que tu _productoService.UpdateProductoAsync devuelve el ProductoDTO actualizado
                // o un booleano/estado de éxito.
                var productoEditado = await _productoServicio.Editar(productoActualizadoDto);

                if (productoEditado == null) // Si el servicio devuelve null si no se encontró o no se pudo actualizar
                {
                    return NotFound(new { esCorrecto = false, mensaje = "Producto no encontrado para actualizar." });
                }

                // 3. LA CLAVE: Devolver el ProductoDTO actualizado en el campo "resultado"
                return Ok(new
                {
                    resultado = productoEditado, // <-- ¡Aquí está el cambio clave!
                    esCorrecto = true,
                    mensaje = "Producto editado correctamente"
                });
            }
            catch (Exception ex)
            {
                // Logear la excepción (muy importante para depuración)
                // _logger.LogError(ex, "Error al editar producto.");
                return StatusCode(500, new { esCorrecto = false, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }


        // DELETE: /api/Producto/Eliminar/{Id}
        [HttpDelete("Eliminar/{Id:int}")]
        // [Authorize(Roles = "administrador")] // ELIMINADO O COMENTADO
        public async Task<IActionResult> Eliminar(int Id)
        {
            try
            {
                var servicioResponse = await _productoServicio.Eliminar(Id);
                return Ok(servicioResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<bool>
                {
                    EsCorrecto = false,
                    Mensaje = ex.Message
                });
            }
        }

        private string GetFullErrorMessage(Exception ex)
        {
            if (ex == null) return string.Empty;
            return $"{ex.Message} {(ex.InnerException != null ? GetFullErrorMessage(ex.InnerException) : "")}";
        }
    }
}

//[HttpGet("Catalogo/{categoria:alpha}/{buscar:alpha?}")]
//public async Task<IActionResult> Catalogo(string categoria, string buscar = "NA")
//{
//    var response = new ResponseDTO<List<ProductoDTO>>();
//
//    try
//    {
//        if (categoria.ToLower() == "todos") categoria = "";
//        if (buscar == "NA") buscar = "";
//
//        var servicioResponse = await _poductoServicio.Catalogo(categoria, buscar);
//
//        response.EsCorrecto = servicioResponse.EsCorrecto;
//        response.Mensaje = servicioResponse.Mensaje;
//        response.Resultado = servicioResponse.Resultado;
//    }
//    catch (Exception ex)
//    {
//        response.EsCorrecto = false;
//        response.Mensaje = ex.Message;
//    }
//
//    return Ok(response);
//}

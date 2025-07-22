using Microsoft.AspNetCore.Mvc;

using Ecommerce.Servicio.Contrato;

using Ecommerce.DTO;

// using Microsoft.AspNetCore.Authorization; // Ya no necesario si quitamos todos los [Authorize]

using System.Security.Claims;

using Microsoft.Extensions.Configuration;



namespace Ecommerce.Controllers

{

    [Route("api/[controller]")]

    [ApiController]

    // [Authorize] // Eliminado o comentado, ya no se usa a nivel de controlador

    public class UsuarioController : ControllerBase

    {

        private readonly IUsuarioServicio _usuarioServicio;



        public UsuarioController(IUsuarioServicio usuarioServicio)

        {

            _usuarioServicio = usuarioServicio;

        }



        // GET: /api/Usuario/Lista

        [HttpGet("Lista")]

        // [Authorize(Roles = "administrador")] // ELIMINADO O COMENTADO

        public async Task<IActionResult> Lista([FromQuery] string rol = "", [FromQuery] string buscar = "")

        {

            try

            {

                Console.WriteLine($"Rol recibido: {rol}, Buscar: {buscar}");

                var response = await _usuarioServicio.ListaCompleta(rol, buscar);

                return Ok(response);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new ResponseDTO<List<UsuarioDTO>>

                {

                    EsCorrecto = false,

                    Mensaje = ex.Message,

                    Valor = null

                });

            }

        }



        // GET: /api/Usuario/Obtener/{Id}

        [HttpGet("Obtener/{Id:int}")]

        // [Authorize(Roles = "administrador")] // ELIMINADO O COMENTADO

        public async Task<IActionResult> Obtener(int Id)

        {

            try

            {

                var response = await _usuarioServicio.Obtener(Id);

                return Ok(response);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new ResponseDTO<UsuarioDTO>

                {

                    EsCorrecto = false,

                    Mensaje = ex.Message

                });

            }

        }



        // POST: /api/Usuario/Crear - PARA REGISTRAR UN NUEVO USUARIO (SIEMPRE ANÓNIMO)

        [HttpPost("Crear")]

        // [AllowAnonymous] // ELIMINADO O COMENTADO (si no hay [Authorize] global, AllowAnonymous es redundante)

        public async Task<IActionResult> Crear([FromBody] UsuarioDTO modelo)

        {

            if (!ModelState.IsValid)

            {

                var errores = ModelState.Values

                  .SelectMany(v => v.Errors)

                  .Select(e => e.ErrorMessage)

                  .ToList();

                return BadRequest(new ResponseDTO<UsuarioDTO>

                {

                    EsCorrecto = false,

                    Mensaje = string.Join(" | ", errores)

                });

            }

            try

            {

                var response = await _usuarioServicio.Crear(modelo);

                if (!response.EsCorrecto)

                    return BadRequest(response);

                return Ok(response);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new ResponseDTO<UsuarioDTO>

                {

                    EsCorrecto = false,

                    Mensaje = GetFullErrorMessage(ex)

                });

            }

        }





        [HttpGet("por-cedula/{cedula}")]

        // [Authorize(Roles = "administrador,cliente")] // ELIMINADO O COMENTADO

        public async Task<IActionResult> ObtenerPorCedula(string cedula)

        {

            Console.WriteLine($"🔍 Buscando cliente con cédula: {cedula}");

            var responseDTO = new ResponseDTO<UsuarioDTO>();



            try

            {

                var servicioResponse = await _usuarioServicio.ObtenerPorCedula(cedula);



                if (servicioResponse.EsCorrecto && servicioResponse.Resultado != null)

                {

                    responseDTO.EsCorrecto = true;

                    responseDTO.Resultado = servicioResponse.Resultado;

                    responseDTO.Mensaje = servicioResponse.Mensaje;

                }

                else

                {

                    responseDTO.EsCorrecto = false;

                    responseDTO.Mensaje = servicioResponse.Mensaje ?? "Usuario no encontrado.";

                    responseDTO.Resultado = null;

                }

            }

            catch (Exception ex)

            {

                responseDTO.EsCorrecto = false;

                responseDTO.Mensaje = ex.Message;

            }

            return Ok(responseDTO);

        }



        // POST: /api/Usuario/autenticar - PARA INICIAR SESIÓN (SIEMPRE ANÓNIMO)

        [HttpPost("autenticar")]

        // [AllowAnonymous] // ELIMINADO O COMENTADO

        public async Task<IActionResult> Autenticar([FromBody] LoginDTO modelo)

        {

            try

            {

                var response = await _usuarioServicio.Autorizacion(modelo);

                return Ok(response);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new ResponseDTO<SesionDTO>

                {

                    EsCorrecto = false,

                    Mensaje = ex.Message

                });

            }

        }



        // PUT: /api/Usuario/Editar

        [HttpPut("Editar")]

        // [Authorize(Roles = "administrador")] // ELIMINADO O COMENTADO

        public async Task<IActionResult> Editar([FromBody] UsuarioDTO modelo)

        {

            try

            {

                var response = await _usuarioServicio.Editar(modelo);

                return Ok(response);

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



        // DELETE: /api/Usuario/Eliminar/{Id}

        [HttpDelete("Eliminar/{Id:int}")]
        // [Authorize(Roles = "administrador")] // Asegúrate de que esto esté COMENTADO por ahora
        public async Task<IActionResult> Eliminar(int Id)
        {
            try
            {
                // La llamada a tu servicio de backend (Ecommerce.Servicio.Implementacion.UsuarioServicio)
                // debe ser la que contenga la lógica para llamar al repositorio y manejar las excepciones de DB.
                var resultado = await _usuarioServicio.Eliminar(Id); // <-- Aquí se espera un `bool`

                // Si Eliminar devuelve false, significa que la operación no fue exitosa.
                // PERO si lanza una excepción (como por la FK), el catch la debe atrapar.
                if (resultado) // Si el servicio devuelve true, la eliminación fue exitosa.
                {
                    return Ok(new ResponseDTO<bool>
                    {
                        EsCorrecto = true,
                        Valor = resultado, // Debería ser true
                        Mensaje = "El usuario fue eliminado correctamente"
                    });
                }
                else
                {
                    // Este caso debería ocurrir si el servicio devuelve false explícitamente,
                    // pero si hay una excepción de FK, el catch de este controlador debería atraparla.
                    return BadRequest(new ResponseDTO<bool>
                    {
                        EsCorrecto = false,
                        Valor = resultado, // Debería ser false
                        Mensaje = "No fue posible eliminar el usuario por una razón no especificada."
                    });
                }
            }
            catch (Exception ex)
            {
                // ESTE ES EL BLOQUE CRÍTICO: Captura la excepción de la DB y devuelve un ResponseDTO<bool> de error.
                Console.WriteLine($"Error en UsuarioController.Eliminar: {ex.Message}"); // Para depuración en la consola del servidor

                // Puedes personalizar el mensaje de error para el usuario aquí
                string mensajeErrorUsuario = "No fue posible eliminar el usuario. Es posible que tenga ventas registradas o esté relacionado con otros datos.";

                // Si quieres el mensaje específico de la excepción para depuración:
                // if (ex is DbUpdateException dbEx && dbEx.InnerException != null)
                // {
                //     mensajeErrorUsuario = $"Error de base de datos: {dbEx.InnerException.Message}";
                // }
                // else
                // {
                //     mensajeErrorUsuario = $"Error interno del servidor: {ex.Message}";
                // }


                return StatusCode(500, new ResponseDTO<bool>
                {
                    EsCorrecto = false,
                    Mensaje = mensajeErrorUsuario,
                    Valor = false
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

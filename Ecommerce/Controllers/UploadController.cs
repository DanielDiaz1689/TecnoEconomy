using Ecommerce.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }


        [ApiExplorerSettings(IgnoreApi = true)]

        [HttpPost("imagen")]
        public async Task<IActionResult> SubirImagen([FromForm] IFormFile archivo)
        {
            var response = new ResponseDTO<string>();

            try
            {
                Console.WriteLine("➡️ Se recibió una solicitud para subir imagen.");

                if (archivo == null || archivo.Length == 0)
                {
                    Console.WriteLine("⚠️ El archivo está vacío o es nulo.");
                    response.EsCorrecto = false;
                    response.Mensaje = "Archivo no válido";
                    return BadRequest(response);
                }

                Console.WriteLine($"📁 Nombre original del archivo: {archivo.FileName}");
                Console.WriteLine($"📦 Tamaño: {archivo.Length} bytes");

                string carpeta = Path.Combine(_env.WebRootPath, "imagenes");

                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                    Console.WriteLine("📂 Carpeta 'imagenes' creada.");
                }

                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
                string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                Console.WriteLine($"📝 Guardando imagen en: {rutaCompleta}");

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                string rutaRelativa = $"imagenes/{nombreArchivo}";
                Console.WriteLine($"✅ Imagen guardada. Ruta relativa: {rutaRelativa}");

                response.EsCorrecto = true;
                response.Valor = rutaRelativa;
                response.Mensaje = "Imagen subida correctamente";

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al subir imagen: {ex.Message}");
                response.EsCorrecto = false;
                response.Mensaje = $"Error al subir la imagen: {ex.Message}";
                return StatusCode(500, response);
            }
        }

    }
}

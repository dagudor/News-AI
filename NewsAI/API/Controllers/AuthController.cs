using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NewsAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly ILogger<AuthController> logger;

        public AuthController(AppDbContext context, ILogger<AuthController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        [HttpPost("registro")]
        public async Task<ActionResult<object>> Registro([FromBody] RegistroRequest request)
        {
            try
            {
                logger.LogInformation($"Intento de registro para usuario: {request.Login}");

                // Validaciones básicas
                if (string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Email) ||
                    string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Todos los campos son requeridos"
                    });
                }

                if (request.Login.Length < 3)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre de usuario debe tener al menos 3 caracteres"
                    });
                }

                if (request.Password.Length < 6)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La contraseña debe tener al menos 6 caracteres"
                    });
                }

                // Verificar si el usuario o email ya existen
                var usuarioExistente = await context.Usuarios
                    .FirstOrDefaultAsync(u => u.Login == request.Login || u.Email == request.Email);

                if (usuarioExistente != null)
                {
                    var campo = usuarioExistente.Login == request.Login ? "nombre de usuario" : "email";
                    return BadRequest(new
                    {
                        success = false,
                        message = $"El {campo} ya está en uso"
                    });
                }

                // Crear nuevo usuario
                var nuevoUsuario = new Dominio.Entidades.Usuario
                {
                    Nombre = request.Nombre.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    Login = request.Login.Trim(),
                    Password = request.Password, // hashear la contraseña para seeguridad
                    FechaAlta = DateTime.Now
                };

                context.Usuarios.Add(nuevoUsuario);
                await context.SaveChangesAsync();

                logger.LogInformation($"Usuario registrado correctamente: {request.Login}");

                return Ok(new
                {
                    success = true,
                    message = "Usuario creado correctamente",
                    user = new
                    {
                        id = nuevoUsuario.Id,
                        nombre = nuevoUsuario.Nombre,
                        email = nuevoUsuario.Email,
                        login = nuevoUsuario.Login
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error durante el registro para usuario: {request.Login}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
        {
            try
            {
                logger.LogInformation($"Intento de login para usuario: {request.Login}");

                if (string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Login y contraseña son requeridos"
                    });
                }

                // Buscar usuario por login
                var usuario = await context.Usuarios
                    .FirstOrDefaultAsync(u => u.Login == request.Login);

                if (usuario == null)
                {
                    logger.LogWarning($"Usuario no encontrado: {request.Login}");
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Credenciales incorrectas"
                    });
                }

                // Verificación simple de contraseña (en producción usa hash)
                if (usuario.Password != request.Password)
                {
                    logger.LogWarning($"Contraseña incorrecta para usuario: {request.Login}");
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Credenciales incorrectas"
                    });
                }

                // Login exitoso
                logger.LogInformation($"Login exitoso para usuario: {request.Login}");

                return Ok(new
                {
                    success = true,
                    message = "Login exitoso",
                    user = new
                    {
                        id = usuario.Id,
                        nombre = usuario.Nombre,
                        email = usuario.Email,
                        login = usuario.Login
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error durante el login para usuario: {request.Login}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public ActionResult<object> Logout()
        {
            // Por ahora solo confirmamos el logout
            // En el futuro podrías invalidar tokens aquí
            return Ok(new
            {
                success = true,
                message = "Logout exitoso"
            });
        }

        // GET: api/auth/check
        [HttpGet("check")]
        public ActionResult<object> CheckAuth()
        {
            // Endpoint simple para verificar si el usuario está logueado
            // Por ahora siempre devuelve false, el frontend manejará el estado
            return Ok(new
            {
                success = true,
                authenticated = false,
                message = "Verificación de autenticación"
            });
        }
    }

    // Modelo para la request de registro
    public class RegistroRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Modelo para la request de login
    public class LoginRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
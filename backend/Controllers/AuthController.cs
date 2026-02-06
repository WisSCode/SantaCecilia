using backend.DTOs.Auth;
using backend.Models;
using backend.Services;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    //REGISTER
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { Message = "Email y contraseña son obligatorios" });
        }

        try
        {
            //Crear usuario en Firebase Auth
            var userRecord = await FirebaseAuth.DefaultInstance
                .CreateUserAsync(new UserRecordArgs
                {
                    Email = dto.Email,
                    Password = dto.Password,
                    EmailVerified = false
                });

            //Crear usuario en Firestore
            var user = new Users
            {
                Email = dto.Email,
                Role = "user",
                Validated = false,
                CreatedAt = Timestamp.GetCurrentTimestamp()
            };

            await _userService.CreateAsync(userRecord.Uid, user);

            _logger.LogInformation($"Usuario registrado exitosamente: {dto.Email}");

            // 3️⃣ Retornar respuesta exitosa
            return Ok(new
            {
                UserId = userRecord.Uid,
                Email = dto.Email,
                Role = "user",
                Token = "",
                Message = "Usuario registrado exitosamente. Espera la validación del administrador."
            });
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogError($"Error en Firebase Auth: {ex.Message}");
            
            string errorMessage = ex.Message;
            
            if (ex.Message.Contains("EMAIL_EXISTS") || ex.Message.Contains("already exists"))
                errorMessage = "Este email ya está registrado";
            else if (ex.Message.Contains("INVALID_EMAIL"))
                errorMessage = "Email inválido";
            else if (ex.Message.Contains("WEAK_PASSWORD"))
                errorMessage = "La contraseña es muy débil (mínimo 6 caracteres)";
            else
                errorMessage = "Error al registrar usuario";
            
            
            return BadRequest(new { Message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error inesperado: {ex.Message}");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    
    //LOGIN
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.IdToken))
        {
            return BadRequest(new { Message = "Token requerido" });
        }

        try
        {
            // 1️⃣ Validar token con Firebase
            FirebaseToken decodedToken =
                await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(dto.IdToken);

            string uid = decodedToken.Uid;

            //Buscar usuario en Firestore
            var user = await _userService.GetAsync(uid);

            if (user == null)
            {
                _logger.LogWarning($"Usuario no encontrado en Firestore: {uid}");
                return Unauthorized(new { Message = "Usuario no registrado en el sistema" });
            }

            if (!user.Validated)
            {
                _logger.LogWarning($"Usuario no validado: {user.Email}");
                return Unauthorized(new { Message = "Usuario no validado por el administrador" });
            }

            _logger.LogInformation($"Login exitoso: {user.Email}");

            //Login exitoso
            return Ok(new
            {
                UserId = uid,
                Email = user.Email,
                Role = user.Role
            });
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogError($"Error validando token: {ex.Message}");
            return Unauthorized(new { Message = "Token inválido o expirado" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error inesperado en login: {ex.Message}");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }
}
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Enums;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace Dsw2025Tpi.Api.Controllers 
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly TokenService _tokenService;

        public AuthenticateController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            TokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized("Usuario no encontrado");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Contraseña incorrectos");
            }

            var token = await _tokenService.GenerateToken(user);
            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!Enum.TryParse<Roles>(model.Role, true, out var parsedRole))
            {
                return BadRequest("Rol invalido");
            }

            var user = new IdentityUser { UserName = model.Username, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var assignRoleResult = await _userManager.AddToRoleAsync(user, parsedRole.ToString());

            if (!assignRoleResult.Succeeded)
            {
                return BadRequest(assignRoleResult.Errors);
            }

            return Ok(new
            {
                message = "Usuario registrado correctamente",
                role = parsedRole
            });
        }

        [HttpGet("users")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<UserResponseModel>>> GetAllUsuarios()
        {
            var users = await _userManager.Users.ToListAsync();

            var userResponses = new List<UserResponseModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userResponses.Add(new UserResponseModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return Ok(userResponses);
        }

    }
}
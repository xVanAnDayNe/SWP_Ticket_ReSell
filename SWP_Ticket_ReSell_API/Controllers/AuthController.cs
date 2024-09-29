using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Authentication;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
            private readonly IConfiguration _configuration;
            private readonly ServiceBase<Customer> _service;
            private readonly ServiceBase<Role>  _serviceRole;

            public AuthController(IConfiguration configuration, ServiceBase<Customer> service, ServiceBase<Role> serviceRole)
            {
                _configuration = configuration;
                _service = service;
                _serviceRole = serviceRole;
            }

            [HttpPost("Login")]
            public async Task<ActionResult> Login(LoginRequestDTO login)
            {
                var user = await _service
                    .FindByAsync(x => x.Email == login.Email &&
                                      x.Password == login.Password);
                if (user == null)
                {
                    return Unauthorized();
                }

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Email.ToString()),
                    new Claim(ClaimTypes.Role, user.ID_Role.ToString()!)
                //role
                };
                var key = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:SerectKey").Value!));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
                long expiredToken = 30;
                var token = new JwtSecurityToken(
                     claims: claims,
                     expires: DateTime.UtcNow.AddMinutes(expiredToken),
                     signingCredentials: creds);
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                //return Ok(new TokenRequest(jwt, user.Role));
                return Ok(new AccessTokenResponse { AccessToken = jwt, ExpiresIn = expiredToken });
            }
        [HttpPost("Register")]
        public async Task<ActionResult<RegisterResponseDTO>> Register(RegisterRequestDTO request)
        {
            if (await _service.ExistsByAsync(p => p.Email.Equals(request.Email)))
            {
                return Problem(detail: $"Email {request.Email} already exists", statusCode: 400);
            }
            if (request.Password != request.ConfirmPassWord)
            {
                return Problem(detail: $"Password and Confirm Password different", statusCode: 400);
            }
            var customer = new Customer();
            request.Adapt(customer); 
            await _service.CreateAsync(customer);
            return Ok("Create customer successfull.");
        }
    }
}


using CozyPlaceSG.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CozyPlaceSG.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        // Helper method to parse input data based on content type
        private async Task<T?> ParseInputDataAsync<T>()
        {
            if (Request.ContentType == "application/json")
            {
                var requestBody = string.Empty;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                return JsonConvert.DeserializeObject<T>(requestBody);
            }
            else if (Request.ContentType == "application/x-www-form-urlencoded")
            {
                var formData = await Request.ReadFormAsync();
                var json = JsonConvert.SerializeObject(formData.ToDictionary(x => x.Key, x => x.Value.ToString()));
                return JsonConvert.DeserializeObject<T>(json);
            }

            return default;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login()
        {
            var inputData = await ParseInputDataAsync<LoginModel>();

            if (inputData == null || string.IsNullOrEmpty(inputData.Username) || string.IsNullOrEmpty(inputData.Password))
            {
                return BadRequest(new Response { Status = "Error", Message = "Invalid input data." });
            }

            var user = await _userManager.FindByNameAsync(inputData.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, inputData.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("register-member")]
        public async Task<IActionResult> Register()
        {
            var inputData = await ParseInputDataAsync<RegisterModel>();

            if (inputData == null || string.IsNullOrEmpty(inputData.Username) || string.IsNullOrEmpty(inputData.Email) || string.IsNullOrEmpty(inputData.Password))
            {
                return BadRequest(new Response { Status = "Error", Message = "Invalid input data." });
            }

            var userExists = await _userManager.FindByNameAsync(inputData.Username);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "User already exists!" });
            }

            IdentityUser user = new()
            {
                Email = inputData.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = inputData.Username
            };

            var result = await _userManager.CreateAsync(user, inputData.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.member))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.member));

            if (await _roleManager.RoleExistsAsync(UserRoles.member))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.member);
            }

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost("register-administrator")]
        public async Task<IActionResult> RegisterAdmin()
        {
            var inputData = await ParseInputDataAsync<RegisterModel>();

            if (inputData == null || string.IsNullOrEmpty(inputData.Username) || string.IsNullOrEmpty(inputData.Email) || string.IsNullOrEmpty(inputData.Password))
            {
                return BadRequest(new Response { Status = "Error", Message = "Invalid input data." });
            }

            var userExists = await _userManager.FindByNameAsync(inputData.Username);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "User already exists!" });
            }

            IdentityUser user = new()
            {
                Email = inputData.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = inputData.Username
            };

            var result = await _userManager.CreateAsync(user, inputData.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.Administrator))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Administrator));

            if (await _roleManager.RoleExistsAsync(UserRoles.Administrator))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Administrator);
            }

            return Ok(new Response { Status = "Success", Message = "Administrator created successfully!" });
        }
    }
}

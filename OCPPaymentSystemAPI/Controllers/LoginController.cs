using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Helpers;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly LoginData _loginData;
        private readonly IConfiguration _configuration;

        public LoginController(
            LoginData loginData,
            IConfiguration configuration)
        {
            _loginData = loginData;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetServerHostName")]
        public IActionResult GetServerHostName()
        {
            return Ok(new
            {
                BaseUrl = AppEnvironment.GetBaseUrl(),
                IsProduction = AppEnvironment.IsProductionServer(),
                IsQAS = AppEnvironment.IsQASServer()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            try
            {
                LoginResponse result =
                    await _loginData.LoginAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
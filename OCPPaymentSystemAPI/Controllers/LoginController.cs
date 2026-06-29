using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly LoginData _loginData;

        public LoginController(
            LoginData loginData)
        {
            _loginData = loginData;
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
using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Helpers;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly IConfiguration _configuration;

        public BaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected bool ValidateApiKey()
        {
            if (!Request.Headers.TryGetValue("x-api-key", out var apiKey))
                return false;

            return ApiKeyHelper.Validate(
                _configuration,
                apiKey!);
        }

        protected IActionResult ApiKeyError()
        {
            return Unauthorized(
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid API Key"
                });
        }

        protected IActionResult Success(object? data)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Success",
                Data = data
            });
        }

        protected IActionResult Failed(string message)
        {
            return BadRequest(
                new ApiResponse<object>
                {
                    Success = false,
                    Message = message
                });
        }
    }
}

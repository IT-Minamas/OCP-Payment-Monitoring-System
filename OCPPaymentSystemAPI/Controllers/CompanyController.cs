using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : BaseController
    {
        private readonly CompanyData _data;

        public CompanyController(
            IConfiguration configuration,
            CompanyData data)
            : base(configuration)
        {
            _data = data;
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IActionResult> Search(
            [FromBody] CompanyRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var result =
                    await _data.SearchAsync();

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }
    }
}
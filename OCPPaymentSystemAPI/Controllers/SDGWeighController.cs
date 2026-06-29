using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SDGWeighController : BaseController
    {
        private readonly SDGWeighData _data;

        public SDGWeighController(
            IConfiguration configuration,
            SDGWeighData data)
            : base(configuration)
        {
            _data = data;
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IActionResult> Search(
            [FromBody] SDGWeighRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var result = await _data.SearchAsync(
                    request.SupplierCode,
                    request.DateFrom,
                    request.DateTo);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }
    }
}
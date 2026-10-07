using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApproverController : BaseController
    {
        private readonly ApproverData _data;

        public ApproverController(
            IConfiguration configuration,
            ApproverData data)
            : base(configuration)
        {
            _data = data;
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IActionResult> Search(
            [FromBody] ApproverRequest request)
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
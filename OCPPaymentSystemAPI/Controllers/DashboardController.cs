using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly DashboardData _data;


        public DashboardController(
            IConfiguration configuration,
            DashboardData data)
            : base(configuration)
        {
            _data = data;
        }



        [HttpPost]
        [Route("Summary")]

        public async Task<IActionResult> Summary(
            [FromBody] DashboardRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();


            try
            {
                DashboardResponse result =
                    await _data.SummaryAsync(request);


                return Success(result);

            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }
    }
}
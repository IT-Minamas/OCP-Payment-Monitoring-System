using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OCPSupplierController : BaseController
    {
        private readonly OCPSupplierData _data;
        public OCPSupplierController(
            IConfiguration configuration,
            OCPSupplierData data)
            : base(configuration)
        {
            _data = data;
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IActionResult> Search(
            [FromBody] OCPSupplierRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var result =
                    await _data.SearchAsync(
                        request.MillCode);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }
    }
}
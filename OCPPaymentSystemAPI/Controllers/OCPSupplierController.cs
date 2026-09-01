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

        [HttpGet]
        [Route("Bank")]
        public async Task<IActionResult> Bank(
            [FromQuery] string supplierCode,
            [FromQuery] string millCode)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                SupplierBankRequest request =
                    new SupplierBankRequest
                    {
                        SupplierCode = supplierCode,
                        MillCode = millCode
                    };

                var result =
                    await _data.GetBankAsync(request);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
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
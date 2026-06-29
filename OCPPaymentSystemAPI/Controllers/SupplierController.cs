using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : BaseController
    {
        private readonly SupplierData _supplierData;

        public SupplierController(
            IConfiguration configuration,
            SupplierData supplierData)
            : base(configuration)
        {
            _supplierData = supplierData;
        }

        [HttpPost]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll(
            [FromBody] SupplierRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var data =
                    await _supplierData.GetAllAsync(
                        request.MillCode);

                return Success(data);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }
    }
}
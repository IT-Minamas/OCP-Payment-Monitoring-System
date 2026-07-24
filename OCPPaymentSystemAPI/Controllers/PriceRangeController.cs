using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Controllers
{
    [Route("api/[controller]")]
    public class PriceRangeController : BaseController
    {
        private readonly PriceRangeData _priceRangeData;

        public PriceRangeController(
            IConfiguration configuration,
            PriceRangeData priceRangeData)
            : base(configuration)
        {
            _priceRangeData = priceRangeData;
        }

        [HttpGet]
        [Route("GetByID")]
        public async Task<IActionResult> GetByID([FromQuery] int id)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var result = await _priceRangeData.GetByIDAsync(id);

                if (result == null)
                    return Failed("Price Range not found.");

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
            [FromBody] PriceRangeSearchRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var result =
                    await _priceRangeData.SearchAsync(request);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(
            [FromBody] PriceRangeCreateRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                int id =
                    await _priceRangeData.CreateAsync(request);

                return Success(id);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> Update(
            [FromBody] PriceRangeUpdateRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _priceRangeData.UpdateAsync(request);

                if (!result)
                    return Failed("Price Range not found.");

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(
            [FromBody] PriceRangeDeleteRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _priceRangeData.DeleteAsync(request.ID);

                if (!result)
                    return Failed("Price Range not found.");

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpPost]
        [Route("UploadAttachment")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment(
            [FromForm] PriceRangeAttachmentRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _priceRangeData.UploadAttachmentAsync(request);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpGet]
        [Route("DownloadAttachment")]
        public async Task<IActionResult> DownloadAttachment(
            int id)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            var file =
                await _priceRangeData.DownloadAttachmentAsync(id);

            if (file == null)
                return NotFound();

            return File(
                file.FileData,
                file.ContentType,
                file.FileName);
        }

    }
}
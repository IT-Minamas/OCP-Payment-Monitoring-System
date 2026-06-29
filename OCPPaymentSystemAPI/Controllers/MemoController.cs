using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystemAPI.Controllers;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Models;
using OCPPaymentSystemAPI.Models.Workflow;

namespace OCPPaymentSystemAPI.Controllers
{
    [Route("api/[controller]")]
    public class MemoController : BaseController
    {
        private readonly MemoData _memoData;

        public MemoController(
            IConfiguration configuration,
            MemoData memoData)
            : base(configuration)
        {
            _memoData = memoData;
        }

        [HttpGet]
        [Route("GetByNo/{memoNo}")]
        public async Task<IActionResult> GetByNo(string memoNo)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var result =
                    await _memoData.GetByNoAsync(memoNo);

                if (result == null)
                    return Failed("Memo not found.");

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public async Task<IActionResult> Update(
            [FromBody] MemoUpdateRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _memoData.UpdateAsync(request);

                if (!result)
                    return Failed("Memo not found.");

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
            [FromBody] MemoCreateRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                string memoNo =
                    await _memoData.CreateAsync(request);

                return Success(memoNo);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete/{memoNo}")]
        public async Task<IActionResult> Delete(string memoNo)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _memoData.DeleteAsync(memoNo);

                if (!result)
                    return Failed("Memo not found.");

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
            [FromBody] MemoSearchRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var result =
                    await _memoData.SearchAsync(request);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpPost]
        [Route("Submit")]
        public async Task<IActionResult> Submit(
            [FromBody] MemoSubmitRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _memoData.SubmitAsync(request);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpPost]
        [Route("Approve")]
        public async Task<IActionResult> Approve(
            [FromBody] MemoApproveRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _memoData.ApproveAsync(request);

                // Nanti:
                // await _emailData.SendApproveEmailAsync(...);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpGet]
        [Route("CurrentApproval/{memoNo}")]
        public async Task<IActionResult> CurrentApproval(
            string memoNo)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var data =
                    await _memoData.GetCurrentApprovalAsync(memoNo);

                return Success(data);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpPost]
        [Route("Reject")]
        public async Task<IActionResult> Reject(
            [FromBody] MemoRejectRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _memoData.RejectAsync(request);

                // nanti:
                // await _emailData.SendRejectEmail();

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpPost]
        [Route("UploadAttachment")]
        public async Task<IActionResult> UploadAttachment(
            [FromForm] MemoUploadRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _memoData.UploadAttachmentAsync(request);

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
            string memoNo,
            string documentType)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var file =
                    await _memoData.DownloadAttachmentAsync(
                        memoNo,
                        documentType);

                return File(
                    file.FileData,
                    file.ContentType,
                    file.FileName);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteAttachment")]
        public async Task<IActionResult> DeleteAttachment(
            string memoNo,
            string documentType,
            string userName,
            string userIP)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _memoData.DeleteAttachmentAsync(
                        memoNo,
                        documentType,
                        userName,
                        userIP);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpGet]
        [Route("AttachmentList/{memoNo}")]
        public async Task<IActionResult>
        AttachmentList(string memoNo)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                var data =
                    await _memoData
                    .GetAttachmentListAsync(memoNo);

                return Success(data);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }
    }

}
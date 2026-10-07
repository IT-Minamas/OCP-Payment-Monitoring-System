using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
        private readonly EmailData _emailData;

        public MemoController(
            IConfiguration configuration,
            MemoData memoData,
            EmailData emailData)
            : base(configuration)
        {
            _memoData = memoData;
            _emailData = emailData;
        }

        [HttpGet]
        [Route("SDGWeigh/Detail")]

        public async Task<IActionResult>
        SDGWeighDetail([FromQuery] string memoNo)
        {

            if (!ValidateApiKey())
                return ApiKeyError();


            var result =
                await _memoData
                .GetMemoDetailAsync(
                    memoNo);


            return Success(result);

        }

        [HttpPost]
        [Route("SDGWeigh/Search")]
        public async Task<IActionResult>
        SearchSDGWeigh(
        SDGWeighRequest request)
        {

            if (!ValidateApiKey())
                return ApiKeyError();


            var result =
            await _memoData
            .GetSDGWeighAsync(request);


            return Success(result);

        }

        [HttpPost]
        [Route("SDGWeigh/Check")]
        public async Task<IActionResult>
        CheckSDGWeigh(
        SDGWeighCheckRequest request)
        {

            if (!ValidateApiKey())
                return ApiKeyError();


            var result =
            await _memoData
            .SaveSDGWeighAsync(request);


            return Success(result);

        }

        [HttpPost]
        [Route("UploadAttachment")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment(
            [FromForm] MemoAttachmentRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();


            var result =
                await _memoData.UploadAttachmentAsync(
                    request);


            return Success(result);
        }

        [HttpGet]
        [Route("GetByNo")]
        public async Task<IActionResult> GetByNo([FromQuery] string memoNo)
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

        [HttpPost]
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

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(
            [FromBody] MemoDeleteRequest request)
        {
            if (!ValidateApiKey())
                return ApiKeyError();

            try
            {
                bool result =
                    await _memoData.DeleteAsync(request.MemoNo);

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

                //Send Email to approver and requester
                await _emailData.SendApproverEmailAsync(request);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpGet]
        [Route("CurrentApproval")]
        public async Task<IActionResult> CurrentApproval(
            [FromQuery] string memoNo)
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

        [HttpGet]
        [Route("DownloadAttachment")]
        public async Task<IActionResult> DownloadAttachment(
            string memoNo,
            string type)
        {

            if (!ValidateApiKey())
                return ApiKeyError();


            var file =
                await _memoData.DownloadAttachmentAsync(
                    memoNo,
                    type);


            if (file == null)
                return NotFound();


            return File(
                file.FileData,
                file.ContentType,
                file.FileName);
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
                        documentType);

                return Success(result);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message);
            }
        }

        [HttpGet]
        [Route("AttachmentList")]
        public async Task<IActionResult>
            AttachmentList([FromQuery] string memoNo)
        {

            if (!ValidateApiKey())
                return ApiKeyError();


            var result =
                await _memoData
                .GetAttachmentListAsync(
                    memoNo);


            return Success(result);

        }
    }

}
using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystem.Web.Models;
using OCPPaymentSystem.Web.Services;
using System.Text.Json;
using System.Linq;
using OCPPaymentSystem.Web.Helpers;

namespace OCPPaymentSystem.Web.Controllers
{
    public class MemoController : Controller
    {
        private readonly ApiService _api;

        public MemoController(ApiService api)
        {
            _api = api;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> Company()
        {
            try
            {
                string? json =
                    HttpContext.Session.GetString("CurrentUser");

                LoginUser? currentUser = null;

                if (!string.IsNullOrEmpty(json))
                {
                    currentUser =
                        JsonSerializer.Deserialize<LoginUser>(json);
                }

                var result =
                    await _api.PostAsync<
                        object,
                        ApiResponse<List<CompanyModel>>>
                (
                    "Company/Search",
                    new { },
                    true,
                    "MINAMAS-2026"
                );

                if (currentUser != null)
                {
                    result.Data = result.Data
                        .Where(x => currentUser.CompanyAccess.Contains(x.Code))
                        .ToList();
                }

                return Json(result.Data);
            }
            catch (Exception ex)
            {
                return Json(new List<CompanyModel>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> Supplier()
        {
            string? json =
                HttpContext.Session.GetString("CurrentUser");

            if (string.IsNullOrEmpty(json))
            {
                return Json(new
                {
                    success = false,
                    sessionExpired = true,
                    message = "Session expired."
                });
            }

            LoginUser currentUser = JsonSerializer.Deserialize<LoginUser>(json)!;

            var result =
                await _api.PostAsync<
                    SupplierRequest,
                    ApiResponse<List<SupplierModel>>>
            (
                "Supplier/GetAll",
                new SupplierRequest { MillCode = currentUser.UnitCode  },
                true,
                "MINAMAS-2026"
            );
            return Json(result.Data);
        }

        [HttpGet]
        public async Task<JsonResult> MemoList()
        {
            string? json =
                HttpContext.Session.GetString("CurrentUser");

            if (string.IsNullOrEmpty(json))
            {
                return Json(new
                {
                    success = false,
                    sessionExpired = true,
                    message = "Session expired."
                });
            }

            LoginUser currentUser = JsonSerializer.Deserialize<LoginUser>(json)!;

            MemoListRequest request = new MemoListRequest
            {
                approvalLevel = currentUser.ApprovalLevel,
                CompanyAccess = currentUser.CompanyAccess,
                memoNo = "",
                supplierCode = "",
                millCode = currentUser.UnitCode,
                dateFrom = new DateTime(2000, 1, 1),
                dateTo = new DateTime(2100, 1, 1)
            };

            if (currentUser.ApprovalLevel >= 20)
            {
                request.millCode = null;
            }

            var result =
                await _api.PostAsync<
                    MemoListRequest,
                    ApiResponse<List<MemoListModel>>>
            (
                "Memo/Search",
                request,
                true,
                "MINAMAS-2026"
            );

            return Json(result.Data);
        }

        [HttpGet]
        public async Task<JsonResult> Detail(string memoNo)
        {
            var result =
                await _api.GetAsync<ApiResponse<MemoDetailModel>>(
                    "Memo/GetByNo/" + memoNo,
                    true,
                    "MINAMAS-2026");

            return Json(result.Data);
        }

        [HttpPost]
        public async Task<JsonResult> Delete(
            [FromBody] MemoDeleteRequest request)
        {
            try
            {
                LoginUser? currentUser =
                    HttpContext.Session.GetObject<LoginUser>("CurrentUser");

                if (currentUser == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session expired."
                    });
                }

                var apiRequest = new
                {
                    MemoNo = request.MemoNo
                };

                var result =
                    await _api.PostAsync<
                        object,
                        ApiResponse<bool>>
                (
                    "Memo/Delete/",
                    apiRequest,
                    true,
                    "MINAMAS-2026"
                );

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Submit(
            [FromBody] MemoSubmitRequest request)
        {
            try
            {
                LoginUser? currentUser = HttpContext.Session.GetObject<LoginUser>("CurrentUser");

                if (currentUser == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session expired."
                    });
                }

                request.UserName =
                    currentUser.fldUserId;

                request.UserIP =
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

                var result =
                    await _api.PostAsync<
                        MemoSubmitRequest,
                        ApiResponse<bool>>
                (
                    "Memo/Submit",
                    request,
                    true,
                    "MINAMAS-2026"
                );

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpPost]
        public async Task<JsonResult> Update(
            [FromBody] MemoSaveRequest request)
        {
            try
            {
                LoginUser? currentUser =
                    HttpContext.Session.GetObject<LoginUser>("CurrentUser");

                if (currentUser == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session expired."
                    });
                }

                request.userName =
                    currentUser.fldUserId;

                request.userIP =
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

                var result =
                    await _api.PostAsync<
                        MemoSaveRequest,
                        ApiResponse<bool>>
                (
                    "Memo/Update",
                    request,
                    true,
                    "MINAMAS-2026"
                );

                return Json(result);

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Save(
            [FromBody] MemoSaveRequest model)
        {
            try
            {
                string? json = HttpContext.Session.GetString("CurrentUser");

                LoginUser? currentUser = null;
                if (!string.IsNullOrEmpty(json))
                {
                    currentUser = JsonSerializer.Deserialize<LoginUser>(json);
                }
                model.millCode = currentUser.UnitCode ?? string.Empty;

                if (string.IsNullOrWhiteSpace(model.memoNo))
                {
                    var result =
                        await _api.PostAsync<
                            MemoSaveRequest,
                            ApiResponse<string>>
                        (
                            "Memo/Create",
                            model,
                            true,
                            "MINAMAS-2026"
                        );

                    return Json(result);
                }
                else
                {
                    var result =
                        await _api.PostAsync<
                            MemoSaveRequest,
                            ApiResponse<bool>>
                        (
                            "Memo/Update",
                            model,
                            true,
                            "MINAMAS-2026"
                        );

                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Approve(
            [FromBody] MemoApproveRequest request)
        {
            try
            {
                LoginUser? user =
                    HttpContext.Session.GetObject<LoginUser>("CurrentUser");

                if (user == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session expired."
                    });
                }

                request.UserName =
                    user.fldUserId;

                request.UserIP =
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

                var result =
                    await _api.PostAsync<
                        MemoApproveRequest,
                        ApiResponse<bool>>
                (
                    "Memo/Approve",
                    request,
                    true,
                    "MINAMAS-2026"
                );

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UploadAttachment()
        {
            try
            {
                var file = Request.Form.Files[0];

                bool result =
                    await _api.UploadFileAsync(
                        "Memo/UploadAttachment",
                        file,
                        new Dictionary<string, string>()
                        {
                    { "MemoNo", Request.Form["MemoNo"]! },
                    { "DocumentType", Request.Form["DocumentType"]! }
                        });

                return Json(new
                {
                    Success = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<JsonResult> AttachmentList(
            string memoNo)
        {
            var data =
                await _api.GetAsync<List<MemoAttachmentModel>>
                (
                    "Memo/AttachmentList/" + memoNo
                );

            return Json(data);
        }

        public async Task<IActionResult>
        DownloadAttachment(
            string memoNo,
            string documentType)
        {
            HttpResponseMessage response =
                await _api.DownloadAsync(

                    "Memo/DownloadAttachment?" +

                    "memoNo=" + memoNo +

                    "&documentType=" + documentType);

            byte[] bytes =
                await response.Content
                .ReadAsByteArrayAsync();

            string contentType =
                response.Content.Headers.ContentType!
                .MediaType!;

            string fileName =
                response.Content.Headers
                .ContentDisposition!
                .FileName!
                .Replace("\"", "");

            return File(
                bytes,
                contentType,
                fileName);
        }

        [HttpDelete]
        public async Task<JsonResult>
        DeleteAttachment(
            string memoNo,
            string documentType)
        {
            bool result =
                await _api.DeleteAsync(

                    "Memo/DeleteAttachment?" +

                    "memoNo=" + memoNo +

                    "&documentType=" + documentType +

                    "&userName=" +

                    HttpContext.Session.GetString("UserID") +

                    "&userIP=" +

                    HttpContext.Connection
                    .RemoteIpAddress);

            return Json(new
            {
                Success = result
            });
        }
    }
}
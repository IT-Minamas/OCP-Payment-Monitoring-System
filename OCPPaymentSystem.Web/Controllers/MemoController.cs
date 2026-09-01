using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystem.Web.Models;
using OCPPaymentSystem.Web.Services;
using System.Text.Json;
using System.Linq;
using OCPPaymentSystem.Web.Helpers;
using System.Net.Http.Headers;

namespace OCPPaymentSystem.Web.Controllers
{
    public class MemoController : Controller
    {
        private readonly ApiService _api;

        public MemoController(ApiService api)
        {
            _api = api;
        }

        public IActionResult Index(
            string memoNo = "")
        {
            LoginUser? user =
                HttpContext.Session
                .GetObject<LoginUser>("CurrentUser");


            if (user == null)
            {
                return RedirectToAction(
                    "Index",
                    "Login");
            }

            ViewBag.MemoNo = memoNo;
            return View();

        }

        [HttpGet]
        public async Task<JsonResult> Bank(string supplierCode, string millCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(supplierCode))
                {
                    return Json(new List<BankModel>());
                }

                var result =
                    await _api.GetAsync<
                        ApiResponse<List<BankModel>>
                    >(
                        "OCPSupplier/Bank?supplierCode=" + Uri.EscapeDataString(supplierCode) + "&millCode=" + Uri.EscapeDataString(millCode),
                        true,
                        "MINAMAS-2026"
                    );

                return Json(result.Data);
            }
            catch
            {
                return Json(new List<BankModel>());
            }
        }

        [HttpGet]
        public IActionResult SDGWeighChecking(
            string memoNo,
            string supplierCode,
            string millCode)
        {

            ViewBag.MemoNo =
                memoNo;


            ViewBag.SupplierCode =
                supplierCode;


            ViewBag.MillCode =
                millCode;


            return View();

        }


        [HttpGet]
        public async Task<JsonResult>
        SDGWeighDetail(
            string memoNo)
        {

            var result =
            await _api.GetAsync
            <
              ApiResponse<List<MemoSDGWeighDetailModel>>
            >
            (
                "Memo/SDGWeigh/Detail?memoNo=" + Uri.EscapeDataString(memoNo),
                true,
                "MINAMAS-2026"
            );


            return Json(result.Data);

        }

        [HttpPost]
        public async Task<JsonResult> SearchSDGWeigh(
            [FromBody] SDGWeighRequest request)
        {

            var result =
                await _api.PostAsync
                <
                    SDGWeighRequest,
                    ApiResponse<List<SDGWeighModel>>
                >
                (
                    "Memo/SDGWeigh/Search",
                    request,
                    true,
                    "MINAMAS-2026"
                );


            return Json(result.Data);

        }

        [HttpPost]
        public async Task<JsonResult>
        CheckSDGWeigh(
            [FromBody]
    MemoSDGWeighSaveRequest request)
        {


            var result =
                await _api.PostAsync
                <
                    MemoSDGWeighSaveRequest,
                    ApiResponse<bool>
                >
                (
                    "Memo/SDGWeigh/Check",
                    request,
                    true,
                    "MINAMAS-2026"
                );


            return Json(
                result!.Data
            );


        }
        [HttpPost]
        public async Task<JsonResult> UploadAttachment(
            IFormFile file,
            string memoNo,
            string type)
        {
            try
            {
                if (file == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "File kosong"
                    });
                }


                var form =
                    new MultipartFormDataContent();


                // TAMBAHKAN DULU FIELD TEXT

                form.Add(
                    new StringContent(memoNo),
                    "memoNo");


                form.Add(
                    new StringContent(type),
                    "type");


                // BARU FILE

                var fileContent =
                    new StreamContent(
                        file.OpenReadStream());


                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        file.ContentType);


                form.Add(
                    fileContent,
                    "file",
                    file.FileName);


                foreach (var item in form)
                {
                    var value =
                        await item.ReadAsStringAsync();

                    Console.WriteLine(value);
                }

                var result =
                    await _api.PostFileAsync<ApiResponse<bool>>
                    (
                        "Memo/UploadAttachment",
                        form,
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
                    message = ex.ToString()
                });
            }
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
                    "Memo/GetByNo?memoNo=" + Uri.EscapeDataString(memoNo),
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
        public async Task<JsonResult> Reject(
    [FromBody] MemoRejectRequest request)
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

                request.UserName =
                    currentUser.fldUserId;

                request.UserIP =
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

                var result =
                    await _api.PostAsync<
                        MemoRejectRequest,
                        ApiResponse<bool>>
                (
                    "Memo/Reject",
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
                model.userName = currentUser.fldUserId;
                model.userIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

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

        [HttpGet]
        public async Task<JsonResult> AttachmentList(
            string memoNo)
        {

            var result =
                await _api.GetAsync
                <
                    ApiResponse<List<MemoAttachmentModel>>
                >
                (
                    "Memo/AttachmentList?memoNo=" + Uri.EscapeDataString(memoNo),
                    true,
                    "MINAMAS-2026"
                );


            return Json(result.Data);

        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(
            string memoNo,
            string type)
        {
            var response =
                await _api.DownloadAsync(
                    "Memo/DownloadAttachment?memoNo="
                    + Uri.EscapeDataString(memoNo)
                    + "&type="
                    + type,
                    true,
                    "MINAMAS-2026");


            byte[] data =
                await response.Content
                    .ReadAsByteArrayAsync();


            string contentType =
                response.Content.Headers.ContentType?
                .ToString()
                ?? "application/octet-stream";


            string fileName =
                response.Content.Headers.ContentDisposition?
                .FileName?
                .Replace("\"", "")
                ?? "attachment";


            Response.Headers.Append(
                "Content-Disposition",
                $"inline; filename=\"{fileName}\"");


            return File(
                data,
                contentType);
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

                    "memoNo=" + Uri.EscapeDataString(memoNo) +

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

        [HttpPost]
        public async Task<JsonResult> Supplier(
            [FromBody] SupplierRequest request)
        {
            try
            {
                LoginUser? currentUser = HttpContext.Session.GetObject<LoginUser>("CurrentUser");


                if (currentUser == null)
                {
                    return Json(new List<SupplierModel>());
                }

                if (currentUser.UnitCode != "")
                {
                    request.CompanyCode = "";
                    request.MillCode = currentUser.UnitCode;
                }

                var result =
                    await _api.PostAsync
                    <
                        SupplierRequest,
                        ApiResponse<List<SupplierModel>>
                    >
                (
                    "Supplier/GetAll",
                    request,
                    true,
                    "MINAMAS-2026"
                );

                return Json(result.Data);
            }
            catch
            {
                return Json(new List<SupplierModel>());
            }
        }
    }
}
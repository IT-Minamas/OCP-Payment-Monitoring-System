using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystem.Web.Helpers;
using OCPPaymentSystem.Web.Models;
using OCPPaymentSystem.Web.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OCPPaymentSystem.Web.Controllers
{
    public class PriceRangeController : Controller
    {
        private readonly ApiService _api;

        public PriceRangeController(ApiService api)
        {
            _api = api;
        }

        public IActionResult Index(int id = 0)
        {
            ViewBag.ID = id;
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
            catch
            {
                return Json(new List<CompanyModel>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> Mill()
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
                    "Company/SearchMill",
                    new { },
                    true,
                    "MINAMAS-2026"
                );

                if (currentUser != null)
                {
                    result.Data = result.Data
                        .Where(x => currentUser.CompanyAccess.Contains(x.SapCode))
                        .ToList();
                    //result.Data = result.Data
                    //    .Where(x => x.SapCode.StartsWith(currentUser.UnitCode.Substring(0, 4)))
                    //    .ToList();
                }

                return Json(result.Data);
            }
            catch
            {
                return Json(new List<CompanyModel>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> PriceRangeList(
            [FromBody] PriceRangeSearchRequest request)
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

                request.CompanyCode =
                    string.Join(",",
                        currentUser.CompanyAccess);

                var result =
                    await _api.PostAsync<
                        PriceRangeSearchRequest,
                        ApiResponse<List<PriceRangeModel>>>
                    (
                        "PriceRange/Search",
                        request,
                        true,
                        "MINAMAS-2026"
                    );

                return Json(result.Data);
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
        public async Task<JsonResult> Detail(int id)
        {
            var result =
                await _api.GetAsync<
                    ApiResponse<PriceRangeModel>>
                (
                    "PriceRange/GetByID?id=" + id,
                    true,
                    "MINAMAS-2026"
                );

            return Json(result.Data);
        }

        [HttpPost]
        public async Task<JsonResult> Save1([FromBody] object request)
        {
            return Json(request);
        }

        [HttpPost]
        public async Task<JsonResult> Save(
            [FromBody] PriceRangeSaveRequest request)
        {
            try
            {
                string? json =
                    HttpContext.Session.GetString("CurrentUser");

                LoginUser? currentUser = null;

                if (!string.IsNullOrEmpty(json))
                    currentUser =
                        JsonSerializer.Deserialize<LoginUser>(json);

                if (currentUser == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Session expired."
                    });
                }

                request.UserName =
                    currentUser.fldUserId;

                request.UserIP =
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

                var result =
                    await _api.PostAsync<
                        PriceRangeSaveRequest,
                        ApiResponse<int>>
                    (
                        "PriceRange/Create",
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
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Update(
            [FromBody] PriceRangeSaveRequest request)
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
                        PriceRangeSaveRequest,
                        ApiResponse<bool>>
                    (
                        "PriceRange/Update",
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
        public async Task<JsonResult> Delete(
            [FromBody] PriceRangeDeleteRequest request)
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

                var result =
                    await _api.PostAsync<
                        PriceRangeDeleteRequest,
                        ApiResponse<bool>>
                    (
                        "PriceRange/Delete",
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
        public async Task<JsonResult> UploadAttachment(
    IFormFile file,
    int id)
        {
            try
            {
                if (file == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "File kosong."
                    });
                }

                var form =
                    new MultipartFormDataContent();

                form.Add(
                    new StringContent(id.ToString()),
                    "id");

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

                var result =
                    await _api.PostFileAsync<ApiResponse<bool>>
                    (
                        "PriceRange/UploadAttachment",
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
        public async Task<IActionResult> DownloadAttachment(
            int id)
        {
            var response =
                await _api.DownloadAsync(
                    "PriceRange/DownloadAttachment?id=" + id,
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
    }
}
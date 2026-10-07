using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystem.Web.Helpers;
using OCPPaymentSystem.Web.Models;
using OCPPaymentSystem.Web.Services;

namespace OCPPaymentSystem.Web.Controllers
{
    public class MemoSearchController : Controller
    {
        private readonly ApiService _api;


        public MemoSearchController(
            ApiService api)
        {
            _api = api;
        }



        public IActionResult Index()
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


            return View();
        }



        [HttpGet]
        public async Task<JsonResult> Approver()
        {

            LoginUser? user =
                HttpContext.Session
                .GetObject<LoginUser>("CurrentUser");


            var result =
                await _api.PostAsync<
                    object,
                    ApiResponse<List<ApproverModel>>>
                (
                    "Approver/Search",
                    new { },
                    true,
                    "MINAMAS-2026"
                );


            return Json(result.Data);

        }

        [HttpGet]
        public async Task<JsonResult> Company()
        {

            LoginUser? user =
                HttpContext.Session
                .GetObject<LoginUser>("CurrentUser");


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


            result.Data =
                result.Data
                .Where(x =>
                    user.CompanyAccess.Contains(x.Code))
                .ToList();


            return Json(result.Data);

        }


        [HttpPost]
        public async Task<JsonResult> Search(
            [FromBody] MemoSearchRequest request)
        {
            LoginUser? user =
                HttpContext.Session
                .GetObject<LoginUser>("CurrentUser");


            var result =
                await _api.PostAsync<
                    MemoSearchRequest,
                    ApiResponse<List<MemoListModel>>>
            (
                "Memo/Search",
                request,
                true,
                "MINAMAS-2026"
            );


            return Json(result.Data);
        }

    }
}
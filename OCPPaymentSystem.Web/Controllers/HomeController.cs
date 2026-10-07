using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystem.Web.Helpers;
using OCPPaymentSystem.Web.Models;
using OCPPaymentSystem.Web.Services;

namespace OCPPaymentSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api)
        {
            _api = api;
        }


        public async Task<IActionResult> Index()
        {
            LoginUser? currentUser =
                HttpContext.Session.GetObject<LoginUser>("CurrentUser");


            if (currentUser == null)
            {
                return RedirectToAction(
                    "Index",
                    "Login");
            }


            var result =
                await _api.PostAsync<
                    object,
                    ApiResponse<DashboardModel>>
            (
                "Dashboard/Summary",
                new
                {
                    CompanyAccess =
                        currentUser.CompanyAccess
                },
                true,
                "MINAMAS-2026"
            );

            if (currentUser.ApprovalLevel >= 40)        //above CFO level, only show approval for their level
            {
                result.Data.Approval = result.Data.Approval
                    .Where(x => x.ApprovalLevel == currentUser.ApprovalLevel)
                    .ToList();
            }

            return View(result.Data);
        }
    }
}
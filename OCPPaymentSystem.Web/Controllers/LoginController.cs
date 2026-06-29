using Microsoft.AspNetCore.Mvc;
using OCPPaymentSystem.Web.Models;
using OCPPaymentSystem.Web.Services;
using System.Text.Json;

namespace OCPPaymentSystem.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApiService _api;

        public LoginController(ApiService api)
        {
            _api = api;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]

        [HttpPost]
        public async Task<IActionResult> Index(LoginRequest model)
        {
            try
            {
                var result = await _api.PostAsync<LoginRequest, LoginResponse>("login", model, false);
                if (result == null)
                {
                    ViewBag.Error = "Cannot connect to API.";
                    return View(model);
                }

                if (result.fldType != "SUCCESS")
                {
                    ViewBag.Error = result.fldMessage;
                    return View(model);
                }

                LoginUser sessionUser = new()
                {
                    fldUserId = result.user.fldUserId,
                    fldName = result.user.fldName,
                    fldApiKey = result.user.fldApiKey,
                    UnitCode = result.user.UnitCode,
                    CompanyCode = result.user.CompanyCode,
                    CompanyName = result.user.CompanyName,
                    Role = result.user.Role,
                    ApprovalLevel = result.user.ApprovalLevel,
                    BusinessTitle = result.user.BusinessTitle,
                    ApprovalLevelName = result.user.ApprovalLevelName,
                    CompanyAccess = result.user.CompanyAccess,
                    AreaName = result.user.AreaName,
                    RegionName = result.user.RegionName
                };

                HttpContext.Session.SetString(
                    "CurrentUser",
                    JsonSerializer.Serialize(sessionUser));

                return RedirectToAction(
                    "Index",
                    "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Login");
        }
    }
}
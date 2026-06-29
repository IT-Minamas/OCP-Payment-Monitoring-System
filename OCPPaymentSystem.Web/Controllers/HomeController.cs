using Microsoft.AspNetCore.Mvc;

namespace OCPPaymentSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction(
                "Index",
                "Memo");
        }
    }
}
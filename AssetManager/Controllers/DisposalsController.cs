using Microsoft.AspNetCore.Mvc;

namespace AssetManager.Controllers
{
    public class DisposalsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

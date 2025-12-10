using Microsoft.AspNetCore.Mvc;

namespace NongDanService.Controllers
{
    public class LoNongSanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

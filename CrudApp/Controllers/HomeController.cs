using System.Diagnostics;
using CrudApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public IActionResult AuthorizedPage()
        {
            return View();
        }

        [Authorize(Roles ="Admin")]
        public IActionResult AdminPage()
        {
            return View();
        }

        [Authorize(Roles ="Accountant")]
        public IActionResult AccountantPage()
        {
            return View();
        }

        [Authorize(Roles ="Cashier")]
        public IActionResult CashierPage()
        {
            return View();
        }

    }

}

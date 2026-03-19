using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Payroll_Management_Solutions.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Payroll_Management_Solutions.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static int _visitorCount = 0;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //public IActionResult Index()
        //{
        //    int visitorCount = HttpContext.Session.GetInt32("VisitorCount") ?? 0;

        //    if (HttpContext.Session.GetString("Visited") == null)
        //    {
        //        visitorCount++;
        //        HttpContext.Session.SetInt32("VisitorCount", visitorCount);
        //        HttpContext.Session.SetString("Visited", "true");
        //    }

        //    return View();
        //}

        public IActionResult Index()
        {
            int count = HttpContext.Session.GetInt32("VisitorCount") ?? 0;

            if (HttpContext.Session.GetString("Visited") == null)
            {
                count++;

                HttpContext.Session.SetInt32("VisitorCount", count);
                HttpContext.Session.SetString("Visited", "true");
            }

            ViewBag.VisitorCount = count;

            return View();
        }
        public IActionResult About() { return RedirectToAction("Index", new { section = "about" }); }
        public IActionResult Features() { return RedirectToAction("Index", new { section = "Features" }); }
        public IActionResult Terms() { return RedirectToAction("Index", new { section = "Terms" }); }
        public IActionResult Contact() { return RedirectToAction("Index", new { section = "Contact" }); }
        public IActionResult SiteMap() { return RedirectToAction("Index", new { section = "SiteMap" }); }
        public IActionResult Privacy()
        {
            { return RedirectToAction("Index", new { section = "Privacy" }); }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Maintainance()
        {
            return View();
        }

    }
}

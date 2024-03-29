using System.Diagnostics;
using System.Text;
using ImageProcessor.Domain;
using ImageProcessor.Domain.Models;
using ImageProcessor.Models;
using ImageProcessor.Presentation.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Presentation.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

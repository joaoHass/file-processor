using ImageProcessor.Presentation.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Presentation.Controllers;

public class FileController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult UploadFiles(FilesUploadDto uploadedDto)
    {
        // TODO: Implement the logic
        return Ok();
    }
}
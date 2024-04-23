using System.Net.Mime;
using ImageProcessor.Domain;
using ImageProcessor.Domain.Models;
using ImageProcessor.Presentation.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Presentation.Controllers;

public class FileController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadFiles(FilesUploadDto dto)
    {
        var files = new Dictionary<MemoryStream, string>();

        foreach (var file in dto.Files)
        {
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            files.Add(memoryStream, file.FileName);
        }

        var fileProcessor = new FileProcessor(files, dto.TargetFileType, dto.Compress, dto.Resize);

        var processedFiles = new List<ProcessedFile>();
        try
        {
            processedFiles.AddRange(await fileProcessor.ProcessAsync());
        }
        catch (Exception e)
        {
            //_logger.LogCritical(e.ToString());
            return new StatusCodeResult(500);
        }
        
        return File(await fileProcessor.ReturnProcessedFileAsZip(),
            MediaTypeNames.Application.Zip,
            "compressed-files.zip");
    }
}
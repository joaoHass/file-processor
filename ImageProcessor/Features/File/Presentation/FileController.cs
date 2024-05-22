using System.Net;
using System.Net.Mime;
using ImageProcessor.Features.File.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Features.File.Presentation;

public class FileController : Controller
{
    private readonly FileProcessorFactory _fileProcessorFactory;
    private readonly ILogger<FileController> _logger;

    public FileController(FileProcessorFactory fileProcessorFactory, ILogger<FileController> logger)
    {
        _fileProcessorFactory = fileProcessorFactory;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadFiles(FilesUploadDto dto)
    {
        if (dto.Files == null)
            return BadRequest("The files properties can't be null");

        if (dto.Files.Length > 10)
            return StatusCode(
                (int)HttpStatusCode.RequestEntityTooLarge,
                "The payload exceeds the limit of 10 files"
            );

        var files = new Dictionary<MemoryStream, string>();
        foreach (var file in dto.Files)
        {
            if (file.Length > 5242880) // 5MB in binary
                return StatusCode(
                    (int)HttpStatusCode.RequestEntityTooLarge,
                    "At least one of the files exceeds 5MB"
                );

            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            files.Add(memoryStream, file.FileName);
        }

        var fileProcessor = _fileProcessorFactory.Create(
            files,
            dto.TargetFileType,
            dto.Compress,
            dto.Resize
        );

        var processedFiles = new List<ProcessedFile>();
        try
        {
            processedFiles.AddRange(await fileProcessor.ProcessAsync());
        }
        catch (Exception e)
        {
            _logger.LogError(message: e.ToString(), e);
            return new StatusCodeResult(500);
        }

        return File(
            await fileProcessor.ReturnProcessedFileAsZip(),
            MediaTypeNames.Application.Zip,
            "compressed-files.zip"
        );
    }
}

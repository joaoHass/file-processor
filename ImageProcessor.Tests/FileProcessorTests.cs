using System.Net;
using ImageProcessor.Data;
using ImageProcessor.Domain;
using ImageProcessor.Domain.FileStorageStrategy;
using ImageProcessor.Domain.Models;
using ImageProcessor.Presentation.Controllers;
using ImageProcessor.Presentation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ImageProcessor.Tests;

public class FileProcessorTests
{
    private readonly string _filesPath;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageStrategy _fileStorage;
    private readonly ILogger<FileController> _logger;

    public FileProcessorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _filesPath = "C:\\Users\\Hass\\Documents\\file-processor\\ImageProcessor.Tests\\TestFiles";
        _context = new DbContextFactory().Create();
        _fileStorage = new LocalFileStorageStrategy();
        _logger = _testOutputHelper.BuildLoggerFor<FileController>();
    }

    [Fact]
    public async void Valid_files_should_be_processed_successfully()
    {
        var files = CreateValidFile();
        var processorFactory = new FileProcessorFactory(_context, _fileStorage);
        var processor = processorFactory.Create(files, FileType.Jpeg, true, true);

        var processedFiles = await processor.ProcessAsync();

        Assert.Equal(1, _context.ProcessedFile.Count());
        Assert.Equal(FileStatus.Success, _context.ProcessedFile.First().StatusId);
        Assert.Equal(FileStatus.Success, processedFiles.First().FileStatus);
    }

    [Fact]
    public async void Invalid_files_should_be_processed_as_failed()
    {
        var files = CreateInvalidFile();
        var processorFactory = new FileProcessorFactory(_context, _fileStorage);
        var processor = processorFactory.Create(files, FileType.Jpeg, true, true);

        var processedFiles = await processor.ProcessAsync();

        Assert.Equal(1, _context.ProcessedFile.Count());
        Assert.Equal(FileStatus.FailedUnknownFormat, _context.ProcessedFile.First().StatusId);
        Assert.Equal(FileStatus.FailedUnknownFormat, processedFiles.First().FileStatus);
    }

    [Fact]
    public async void Failed_files_should_return_null_for_the_resulting_stream()
    {
        var files = CreateInvalidFile();
        var processor = new FileProcessorFactory(_context, _fileStorage).Create(files, FileType.Jpeg, true, true);

        var processedFiles = await processor.ProcessAsync();

        Assert.Null(processedFiles.First().ConvertedFile);
    }

    [Fact]
    public async void UploadFiles_should_reject_requests_when_files_property_is_null()
    {
        var controller = new FileController(new FileProcessorFactory(_context, _fileStorage), _logger);

        var result = await controller.UploadFiles(new FilesUploadDto(null, FileType.Jpeg, true, true));
        var statusResult = result as ObjectResult;

        Assert.NotNull(statusResult);
        Assert.IsType<BadRequestObjectResult>(statusResult);
    }

    [Fact]
    public async void UploadFiles_should_reject_requests_that_contains_files_larger_than_5mb()
    {
        var controller = new FileController(new FileProcessorFactory(_context, _fileStorage), _logger);
        var ms = CreateValidFile().First().Key;
        ms.SetLength(5242880 + 1);
        IFormFile[] formFiles = [new FormFile(ms, 0, ms.Length, "valid_test_file.png", "valid_test_file.png")];

        var result = await controller.UploadFiles(new FilesUploadDto(formFiles, FileType.Jpeg, true, true));
        var statusResult = result as ObjectResult;

       Assert.NotNull(result);
       Assert.Equal((int)HttpStatusCode.RequestEntityTooLarge, statusResult?.StatusCode);
    }

    [Fact]
    public async void UploadFiles_should_reject_requests_with_more_than_10_files()
    {
        var controller = new FileController(new FileProcessorFactory(_context, _fileStorage), _logger);
        var ms = CreateValidFile().First().Key;
        var formFiles = new IFormFile[11];
        for (int i = 0; i < 11; i++)
        {
            formFiles[i] = new FormFile(ms, 0, ms.Length, "valid_test_file.png", "valid_test_file.png");
        }

        var result = await controller.UploadFiles(new FilesUploadDto(formFiles, FileType.Jpeg, true, true));
        var statusResult = result as ObjectResult;

        Assert.NotNull(statusResult);
        Assert.Equal((int)HttpStatusCode.RequestEntityTooLarge, statusResult?.StatusCode);
    }

    [Fact]
    public void Non_supported_target_files_type_should_not_process()
    {
        var file = CreateInvalidFile();
        Assert.ThrowsAny<Exception>( () => new FileProcessorFactory(_context, _fileStorage).Create(file, 0, true, true));
    }

    private Dictionary<MemoryStream, string> CreateInvalidFile()
    {
        return new Dictionary<MemoryStream, string> { { new MemoryStream("Test file"u8.ToArray()), "test file name" } };
    }
    
    private Dictionary<MemoryStream, string> CreateValidFile()
    {
        var ms = new MemoryStream();
        using (var fs = new FileStream(Path.Join(_filesPath, "valid_png_file.png"), FileMode.Open, FileAccess.Read))
        {
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, (int)fs.Length);
            ms.Write(bytes, 0, (int)fs.Length);
            ms.Position = 0;
        }
        
        return new Dictionary<MemoryStream, string> { {ms, "valid_test_file.png" } };
    }
}
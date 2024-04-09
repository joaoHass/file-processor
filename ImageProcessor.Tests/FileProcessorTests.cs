using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using ImageProcessor.Domain;
using ImageProcessor.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using Xunit.Abstractions;

namespace ImageProcessor.Tests;

public class FileProcessorTests(ITestOutputHelper testOutputHelper)
{
    private readonly string _filesPath = "/home/joaohass/RiderProjects/ImageProcessor/ImageProcessor.Tests/TestFiles/";
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public async void Invalid_files_should_be_processed_as_failed()
    {
        var processor = new FileProcessor(Mock.Of<ILogger<FileProcessor>>());
        processor.TargetFileType = "png";
        processor.Files = new Dictionary<Stream, string>();
        processor.Files.Add(new MemoryStream(Encoding.UTF8.GetBytes("Test file")), "test file name");
        
        processor.Process();
        
        Assert.Equal(ProcessedFileStatus.FailedUnknownFormat, processor.FilesStatus.Keys.First());
    }

    [Fact]
    public async void Pdf_files_should_not_compress_or_resize()
    {
        
    }
}
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using ImageProcessor.Models;
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
    public async void Invalid_files_should_not_be_processed()
    {
        var processor = new FileProcessor(Mock.Of<ILogger<FileProcessor>>());
        processor.Files = new Dictionary<Stream, string>();
        processor.Files.Add(new MemoryStream(Encoding.UTF8.GetBytes("Test file")), "test file name");
        var expectedResult = new Dictionary<ProcessedFileStatus, string>();
        expectedResult.Add(ProcessedFileStatus.Failed, "aa");
        
        processor.Process();
        
        Assert.Equal(expectedResult, processor.FilesStatus);
    }

    [Fact]
    public async void Pdf_files_should_not_compress_or_resize()
    {
        
    }
}
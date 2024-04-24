using ImageProcessor.Domain;
using ImageProcessor.Domain.Models;
using Xunit.Abstractions;

namespace ImageProcessor.Tests;

public class FileProcessorTests(ITestOutputHelper testOutputHelper)
{
    private readonly string _filesPath = "/home/joaohass/RiderProjects/ImageProcessor/ImageProcessor.Tests/TestFiles/";
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public async void Invalid_files_should_be_processed_as_failed()
    {
        var files = CreateInvalidFile();
        var processor = new FileProcessor(files, FileType.Jpeg, true, true);

        var processedFiles = await processor.ProcessAsync();
        
        Assert.Equal(FileStatus.FailedUnknownFormat, processedFiles.First().FileStatus);
    }

    [Fact]
    public async void Invalid_files_should_not_return_the_file_stream()
    {
        var files = CreateInvalidFile();
        var processor = new FileProcessor(files, FileType.Jpeg, true, true);

        var processedFiles = await processor.ProcessAsync();
        
        Assert.Null(processedFiles.First().ConvertedFile);
    }

    [Fact]
    public async void Non_supported_target_files_type_should_not_process()
    {
        var files = CreateInvalidFile();
        Assert.ThrowsAny<Exception>( () => new FileProcessor(files, 0, true, true));
    }

    private Dictionary<MemoryStream, string> CreateInvalidFile()
    {
        return new Dictionary<MemoryStream, string> { { new MemoryStream("Test file"u8.ToArray()), "test file name" } };
    }
}
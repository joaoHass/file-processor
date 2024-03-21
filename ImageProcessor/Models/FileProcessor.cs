using System.Diagnostics;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessor.Models;

// TODO:
// Path to where files will be uploaded
// A class to handle db things
// One or more file can be processed
// Files can be in one of these states after trying to be processed:
// - Success
// - Processing
// - Failed (and reason why failed)
public class FileProcessor
{
    private readonly ILogger<FileProcessor> _logger;
    private ImageEncoder Encoder { get;  set; }
    
    public string FolderDestination { get; set; }
    public IDictionary<Stream, string> Files { get; set; }
    public IDictionary<ProcessedFileStatus, string> FilesStatus { get; set; }
    public string TargetFileType { get; set; }
    public bool Compress { get; set; }
    public bool Resize { get; set; }

    public FileProcessor(ILogger<FileProcessor> logger)
    {
        _logger = logger;
        FolderDestination = "/usr/local/";
    }

    public async void Process()
    {
        #region error handling
        if (string.IsNullOrWhiteSpace(FolderDestination))
        {
            _logger.LogCritical("The target file PATH was not defined.");
            throw new ApplicationException("The target file PATH was not defined.");
        }

        if (string.IsNullOrWhiteSpace(TargetFileType))
        {
            _logger.LogWarning("The target file TYPE was not defined.");
            throw new ArgumentException("The target file TYPE was not defined.");
        }
        #endregion
        
        FilesStatus = new Dictionary<ProcessedFileStatus, string>();
        
        foreach (var file in Files)
        {
            var fileStream = file.Key;
            var newFileName = Guid.NewGuid().ToString();

            try
            {
                await SaveAsAsync(fileStream, newFileName);
            }
            #region error handling
            catch (NotSupportedException e)
            {
                _logger.LogWarning("Tried to process a file that is not supported. Exception: {0}", e.ToString());
                FilesStatus.Add(ProcessedFileStatus.FailedUnsupportedFormat, newFileName);
                continue;
            }
            catch (UnknownImageFormatException e)
            {
                _logger.LogWarning("Tried to process a file that contains unknown format. Exception: {0}", e);
                FilesStatus.Add(ProcessedFileStatus.FailedUnknownFormat, newFileName);
                continue;
            }
            catch (Exception e)
            {
                FilesStatus.Add(ProcessedFileStatus.Failed, newFileName);
                continue;
            }
            #endregion
            
            // TODO: Save to DB; if the saving fails, delete the file and save as failed to FilesStatus
            
            FilesStatus.Add(ProcessedFileStatus.Success, newFileName);
        }
    }

    /// <summary>
    /// Save the passed image as the TargetFileType, in the configured FolderDestination. 
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="fileName">The file name, without the extension</param>
    /// <exception cref="ArgumentException">Threw if the passed fileName is null or whitespace</exception>
    /// <exception cref="ArgumentNullException">The stream is null.</exception>
    /// <exception cref="NotSupportedException">The stream is not readable or the image format is not supported.</exception>
    /// <exception cref="InvalidImageContentException">The encoded image contains invalid content.</exception>
    /// <exception cref="UnknownImageFormatException">The encoded image format is unknown.</exception>
    private async Task SaveAsAsync(Stream fileStream, string fileName)
    {
        await Image.IdentifyAsync(fileStream);
        
        var filePath = Path.Join(FolderDestination, fileName);
        var originalImage = await Image.LoadAsync(fileStream);
        
       // TODO: What if the target file type does not exist? 
        switch (TargetFileType.ToLower())
        {
            case "png":
                await originalImage.SaveAsPngAsync($"{filePath}.png");
                break;
            case "jpeg":
                await originalImage.SaveAsJpegAsync($"{filePath}.jpeg");
                break;
            case "bmp":
                await originalImage.SaveAsBmpAsync($"{filePath}.bmp");
                break;
            case "webp":
                await originalImage.SaveAsWebpAsync($"{filePath}.webp");
                break;
        }
    }

    private void DefineEncoderType(string targetFileType)
    {
        if (string.IsNullOrWhiteSpace(targetFileType))
        {
            _logger.LogWarning("The target file TYPE is invalid.");
            throw new ArgumentNullException("The target file TYPE is null or empty.");
        }
        
        switch (targetFileType.ToLower())
        {
            case "png":
                Encoder = new PngEncoder();
                break;
            case "jpeg":
                Encoder = new JpegEncoder();
                break;
            case "bmp":
                Encoder = new BmpEncoder();
                break;
            case "webp":
                Encoder = new WebpEncoder();
                break;
            default:
                _logger.LogWarning("The target file TYPE is not recognized.");
                throw new ArgumentException("The target file TYPE is not recognized.");
        }
    }
    
}
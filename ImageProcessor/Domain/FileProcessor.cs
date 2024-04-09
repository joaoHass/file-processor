using ImageProcessor.Domain.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessor.Domain;

public class FileProcessor
{
    private readonly ILogger<FileProcessor> _logger;
    private ImageEncoder Encoder { get; set; }
    
    public string FolderDestination { get; set; }
    public IDictionary<Stream, string> Files { get; set; }
    public IDictionary<ProcessedFileStatus, string> FilesStatus { get; private set; }
    
    private string _targetFileType;
    /// <summary>
    /// The extension target type without the dot. Ex: "png", "jpeg", etc.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the targetFileType is null or whitespace
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the targetFileType is not recognized. Recognized types are: png, jpeg, bmp
    /// </exception>
    public string TargetFileType
    {
        get => _targetFileType;
        set
        {
            DefineEncoderType(value);
            _targetFileType = value;
        } 
    }
    public bool Compress { get; set; }
    public bool Resize { get; set; }

    public FileProcessor() 
    {
        FolderDestination = "/usr/local/";
    }

    public async Task Process()
    {
        if (string.IsNullOrWhiteSpace(FolderDestination))
            throw new ApplicationException("The target file PATH was not defined.");
        
        
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
                FilesStatus.Add(ProcessedFileStatus.FailedUnsupportedFormat, newFileName);
                continue;
            }
            catch (UnknownImageFormatException e)
            {
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
        
        await originalImage.SaveAsync(filePath, Encoder);
    }
    
    /// <summary>
    /// Defines which Encoder to use based on the targetFileType
    /// </summary>
    /// <param name="targetFileType">The extension target type whitout the dot. Ex: "png", "jpeg", etc.</param>
    /// <exception cref="ArgumentNullException">Thrown if the targetFileType is null or whitespace</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the targetFileType is not recognized. Recognized types are: png, jpeg, bmp
    /// </exception>
    private void DefineEncoderType(string targetFileType)
    {
        if (targetFileType == 0)
            throw new ArgumentNullException(nameof(targetFileType), "The target file TYPE is null or empty.");
        
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
                throw new ArgumentException("The target file TYPE is not recognized.");
        }
    }
    
}
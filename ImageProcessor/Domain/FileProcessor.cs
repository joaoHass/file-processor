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
    private ImageEncoder Encoder { get; set; }

    private readonly bool _resize;
    private readonly bool _compress;
    private readonly string _folderDestination;
    private readonly IDictionary<Stream, string> _files;
    public IDictionary<ProcessedFileStatus, string> FilesStatus { get; private set; }
    
    public FileProcessor(IDictionary<Stream, string> files,
        FileType targetFileType,
        bool compress = false,
        bool resize = false) 
    {
        _files = files;
        _resize = resize;
        _compress = compress;
        FilesStatus = new Dictionary<ProcessedFileStatus, string>();
        
        _folderDestination = "/usr/local/";
        DefineEncoderType(targetFileType);
    }

    public async Task Process()
    {
        if (string.IsNullOrWhiteSpace(_folderDestination))
            throw new ApplicationException("The target file PATH was not defined.");
        
        foreach (var file in _files)
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
        
        var filePath = Path.Join(_folderDestination, fileName);
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
    private void DefineEncoderType(FileType targetFileType)
    {
        if (targetFileType == 0)
            throw new ArgumentNullException(nameof(targetFileType), "The target file TYPE is null or empty.");
        
        switch (targetFileType)
        {
            case FileType.Png:
                Encoder = new PngEncoder();
                break;
            case FileType.Jpeg:
                Encoder = new JpegEncoder();
                break;
            case FileType.Bmp:
                Encoder = new BmpEncoder();
                break;
            case FileType.Webp:
                Encoder = new WebpEncoder();
                break;
            default:
                throw new ArgumentException("The target file TYPE is not recognized.");
        }
    }
    
}
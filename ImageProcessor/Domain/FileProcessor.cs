using ImageProcessor.Domain.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace ImageProcessor.Domain;

public class FileProcessor
{
    // Defined inside the constructor using DefineEncoderType, will never be null
    private ImageEncoder Encoder { get; set; } = null!;

    private readonly bool _resize;
    private readonly bool _compress;
    private readonly string _folderDestination;
    private readonly IDictionary<Stream, string> _files;
    public IList<ProcessedFile> FilesStatus { get; }

    public FileProcessor(IDictionary<Stream, string> files,
        FileType targetFileType,
        bool compress = false,
        bool resize = false)
    {
        _files = files;
        _resize = resize;
        _compress = compress;
        FilesStatus = new List<ProcessedFile>();

        _folderDestination = "/usr/local/";
        DefineEncoderType(targetFileType);
    }

    public async Task<IList<ProcessedFile>> ProcessAsync()
    {
        if (string.IsNullOrWhiteSpace(_folderDestination))
            throw new ApplicationException("The target file PATH was not defined.");

        foreach (var file in _files)
        {
            var fileStream = file.Key;
            var newFileName = Guid.NewGuid().ToString();
            
            var fileStatus = new ProcessedFile(
                originalName: file.Value,
                newName: newFileName,
                processedImage: null,
                fileStatus: FileStatus.Processing);
            
            try
            {
                await SaveAsAsync(fileStream, newFileName);
            }
            #region error handling
            catch (NotSupportedException)
            {
                fileStatus.FileStatus = FileStatus.FailedUnsupportedFormat;
                FilesStatus.Add(fileStatus);
                continue;
            }
            catch (UnknownImageFormatException)
            {
                fileStatus.FileStatus = FileStatus.FailedUnknownFormat;
                FilesStatus.Add(fileStatus);
                continue;
            }
            catch (Exception)
            {
                fileStatus.FileStatus = FileStatus.Failed;
                FilesStatus.Add(fileStatus);
                continue;
            }
            #endregion

            // TODO: Save to DB; if the saving fails, delete the file and save as failed to FilesStatus
            fileStatus.FileStatus = FileStatus.Success;
            fileStatus.ProcessedImage = fileStream;
            FilesStatus.Add(fileStatus);
        }

        return FilesStatus;
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

        Encoder = targetFileType switch
        {
            FileType.Png => new PngEncoder(),
            FileType.Jpeg => new JpegEncoder(),
            FileType.Bmp => new BmpEncoder(),
            FileType.Webp => new WebpEncoder(),
            _ => throw new ArgumentException("The target file TYPE is not recognized.")
        };
    }
}
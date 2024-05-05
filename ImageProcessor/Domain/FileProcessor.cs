using System.IO.Compression;
using ImageProcessor.Data;
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
    // Defined inside the constructor using DefineTargetFileType, will never be null
    private ImageEncoder _encoder = null!;
    private string _targetFileType = null!;
    //

    private readonly ApplicationDbContext _context;
    private readonly bool _resize;
    private readonly bool _compress;
    private readonly string _folderDestination;
    private readonly IDictionary<MemoryStream, string> _files;
    public IList<ProcessedFile> ProcessedFiles { get; }

    public FileProcessor(
            ApplicationDbContext context,
            IDictionary<MemoryStream, string> files,
            FileType targetFileType,
            bool compress = false,
            bool resize = false
        )
    {
        _context = context;
        _files = files;
        _resize = resize;
        _compress = compress;
        ProcessedFiles = new List<ProcessedFile>();

        _folderDestination = "/usr/local/";
        DefineTargetFileType(targetFileType);
    }

    public async Task<IList<ProcessedFile>> ProcessAsync()
    {
        if (!Path.Exists(_folderDestination))
            throw new ApplicationException("The target file PATH does not exist.");

        foreach (var (fileStream, fileName) in _files)
        {
            var newFileName = $"{Guid.NewGuid()}.{_targetFileType}";
            var filePath = Path.Join(_folderDestination, $"{newFileName}" );
            var currentFile = new ProcessedFile(fileName, newFileName, null, FileStatus.Processing);
            
            try
            {
                fileStream.Position = 0;
                await Image.IdentifyAsync(fileStream);
                fileStream.Position = 0;

                using var image = await Image.LoadAsync(fileStream);
                
                if (_resize)
                {
                    image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));
                }
                    
                await image.SaveAsync(filePath, _encoder);
                currentFile.FileStatus = FileStatus.Success;
            }
            catch (UnknownImageFormatException) { currentFile.FileStatus = FileStatus.FailedUnknownFormat; }
            catch (NotSupportedException) { currentFile.FileStatus = FileStatus.FailedUnsupportedFormat; }
            catch (Exception e) { currentFile.FileStatus = FileStatus.Failed; }

            // TODO: Save to DB; if the saving fails, delete the file and save as failed to ProcessedFiles
            _context.Add(new Data.Types.ProcessedFile()
            {
                OriginalFileName = fileName,
                FilePath = filePath,
                StatusId = currentFile.FileStatus
            });
            await _context.SaveChangesAsync();

            if (currentFile.FileStatus == FileStatus.Success)
                currentFile.ConvertedFile = fileStream;

            ProcessedFiles.Add(currentFile);
        }

        return ProcessedFiles;
    }

    public async Task<byte[]> ReturnProcessedFileAsZip()
    {
        byte[]? result = null;
        
        using (var zipArchiveMemoryStream = new MemoryStream())
        using (var zipArchive = new ZipArchive(zipArchiveMemoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in ProcessedFiles)
            {
                if (file.FileStatus != FileStatus.Success)
                    continue;
                
                var zipEntry = zipArchive.CreateEntry(file.NewName, CompressionLevel.Fastest);
                await using (BinaryWriter writer = new BinaryWriter(zipEntry.Open()))
                {
                    writer.Write(file.ConvertedFile.ToArray());
                    writer.Close();
                }
                
            }

            zipArchiveMemoryStream.Position = 0;
            result = zipArchiveMemoryStream.ToArray();
        } 
        
        return result;
    }

    /// <summary>
    /// Defines which Encoder to use based on the targetFileType
    /// </summary>
    /// <param name="targetFileType">The extension target type whitout the dot. Ex: "png", "jpeg", etc.</param>
    /// <exception cref="ArgumentNullException">Thrown if the targetFileType is null or whitespace</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the targetFileType is not recognized. Recognized types are: png, jpeg, bmp
    /// </exception>
    private void DefineTargetFileType(FileType targetFileType)
    {
        switch (targetFileType)
        {
            case (FileType.Png):
                _encoder = new PngEncoder();
                _targetFileType = "png";
                break;
            case (FileType.Jpeg):
                _encoder = new JpegEncoder() { Quality = _compress ? 60 : 75};
                _targetFileType = "jpeg";
                break;
            case (FileType.Bmp):
                _encoder = new BmpEncoder();
                _targetFileType = "bmp";
                break;
            case (FileType.Webp):
                _encoder = new WebpEncoder() { Quality = _compress ? 60 : 75 };
                _targetFileType = "webp";
                break;
            default:
                throw new ArgumentException("The target file TYPE is not recognized.");
        }
    }
}
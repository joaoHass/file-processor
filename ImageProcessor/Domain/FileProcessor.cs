using System.IO.Compression;
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
    private ImageEncoder Encoder { get; set; } = null!;
    private string _targetFileType { get; set; } = null!;
    //

    private readonly bool _resize;
    private readonly bool _compress;
    private readonly string _folderDestination;
    private readonly IDictionary<MemoryStream, string> _files;
    public IList<ProcessedFile> ProcessedFiles { get; }

    public FileProcessor(IDictionary<MemoryStream, string> files,
        FileType targetFileType,
        bool compress = false,
        bool resize = false)
    {
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
                    
                await image.SaveAsync(filePath, Encoder);
            }
            catch (UnknownImageFormatException) { currentFile.FileStatus = FileStatus.FailedUnknownFormat; }
            catch (NotSupportedException) { currentFile.FileStatus = FileStatus.FailedUnsupportedFormat; }
            catch (Exception e) { currentFile.FileStatus = FileStatus.Failed; }

            if (currentFile.FileStatus != FileStatus.Processing)
            {
                ProcessedFiles.Add(currentFile);
                continue;
            }

            // TODO: Save to DB; if the saving fails, delete the file and save as failed to ProcessedFiles
            currentFile.FileStatus = FileStatus.Success;
            currentFile.ConvertedFile = fileStream;
            ProcessedFiles.Add(currentFile);
        }

        return ProcessedFiles;
    }

    public async Task<byte[]> ReturnProcessedFileAsZip()
    {
        byte[] result = null;
        
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
                Encoder = new PngEncoder();
                _targetFileType = "png";
                break;
            case (FileType.Jpeg):
                Encoder = new JpegEncoder() { Quality = _compress ? 60 : 75};
                _targetFileType = "jpeg";
                break;
            case (FileType.Bmp):
                Encoder = new BmpEncoder();
                _targetFileType = "bmp";
                break;
            case (FileType.Webp):
                Encoder = new WebpEncoder() { Quality = _compress ? 60 : 75 };
                _targetFileType = "webp";
                break;
            default:
                throw new ArgumentException("The target file TYPE is not recognized.");
        }
    }
}
using ImageProcessor.Features.File.Domain.Enums;

namespace ImageProcessor.Features.File.Domain;
public class ProcessedFile
{
    public string OriginalName { get; private set; }
    public string NewName { get; private set; }
    public MemoryStream? ConvertedFile { get; set; }
    public FileStatus FileStatus { get; set; }

    public ProcessedFile(
        string originalName,
        string newName,
        MemoryStream? convertedImage,
        FileStatus fileStatus
    )
    {
        OriginalName = originalName;
        NewName = newName;
        ConvertedFile = convertedImage;
        FileStatus = fileStatus;
    }
}

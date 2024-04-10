namespace ImageProcessor.Domain.Models;

public class ProcessedFile
{
    public string OriginalName { get; private set; }
    public string NewName { get; private set; }
    public Stream? ConvertedFile { get; set; }
    public FileStatus FileStatus { get; set; }
    
    public ProcessedFile(string originalName, string newName, Stream? convertedImage, FileStatus fileStatus)
    {
        OriginalName = originalName;
        NewName = newName;
        ConvertedFile = convertedImage;
        FileStatus = fileStatus;
    }
}
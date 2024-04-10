namespace ImageProcessor.Domain.Models;

public class ProcessedFile
{
    public string OriginalName { get; private set; }
    public string NewName { get; private set; }
    public Stream? ProcessedImage { get; set; }
    public FileStatus FileStatus { get; set; }
    
    public ProcessedFile(string originalName, string newName, Stream? processedImage, FileStatus fileStatus)
    {
        OriginalName = originalName;
        NewName = newName;
        ProcessedImage = processedImage;
        FileStatus = fileStatus;
    }
}
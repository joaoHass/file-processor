namespace ImageProcessor.Features.File.Domain;

public interface IFileStorageStrategy
{
    public Task<string> SaveAsync(Stream fileStream, string fileName);
}

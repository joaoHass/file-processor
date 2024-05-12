namespace ImageProcessor.Domain.FileStorageStrategy;

public interface IFileStorageStrategy {
    
    public Task<string> SaveAsync(Stream fileStream, string fileName);
}
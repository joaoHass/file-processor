namespace ImageProcessor.Features.File.Domain;

public class LocalFileStorageStrategy : IFileStorageStrategy
{
    private readonly string _storagePath;

    public LocalFileStorageStrategy()
    {
        _storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "processed_images"
        );
        Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName)
    {
        if (fileStream == null)
            throw new ArgumentNullException();

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("fileName");

        fileStream.Position = 0;
        var filePath = Path.Combine(_storagePath, fileName);
        await using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(fs);

        return filePath;
    }
}

namespace ImageProcessor.Features.File.Domain;

public class AzureFileStorageStrategy : IFileStorageStrategy
{
    private readonly string _storagePath;

    public AzureFileStorageStrategy()
    {
        // https://learn.microsoft.com/en-us/azure/app-service/operating-system-functionality#types-of-file-access-granted-to-an-app
        // "site" is the folder where the project resides, it always exists.
        _storagePath = Path.Combine(Environment.ExpandEnvironmentVariables("%HOME%"), "site");

        _storagePath = Directory
            .CreateDirectory(Path.Combine(_storagePath, "processed_images"))
            .FullName;

        if (!Directory.Exists(_storagePath))
            throw new DirectoryNotFoundException(
                $"The storage path does not exist on Azure! HOME variable: {Environment.ExpandEnvironmentVariables("HOME")}"
            );
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

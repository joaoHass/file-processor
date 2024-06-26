using ImageProcessor.Data;
using ImageProcessor.Features.File.Domain;
using ImageProcessor.Features.File.Domain.Enums;

namespace ImageProcessor.Features.File.Domain;

public class FileProcessorFactory
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageStrategy _fileStorage;

    public FileProcessorFactory(ApplicationDbContext context, IFileStorageStrategy fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public FileProcessor Create(
        IDictionary<MemoryStream, string> files,
        FileType targetFileType,
        bool compress = false,
        bool resize = false
    )
    {
        return new FileProcessor(_context, _fileStorage, files, targetFileType, compress, resize);
    }
}

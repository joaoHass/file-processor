using ImageProcessor.Data;
using ImageProcessor.Domain.Models;

namespace ImageProcessor.Domain;

public class FileProcessorFactory {
    private readonly ApplicationDbContext _context;

    public FileProcessorFactory(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public FileProcessor Create(
        IDictionary<MemoryStream, string> files,
        FileType targetFileType,
        bool compress = false,
        bool resize = false
        )
    {
        return new FileProcessor(_context, files, targetFileType, compress, resize );
    }
}
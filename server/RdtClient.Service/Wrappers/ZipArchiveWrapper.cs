using System.IO.Abstractions;
using SharpCompress.Archives.Zip;

namespace RdtClient.Service.Wrappers;

public interface IZipArchiveWrapper
{
    public ZipArchive Open(IEnumerable<IFileInfo> fileInfos);
    public ZipArchive Open(Stream stream);
}

public class ZipArchiveWrapper : IZipArchiveWrapper
{
    public ZipArchive Open(IEnumerable<IFileInfo> fileInfos)
    {
        var realFileInfos = fileInfos.Select(f => new FileInfo(f.FullName));
        
        return ZipArchive.Open(realFileInfos);
    }

    public ZipArchive Open(Stream stream)
    {
        return ZipArchive.Open(stream);
    }
}
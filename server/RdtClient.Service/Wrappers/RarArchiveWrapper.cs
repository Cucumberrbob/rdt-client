using System.IO.Abstractions;
using SharpCompress.Archives.Rar;

namespace RdtClient.Service.Wrappers;

public interface IRarArchiveWrapper
{
    public RarArchive Open(IEnumerable<IFileInfo> fileInfos);
    public RarArchive Open(Stream stream);
}

public class RarArchiveWrapper : IRarArchiveWrapper
{
    public RarArchive Open(IEnumerable<IFileInfo> fileInfos)
    {
        var realFileInfos = fileInfos.Select(f => new FileInfo(f.FullName));
        
        return RarArchive.Open(realFileInfos);
    }

    public RarArchive Open(Stream stream)
    {
        return RarArchive.Open(stream);
    }
}

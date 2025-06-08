using System.IO.Abstractions;
using RdtClient.Data.Models.Data;
using RdtClient.Data.Models.Internal;
using System.Web;
using RdtClient.Service.Services;

namespace RdtClient.Service.Helpers;

public static class DownloadHelper
{
    public static String? GetDownloadPath(String downloadPath, Torrent torrent, Download download, IFileSystem? fileSystem = null, DbSettings? settings = null)
    {
        var fileUrl = download.Link;

        if (String.IsNullOrWhiteSpace(fileUrl) || torrent.RdName == null)
        {
            return null;
        }

        var effectiveSettings = settings ?? Settings.Get;
        var createSubfolder = effectiveSettings.DownloadClient.CreateSubfolderForSingleFile;
        var isSingleFile = torrent.Files.Count == 1;

        var directory = RemoveInvalidPathChars(torrent.RdName);
        var torrentPath = !createSubfolder && isSingleFile ? downloadPath : Path.Combine(downloadPath, directory);

        var fileName = GetFileName(download);

        if (fileName == null)
        {
            return null;
        }

        var matchingTorrentFiles = torrent.Files.Where(m => m.Path.EndsWith(fileName)).Where(m => !String.IsNullOrWhiteSpace(m.Path)).ToList();

        if (matchingTorrentFiles.Count > 0)
        {
            var matchingTorrentFile = matchingTorrentFiles[0];

            var subPath = Path.GetDirectoryName(matchingTorrentFile.Path);

            if (!String.IsNullOrWhiteSpace(subPath))
            {
                subPath = subPath.Trim('/', '\\');
                torrentPath = Path.Combine(torrentPath, subPath);
            }
        }

        fileSystem ??= new FileSystem();

        if (!fileSystem.Directory.Exists(torrentPath))
        {
            fileSystem.Directory.CreateDirectory(torrentPath);
        }

        var filePath = Path.Combine(torrentPath, fileName);

        return filePath;
    }

    public static String? GetDownloadPath(Torrent torrent, Download download, DbSettings? settings = null)
    {
        var fileUrl = download.Link;

        if (String.IsNullOrWhiteSpace(fileUrl) || torrent.RdName == null)
        {
            return null;
        }

        var uri = new Uri(fileUrl);
        
        var effectiveSettings = settings ?? Settings.Get;
        var createSubfolder = effectiveSettings.DownloadClient.CreateSubfolderForSingleFile;
        var isSingleFile = torrent.Files.Count == 1;
        
        var torrentPath = !createSubfolder && isSingleFile ? "" : RemoveInvalidPathChars(torrent.RdName);

        var fileName = download.FileName;

        if (String.IsNullOrWhiteSpace(fileName))
        {
            fileName = uri.Segments.Last();

            fileName = HttpUtility.UrlDecode(fileName);
        }

        var matchingTorrentFiles = torrent.Files.Where(m => m.Path.EndsWith(fileName)).Where(m => !String.IsNullOrWhiteSpace(m.Path)).ToList();

        if (matchingTorrentFiles.Count > 0)
        {
            var matchingTorrentFile = matchingTorrentFiles[0];

            var subPath = Path.GetDirectoryName(matchingTorrentFile.Path);

            if (!String.IsNullOrWhiteSpace(subPath))
            {
                subPath = subPath.Trim('/').Trim('\\');

                torrentPath = Path.Combine(torrentPath, subPath);
            }
        }

        var filePath = Path.Combine(torrentPath, fileName);

        return filePath;
    }

    public static String? GetFileName(Download download)
    {
        if (String.IsNullOrWhiteSpace(download.Link))
        {
            return null;
        }

        var fileName = download.FileName;

        if (String.IsNullOrWhiteSpace(fileName))
        {
            fileName = HttpUtility.UrlDecode(new Uri(download.Link).Segments.Last());
        }

        return FileHelper.RemoveInvalidFileNameChars(fileName);
    }

    public static String RemoveInvalidPathChars(String path)
    {
        return String.Concat(path.Split(Path.GetInvalidPathChars()));
    }
}

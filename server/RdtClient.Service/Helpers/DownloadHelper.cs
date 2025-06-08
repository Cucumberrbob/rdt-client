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
        var relativePath = GetDownloadPathInternal(torrent, download, settings ?? Settings.Get);
        if (relativePath == null)
        {
            return null;
        }

        fileSystem ??= new FileSystem();
        var absolutePath = fileSystem.Path.Combine(downloadPath, relativePath);
        var directory = fileSystem.Path.GetDirectoryName(absolutePath);

        if (!String.IsNullOrWhiteSpace(directory) && !fileSystem.Directory.Exists(directory))
        {
            fileSystem.Directory.CreateDirectory(directory);
        }
        
        return absolutePath;
    }

    public static String? GetDownloadPath(Torrent torrent, Download download, DbSettings? settings = null)
    {
        return GetDownloadPathInternal(torrent, download, settings ?? Settings.Get);
    }

    private static String? GetDownloadPathInternal(Torrent torrent, Download download, DbSettings settings)
    {
        var fileUrl = download.Link;

        if (String.IsNullOrWhiteSpace(fileUrl) || torrent.RdName == null)
        {
            return null;
        }

        var fileName = GetFileName(download);

        if (fileName == null)
        {
            return null;
        }

        var matchingTorrentFiles = torrent.Files
            .Where(m => m.Path.EndsWith(fileName))
            .Where(m => !String.IsNullOrWhiteSpace(m.Path))
            .ToList();

        var createSubfolder = settings.DownloadClient.CreateSubfolderForSingleFile;
        var isSingleFile = torrent.Files.Count == 1;

        var directory = RemoveInvalidPathChars(torrent.RdName);

        var torrentPath = !createSubfolder && isSingleFile ? "" : directory;

        if (matchingTorrentFiles.Count != 0)
        {
            var matchingTorrentFile = matchingTorrentFiles[0];
            var subPath = Path.GetDirectoryName(matchingTorrentFile.Path);

            if (!String.IsNullOrWhiteSpace(subPath))
            {
                subPath = subPath.Trim('/', '\\');
                torrentPath = Path.Combine(torrentPath, subPath);
            }
        }

        return Path.Combine(torrentPath, fileName);
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
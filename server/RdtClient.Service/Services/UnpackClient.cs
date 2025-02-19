using System.Diagnostics;
using System.IO.Abstractions;
using RdtClient.Data.Models.Data;
using RdtClient.Service.Helpers;
using RdtClient.Service.Wrappers;
using SharpCompress.Archives;

namespace RdtClient.Service.Services;

public class UnpackClient(Download download, String destinationPath, IFileSystem fileSystem, IZipArchiveWrapper zipArchiveWrapper, IRarArchiveWrapper rarArchiveWrapper)
{
    public Boolean Finished { get; private set; }
        
    public String? Error { get; private set; }
        
    public Int32 Progess { get; private set; }

    private readonly Torrent _torrent = download.Torrent ?? throw new($"Torrent is null");
    
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public void Start()
    {
        Progess = 0;

        try
        {
            var filePath = DownloadHelper.GetDownloadPath(destinationPath, _torrent, download) ?? throw new("Invalid download path");

            Task.Run(async delegate
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    await Unpack(filePath, _cancellationTokenSource.Token);
                }
            });
        }
        catch (Exception ex)
        {
            Error = $"An unexpected error occurred preparing download {download.Link} for torrent {_torrent.RdName}: {ex.Message}";
            Finished = true;
        }
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }

    private async Task Unpack(String filePath, CancellationToken cancellationToken)
    {
        try
        {
            if (!fileSystem.File.Exists(filePath))
            {
                return;
            }

            var extractPath = destinationPath;
            String? extractPathTemp = null;

            var archiveEntries = await GetArchiveFiles(filePath, fileSystem, zipArchiveWrapper, rarArchiveWrapper);

            if (!archiveEntries.Any(m => m.StartsWith(_torrent.RdName + @"\")) && !archiveEntries.Any(m => m.StartsWith(_torrent.RdName + "/")))
            {
                extractPath = Path.Combine(destinationPath, _torrent.RdName!);
            }

            if (archiveEntries.Any(m => m.Contains(".r00")))
            {
                extractPathTemp = Path.Combine(extractPath, Guid.NewGuid().ToString());
                
                if (!fileSystem.Directory.Exists(extractPathTemp))
                {
                    fileSystem.Directory.CreateDirectory(extractPathTemp);
                }
            }
            
            if (extractPathTemp != null)
            {
                Extract(filePath, extractPathTemp, cancellationToken);

                await FileHelper.Delete(filePath, fileSystem);

                var rarFiles = fileSystem.Directory.GetFiles(extractPathTemp, "*.r00", SearchOption.TopDirectoryOnly);

                foreach (var rarFile in rarFiles)
                {
                    var mainRarFile = Path.ChangeExtension(rarFile, ".rar");

                    if (fileSystem.File.Exists(mainRarFile))
                    {
                        Extract(mainRarFile, extractPath, cancellationToken);
                    }

                    await FileHelper.DeleteDirectory(extractPathTemp, fileSystem);
                }
            }
            else
            {
                Extract(filePath, extractPath, cancellationToken);

                await FileHelper.Delete(filePath, fileSystem);
            }
        }
        catch (Exception ex)
        {
            Error = $"An unexpected error occurred unpacking {download.Link} for torrent {_torrent.RdName}: {ex.Message}";
        }
        finally
        {
            Finished = true;
        }
    }

    private static async Task<IList<String>> GetArchiveFiles(String filePath, IFileSystem fileSystem, IZipArchiveWrapper zipArchiveWrapper, IRarArchiveWrapper rarArchiveWrapper)
    {
        await using Stream stream = fileSystem.File.OpenRead(filePath);

        var extension = Path.GetExtension(filePath);

        IArchive archive;
        if (extension == ".zip")
        {
            archive = zipArchiveWrapper.Open(stream);
        }
        else
        {
            archive = rarArchiveWrapper.Open(stream);
        }

        var entries = archive.Entries
                             .Where(entry => !entry.IsDirectory)
                             .Select(m => m.Key!)
                             .ToList();

        archive.Dispose();

        return entries;
    }

    private void Extract(String filePath, String extractPath, CancellationToken cancellationToken)
    {
        var parts = ArchiveFactory.GetFileParts(filePath);

        var fi = parts.Select(m => fileSystem.FileInfo.New(m));

        var extension = Path.GetExtension(filePath);

        IArchive archive;
        if (extension == ".zip")
        {
            archive = zipArchiveWrapper.Open(fi);
        }
        else
        {
            archive = rarArchiveWrapper.Open(fi);
        }

        archive.ExtractToDirectory(extractPath,
                                   d =>
                                   {
                                       Debug.WriteLine(d);
                                       Progess = (Int32) Math.Round(d);
                                   },
                                   cancellationToken: cancellationToken);
        
        archive.Dispose();

        GC.Collect();
    }
}
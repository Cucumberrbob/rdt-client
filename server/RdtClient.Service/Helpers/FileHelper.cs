using System.IO.Abstractions;
using System.Text;

namespace RdtClient.Service.Helpers;

public static class FileHelper
{
    public static async Task Delete(String path, IFileSystem fileSystem)
    {
        if (String.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!fileSystem.File.Exists(path))
        {
            return;
        }

        var retry = 0;

        while (true)
        {
            try
            {
                fileSystem.File.Delete(path);

                break;
            }
            catch
            {
                if (retry >= 3)
                {
                    throw;
                }

                retry++;

                await Task.Delay(1000 * retry);
            }
        }
    }

    public static async Task DeleteDirectory(String path, IFileSystem fileSystem)
    {
        if (String.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!fileSystem.Directory.Exists(path))
        {
            return;
        }

        var retry = 0;

        while (true)
        {
            try
            {
                fileSystem.Directory.Delete(path, true);

                break;
            }
            catch
            {
                if (retry >= 3)
                {
                    throw;
                }

                retry++;

                await Task.Delay(1000 * retry);
            }
        }
    }

    public static String RemoveInvalidFileNameChars(String filename)
    {
        return String.Concat(filename.Split(Path.GetInvalidFileNameChars()));
    }
    
    public static String GetDirectoryContents(String path, IFileSystem fileSystem)
    {
        var stringBuilder = new StringBuilder();
        GetDirectoryContents(path, stringBuilder, "", fileSystem);
        return stringBuilder.ToString();
    }

    private static void GetDirectoryContents(String path, StringBuilder stringBuilder, String indent, IFileSystem fileSystem)
    {
        fileSystem ??= new FileSystem();
        var directoryInfo = fileSystem.DirectoryInfo.New(path);

        var directories = directoryInfo.GetDirectories();
        foreach (var directory in directories)
        {
            stringBuilder.AppendLine($"{indent}{directory.Name}");
            GetDirectoryContents(directory.FullName, stringBuilder, indent + "  ", fileSystem);
        }

        var files = directoryInfo.GetFiles();
        foreach (var file in files)
        {
            stringBuilder.AppendLine($"{indent}{file.Name}");
        }
    }
}
using System.IO.Compression;

namespace LoveBundler;

using System.Diagnostics;
using SixLabors.ImageSharp;

public static class BundlerCompiler
{
    private static readonly Dictionary<string, ushort> ConsoleIconDimensions =
        new() { { "ctr", 48 }, { "hac", 256 }, { "cafe", 128 } };

    private static bool RunProcess(ProcessStartInfo info, string filename)
    {
        using Process process = new() { StartInfo = info };

        if (!process.Start())
        {
            Console.WriteLine($"Failed to start {info.FileName}");
            return false;
        }
        else
            process.WaitForExit();

        if (Path.Exists(filename)) return true;

        Console.WriteLine($"Failed to create {filename}");
        return false;
    }

    private static async Task<bool> Compile(string directory, string baseDirectory, string console, CompilerSettings settings, string iconPath)
    {
        var path = Path.Join(directory, settings.Title);
        var data = Resources.Data[console];

        (ProcessStartInfo info, string extension) = console switch
        {
            "ctr" => (settings.GetSMDHCommand(directory, iconPath), "smdh"),
            "hac" => (settings.GetNACPCommand(directory), "nacp"),
            "cafe" => (settings.GetRPLCommand(directory, data.Binary), "rpx"),
            _ => throw new NotImplementedException()
        };

        if (!RunProcess(info, $"{path}.{extension}")) return false;

        (info, extension) = console switch
        {
            "ctr" => (settings.Get3DSXCommand(directory, data.Binary, data.RomFS), "3dsx"),
            "hac" => (settings.GetNROCommand(directory, data.Binary, iconPath, data.RomFS), "nro"),
            "cafe" => (settings.GetWUHBCommand(directory, iconPath, data.RomFS), "wuhb"),
            _ => throw new NotImplementedException()
        };

        if (!RunProcess(info, $"{path}.{extension}")) return false;

        // Merge binaries with game assets
        var binaryPath = Path.Combine(directory, $"{settings.Title}.{extension}");
        var assetDirectory = Path.Combine(baseDirectory, settings.Source);
        CopyDirectory(assetDirectory, Path.Combine(directory, settings.Source));
        assetDirectory = Path.Combine(directory, settings.Source);

        using (var zip = new ZipArchive(File.Create(Path.Combine(directory, $"{settings.Title}-bundle.zip")),
                   ZipArchiveMode.Create))
        {
            await BundlerMediaConvertor.ConvertFilesInDir(assetDirectory, true);
            // Add game assets to the zip
            foreach (var file in Directory.GetFiles(assetDirectory, "*", SearchOption.AllDirectories))
            {
                var entryName = Path.GetRelativePath(assetDirectory, file);
                zip.CreateEntryFromFile(file, entryName);
            }
        }

        // Merge binary and game assets into a single file
        var binaryData = await File.ReadAllBytesAsync(binaryPath);
        var gameData = await File.ReadAllBytesAsync(Path.Combine(directory, $"{settings.Title}-bundle.zip"));

        var mergedData = new byte[binaryData.Length + gameData.Length];
        Buffer.BlockCopy(binaryData, 0, mergedData, 0, binaryData.Length);
        Buffer.BlockCopy(gameData, 0, mergedData, binaryData.Length, gameData.Length);

        var mergedFilePath = Path.Combine(directory, $"{settings.Title}.{extension}");
        await File.WriteAllBytesAsync(mergedFilePath, mergedData);

        return true;
    }

    private static bool ValidateIcon(string console, string extension)
    {
        Console.WriteLine($"Validating icon: {console} ({extension})");
        return console switch
        {
            "ctr" or "cafe" => extension.ToLower() == ".png",
            "hac" => extension.ToLower() == ".jpeg" || extension.ToLower() == ".jpg",
            _ => false
        };
    }

    private static void CheckIcon(FileInfo file, string target)
    {
        try
        {
            using var image = Image.Load(file.FullName);

            var dimensions = ConsoleIconDimensions[target];
            int[] imageDimensions = [image.Width, image.Height];

            if (imageDimensions.Any(dimension => dimension != dimensions))
                throw new Exception($"Invalid icon dimensions for {target}.");

            if (!ValidateIcon(target, file.Extension))
                throw new Exception($"Invalid icon type for {target}.");
            else
                Console.WriteLine($"Using custom icon for {target}");
        }
        catch (Exception exception)
        {
            var message = $"An error occurred while processing the icon for {target}.";
            if (exception is ImageFormatException)
                message = $"Invalid icon format for {target}.";

            throw exception;
        }
    }

    public static async Task BuildBundle(string baseDirectory)
    {
        var tomlPath = Path.Combine(baseDirectory, "lovebrew.toml");
        if (!Path.Exists(tomlPath)) throw new Exception("No lovebrew.toml found in the specified directory");
        var tomlContents = File.ReadAllText(tomlPath);
        var settings = new CompilerSettings(tomlContents);
        if (settings.Target.Length == 0) throw new Exception("No target specified in lovebrew.toml");

        foreach (var target in settings.Target)
        {
            if (!Resources.Data.TryGetValue(target, out var data))
            {
                Console.WriteLine($"Invalid target {target}");
                continue;
            }

            string outputDirecotry = Path.Combine(baseDirectory, target);
            Directory.CreateDirectory(outputDirecotry);
            Console.WriteLine($"Creating {target} for {target}");

            var iconPath = Path.GetFullPath(data.Icon);
            var isCustomIcon = settings.Icons.TryGetValue(target, out var customIcon);
            if (isCustomIcon)
            {
                customIcon = Path.Combine(baseDirectory, customIcon);
                CheckIcon(new FileInfo(customIcon), target);
                iconPath = customIcon;
            }


            if (!(await Compile(outputDirecotry, baseDirectory, target, settings, iconPath)))
                Console.WriteLine($"Failed to compile {target}");
            else
                Console.WriteLine($"Successfully compiled {target}");
        }
    }
    
    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
        foreach (var directory in Directory.GetDirectories(sourceDir))
            CopyDirectory(directory, Path.Combine(destDir, Path.GetFileName(directory)));
    }
}
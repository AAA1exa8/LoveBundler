namespace LoveBundler;

using System.Diagnostics;
using SixLabors.ImageSharp;

public class BundlerCompileCommand
{
    public BundlerCompileCommand(string directory)
    {
        this.directory = directory;
    }

    public void Execute()
    {
        Compile();
    }

    private string directory { get; set; }

    private readonly Dictionary<string, ushort> ConsoleIconDimensions =
        new() { { "ctr", 48 }, { "hac", 256 }, { "cafe", 128 } };

    private bool RunProcess(ProcessStartInfo info, string filename)
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

    private bool Compile(string directory, string console, CompilerSettings settings, string iconPath)
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

        return RunProcess(info, $"{path}.{extension}");
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

    private void CheckIcon(FileInfo file, string target)
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

    private void Compile()
    {
        var tomlPath = Path.Combine(this.directory, "lovebrew.toml");
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

            string gameDirectory = Path.Combine(this.directory, target);
            Directory.CreateDirectory(gameDirectory);
            Console.WriteLine($"Creating {target} for {target}");

            var iconPath = Path.GetFullPath(data.Icon);
            var isCustomIcon = settings.Icons.TryGetValue(target, out var customIcon);
            if (isCustomIcon)
            {
                customIcon = Path.Combine(this.directory, customIcon);
                CheckIcon(new FileInfo(customIcon), target);
                iconPath = Path.GetFullPath(customIcon);
            }

            if (!Compile(gameDirectory, target, settings, iconPath))
                Console.WriteLine($"Failed to compile {target}");
            else
                Console.WriteLine($"Successfully compiled {target}");
        }
    }
}
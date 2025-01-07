namespace LoveBundler;

using System.Diagnostics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

public static class BundlerMediaConvertor
{
    private static readonly List<string> ImageExtensions = new() { ".png", ".jpg", ".jpeg" };
    private static readonly List<string> FontExtensions = new() { ".ttf", ".otf" };
    private static bool IsFont(string filename) => FontExtensions.Contains(Path.GetExtension(filename).ToLower());
    private const int MaxImageSize = 1024;
    private const int MinImageSize = 3;

    private static bool IsValidTexture(string filename)
    {
        using var image = Image.Load(filename);

        if (image is null)
        {
            return false;
        }

        if (image.Width > MaxImageSize || image.Height > MaxImageSize)
        {
            var side = image.Width > image.Height ? "width" : "height";
            var size = Math.Max(image.Width, image.Height);

            Console.WriteLine($"Image '{filename}' {side} is too large ({size} pixels > 1024 pixels)");
            return false;
        }

        if (image.Width < MinImageSize || image.Height < MinImageSize)
        {
            var side = image.Width < image.Height ? "width" : "height";
            var size = Math.Min(image.Width, image.Height);

            Console.WriteLine($"Image '{filename}' {side} is too small ({size} pixels < 5 pixels)");
            return false;
        }

        return true;
    }

    private static bool IsValidFont(string filepath)
    {
        using var file = File.OpenRead(filepath);
        return FontDescription.LoadDescription(file) != null;
    }

    private static bool IsValidMediaFile(string filepath, bool isFont)
    {
        var filename = Path.GetFileName(filepath);

        try
        {
            if (isFont)
                return IsValidFont(filepath);

            return IsValidTexture(filepath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading '{filename}': {e.Message}");
        }

        return false;
    }

    private static string GetConvertedFilename(string source, bool isFont)
    {
        if (isFont)
            return Path.ChangeExtension(source, "bcfnt");

        return Path.ChangeExtension(source, "t3x");
    }

    private static ProcessStartInfo CreateConvertCommand(string source, bool isFont)
    {
        string destination = GetConvertedFilename(source, isFont);

        if (isFont)
            return new ProcessStartInfo("mkbcfnt", $"\"{source}\" -o \"{destination}\"");

        return new ProcessStartInfo("tex3ds", $"-f rgba8888 -z auto \"{source}\" -o \"{destination}\"");
    }

    private static async Task<bool> ConvertMediaFile(FileInfo file, bool isFont)
    {
        var directory = file.DirectoryName;

        if (!IsValidMediaFile(file.FullName, isFont))
            return false;

        var info = CreateConvertCommand(file.FullName, isFont);

        var convertedFilename = GetConvertedFilename(file.Name, isFont);
        var convertedPath = Path.Join(directory, convertedFilename);

        Console.WriteLine($"Converting {file.Name} to {convertedFilename}..");

        using var process = new Process();
        process.StartInfo = info;

        if (!process.Start())
        {
            Console.WriteLine($"Failed to start process {info.FileName}");
            return false;
        }

        await process.WaitForExitAsync();

        if (!Path.Exists(convertedPath))
        {
            Console.WriteLine($"Failed to convert {file.Name} to {convertedFilename}");
            return false;
        }

        Console.WriteLine($"Converted {file.Name} to {convertedFilename} successfully.");
        return true;
    }

    public static async Task ConvertMediaFiles(string[] files)
    {
        if (files.Length == 0)
        {
            Console.WriteLine("No files to convert.");
            return;
        }

        List<Task<bool>> tasks = new();
        foreach (var fileName in files)
        {
            var file = new FileInfo(fileName);
            if (!file.Exists)
            {
                Console.WriteLine($"File '{fileName}' not found.");
                continue;
            }

            if (file.Length == 0)
            {
                Console.WriteLine($"File '{fileName}' is empty.");
                continue;
            }

            if (!ImageExtensions.Contains(file.Extension.ToLower()) &&
                !FontExtensions.Contains(file.Extension.ToLower()))
            {
                Console.WriteLine($"File '{fileName}' is not a valid image or font file.");
                continue;
            }

            bool isFont = IsFont(file.Name);
            tasks.Add(ConvertMediaFile(file, isFont));
        }

        await Task.WhenAll(tasks);

        foreach (var task in tasks)
        {
            var result = await task;
            if (!result)
            {
                Console.WriteLine("Something went wrong.");
            }
        }
    }

    public static async Task ConvertFilesInDir(string dir, bool deleteOldFile)
    {
        List<string> files = new();
        foreach (var file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
        {
            if (ImageExtensions.Contains(Path.GetExtension(file).ToLower()) || FontExtensions.Contains(Path.GetExtension(file).ToLower()))
                files.Add(file);
            
        }
        await ConvertMediaFiles(files.ToArray());
        if (deleteOldFile)
        {
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
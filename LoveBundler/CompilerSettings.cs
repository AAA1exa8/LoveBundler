using Tomlyn.Model;

namespace LoveBundler;

using System.Diagnostics;
using Tomlyn;

public class CompilerSettings
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string[] Target { get; set; }
    public Dictionary<string, string> Icons { get; set; }

    public CompilerSettings(string toml)
    {
        var model = Toml.ToModel(toml);
        this.Title = (string)((TomlTable)model["metadata"])["title"];
        this.Author = (string)((TomlTable)model["metadata"])["author"];
        this.Description = (string)((TomlTable)model["metadata"])["description"];
        this.Version = (string)((TomlTable)model["metadata"])["version"];
        this.Target = ((TomlArray)((TomlTable)model["build"]!)["targets"]).Select(x => (string)x).ToArray();
        var tmpIcons = ((TomlTable)model["icons"]).ToDictionary();
        this.Icons = new Dictionary<string, string>();
        foreach (var (key, value) in tmpIcons)
        {
            this.Icons.Add(key, (string)value);
        }
    }

    /// <summary>
    /// Gets the SMDH command for 3DS compilation
    /// </summary>
    /// <param name="directory">Build directory</param>
    /// <param name="iconPath">Path to the icon</param>
    /// <returns>ProcessStartInfo</returns>
    public ProcessStartInfo GetSMDHCommand(string directory, string iconPath)
    {
        var output = Path.Join(directory, this.Title);

        var description = $"{this.Description} - {this.Version}";
        var arguments =
            $"--create \"{this.Title}\" \"{description}\" \"{this.Author}\" \"{iconPath}\" \"{output}.smdh\"";

        return new ProcessStartInfo { FileName = "smdhtool", Arguments = arguments };
    }

    /// <summary>
    /// Gets the 3DSX command for 3DS compilation
    /// </summary>
    /// <param name="directory">Build directory</param>
    /// <param name="binaryPath">Path to the ELF file</param>
    /// <param name="romfsPath">Path to the RomFS image</param>
    /// <returns>ProcessStartInfo</returns>
    public ProcessStartInfo Get3DSXCommand(string directory, string binaryPath, string romfsPath)
    {
        var output = Path.Join(directory, this.Title);
        var arguments = $"\"{binaryPath}\" \"{output}.3dsx\" --smdh=\"{output}.smdh\" --romfs=\"{romfsPath}\"";

        return new ProcessStartInfo { FileName = "3dsxtool", Arguments = arguments };
    }

    /// <summary>
    /// Gets the NACP command for Switch compilation
    /// </summary>
    /// <param name="directory">Build directory</param>
    /// <returns>ProcessStartInfo</returns>
    public ProcessStartInfo GetNACPCommand(string directory)
    {
        var output = Path.Join(directory, this.Title);
        var arguments = $"--create \"{this.Title}\" \"{this.Author}\" \"{this.Version}\" \"{output}.nacp\"";

        return new ProcessStartInfo { FileName = "nacptool", Arguments = arguments };
    }

    /// <summary>
    /// Gets the NRO command for Switch compilation
    /// </summary>
    /// <param name="directory">Build directory</param>
    /// <param name="binaryPath">Path to the ELF file</param>
    /// <param name="iconPath">Path to the icon</param>
    /// <param name="romfsPath">Path to the RomFS image</param>
    /// <returns>ProcessStartInfo</returns>
    public ProcessStartInfo GetNROCommand(string directory, string binaryPath, string iconPath, string romfsPath)
    {
        var output = Path.Join(directory, this.Title);
        var arguments =
            $"\"{binaryPath}\" \"{output}.nro\" --nacp=\"{output}.nacp\" --icon=\"{iconPath}\" --romfs=\"{romfsPath}\"";

        return new ProcessStartInfo { FileName = "elf2nro", Arguments = arguments };
    }

    /// <summary>
    /// Gets the RPX command for Wii U compilation
    /// </summary>
    /// <param name="directory">Build directory</param>
    /// <param name="binaryPath">Path to the ELF file</param>
    /// <returns>ProcessStartInfo</returns>
    public ProcessStartInfo GetRPLCommand(string directory, string binaryPath)
    {
        var output = Path.Join(directory, this.Title);
        var arguments = $"\"{binaryPath}\" \"{output}.rpx\"";

        return new ProcessStartInfo { FileName = "elf2rpl", Arguments = arguments };
    }

    /// <summary>
    /// Gets the WUHB command for Wii U compilation
    /// </summary>
    /// <param name="directory">Build directory</param>
    /// <param name="iconPath">Path to the icon</param>
    /// <param name="romfsPath">Path to the content directory</param>
    /// <returns>ProcessStartInfo</returns>
    public ProcessStartInfo GetWUHBCommand(string directory, string iconPath, string romfsPath)
    {
        var output = Path.Join(directory, this.Title);
        var arguments =
            $"\"{output}.rpx\" \"{output}.wuhb\" --content=\"{romfsPath}\" --name=\"{this.Title}\" --short-name=\"{this.Title}\" --author=\"{this.Author}\" --icon=\"{iconPath}\"";

        return new ProcessStartInfo { FileName = "wuhbtool", Arguments = arguments };
    }
}
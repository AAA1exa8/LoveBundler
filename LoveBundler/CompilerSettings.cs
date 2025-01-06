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

        if (!model.ContainsKey("metadata"))
            throw new Exception("Missing 'metadata' section in the .toml file.");

         var metadata = (TomlTable)model["metadata"];
         if (!metadata.ContainsKey("title"))
             throw new Exception("Missing 'title' in the 'metadata' section of the .toml file.");
         this.Title = (string)metadata["title"];

         if (!metadata.ContainsKey("author"))
             throw new Exception("Missing 'author' in the 'metadata' section of the .toml file.");
         this.Author = (string)metadata["author"];

         if (!metadata.ContainsKey("description"))
             throw new Exception("Missing 'description' in the 'metadata' section of the .toml file.");
         this.Description = (string)metadata["description"];

         if (!metadata.ContainsKey("version"))
             throw new Exception("Missing 'version' in the 'metadata' section of the .toml file.");
         this.Version = (string)metadata["version"];

         if (!model.ContainsKey("build"))
             throw new Exception("Missing 'build' section in the .toml file.");

         var build = (TomlTable)model["build"];
         if (!build.ContainsKey("targets"))
             throw new Exception("Missing 'targets' in the 'build' section of the .toml file.");
         this.Target = ((TomlArray)build["targets"]).Select(x => (string)x).ToArray();

         if (metadata.ContainsKey("icons"))
         {
             var tmpIcons = ((TomlTable)metadata["icons"]).ToDictionary();
             this.Icons = new Dictionary<string, string>();
             foreach (var (key, value) in tmpIcons)
             {
                 if (!string.IsNullOrEmpty(value as string))
                     this.Icons.Add(key, (string)value);
             }
         }
         else
         {
             this.Icons = new Dictionary<string, string>();
         }

         // Ensure no critical fields are left empty
         if (string.IsNullOrWhiteSpace(this.Title))
             throw new Exception("The 'title' field in 'metadata' cannot be empty.");
         if (string.IsNullOrWhiteSpace(this.Author))
             throw new Exception("The 'author' field in 'metadata' cannot be empty.");
         if (string.IsNullOrWhiteSpace(this.Description))
             throw new Exception("The 'description' field in 'metadata' cannot be empty.");
         if (string.IsNullOrWhiteSpace(this.Version))
             throw new Exception("The 'version' field in 'metadata' cannot be empty.");
         if (this.Target.Length == 0)
             throw new Exception("The 'targets' array in 'build' must contain at least one target.");
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

// See https://aka.ms/new-console-template for more information

using LoveBundler;
using System.CommandLine;

var rootCommand = new RootCommand("LoveBundler, CLI version of bundler for LovePotion");

var convertCommand = new Command("convert", "Convert media files to format usable on console");
var filesOption = new Argument<string[]?>("files", "Files to convert");
var dirOption = new Option<string?>("--dir", "Convert all media files in directory");

convertCommand.Add(filesOption);
convertCommand.Add(dirOption);
convertCommand.SetHandler(async (files, dir) =>
{
    if (Directory.Exists(dir))
        await BundlerMediaConvertor.ConvertFilesInDir(dir, false);
    if (files != null)
        await BundlerMediaConvertor.ConvertMediaFiles(files);
}, filesOption, dirOption);

var bundleCommand = new Command("bundle", "Bundle the game for the specified console");
var dirArgument = new Argument<string>("dir", "Directory to bundle");
bundleCommand.Add(dirArgument); 
bundleCommand.SetHandler(async (dir) =>
{
    await Resources.Download();
    await BundlerCompiler.BuildBundle(dir);
}, dirArgument);

rootCommand.Add(convertCommand);
rootCommand.Add(bundleCommand);

await rootCommand.InvokeAsync(args);
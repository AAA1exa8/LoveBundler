// See https://aka.ms/new-console-template for more information

using LoveBundler;
using System.CommandLine;

var rootCommand = new RootCommand("LoveBundler, CLI version of bundler for LovePotion");

var convertCommand = new Command("convert", "Convert media files to format usable on console");
var filesArgument = new Argument<string[]>("files", "Files to convert");
convertCommand.Add(filesArgument);
convertCommand.SetHandler(async (files) =>
{
    var command = new BundlerConvertorCommand(files);
    await command.Execute();
}, filesArgument);

var bundleCommand = new Command("bundle", "Bundle the game for the specified console");
var dirArgument = new Argument<string>("dir", "Directory to bundle");
bundleCommand.Add(dirArgument);
bundleCommand.SetHandler((dir) =>
{
    var command = new BundlerCompileCommand(dir);
    command.Execute();
}, dirArgument);

rootCommand.Add(convertCommand);
rootCommand.Add(bundleCommand);

await rootCommand.InvokeAsync(args);
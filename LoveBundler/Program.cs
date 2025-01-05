// See https://aka.ms/new-console-template for more information

using LoveBundler;
using System.Linq;
using System;

var commandSelector = args[0];
try
{
    await Resources.Download();
    switch (commandSelector)
    {
        case "convert":
        {
            var command = new BundlerConvertorCommand(args.Skip(1).ToArray());
            await command.Execute();
            break;
        }
        case "bundle":
        {
            var command = new BundlerCompileCommand(args[1]);
            command.Execute();
            break;
        }
        case "-h":
        case "--help":
            throw new ArgumentException("print help text");
        default:
            throw new ArgumentException("Invalid command");
    }
}
catch (ArgumentException e)
{
    // Print help text
    Console.WriteLine("Usage: lovebundler <command> [options]");
    Console.WriteLine("Commands:");
    Console.WriteLine("  convert <files>  Convert media files to the required format");
    Console.WriteLine("  bundle <dir>     Bundle the game for the specified console");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -h, --help       Show this help text");
    Console.WriteLine();
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
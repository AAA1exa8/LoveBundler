// See https://aka.ms/new-console-template for more information

using LoveBundler;
using System.Linq;
using System;

var commandSelector = args[0];
try
{
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
        default:
            throw new ArgumentException("Invalid command");
    }
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
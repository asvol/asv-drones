using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public partial class AppArgs : IAppArgs
{
    private static IAppArgs? _instance;
    private readonly HashSet<string> _tags;
    private readonly SortedDictionary<string, string> _args;
    private const string  ProtocolPrefix = "asv://";


    public static IAppArgs Instance => _instance ??= new AppArgs();

    private AppArgs()
    {
        _args = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        _tags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
    }

    public IReadOnlyDictionary<string, string> Args => _args;

    public IReadOnlySet<string> Tags => _tags;

    public bool TryParseFile(string argsFile)
    {
        try
        {
            return File.Exists(argsFile) && TryParse(File.ReadAllLines(argsFile, Encoding.UTF8));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public string this[string key, string defaultValue] => _args.GetValueOrDefault(key, defaultValue);

    [GeneratedRegex(@"^--([^=]+)=(.*)$", RegexOptions.Compiled)]
    private static partial Regex ArgsParserRegex();

    public bool TryParse(IEnumerable<string> args)
    {
        var keyValuePattern = ArgsParserRegex();
        try
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith(ProtocolPrefix)) TryParseLink(arg);
                var match = keyValuePattern.Match(arg);
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value;
                    _args[key] = value;
                }
                else
                {
                    _tags.Add(arg);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error to parse application args:" + e.Message);
            return false;
        }
    }

    public bool TryParseLink(string link)
    {
        var path = link.Substring(ProtocolPrefix.Length);
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        try
        {
            for (int i = 0; i < segments.Length; i++)
            {
                _args[$"arg{i}"] = segments[i];
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error parsing link:" + e.Message);
            return false;
        }
    }
}
﻿using NLog;
using OfficeTools.Core.Models;
using Python.Runtime;

namespace OfficeTools.Core.Python;

/// <summary>
///     Extracts command arguments from Python source file.
/// </summary>
public class ArgParser
{
    readonly private static Logger Logger = LogManager.GetCurrentClassLogger();
    readonly private string moduleName;
    readonly private IPyRunner pyRunner;
    readonly private string rootPath;

    public ArgParser(IPyRunner pyRunner, string rootPath, string moduleName)
    {
        this.pyRunner = pyRunner;
        this.rootPath = rootPath;
        this.moduleName = moduleName;
    }

    /// <summary>
    ///     Convert a PyObject to a value that can be used as a LaunchOption value.
    /// </summary>
    public static object PyObjectToOptionValue(PyObject obj)
    {
        var type = obj.GetPythonType().Name;
        return type switch
        {
            "bool" => obj.As<bool>(),
            "int" => obj.As<int>(),
            "str" => obj.As<string>(),
            _ => throw new ArgumentException($"Unknown option type {type}")
        };
    }

    /// <summary>
    ///     Convert a PyObject to a LaunchOptionType enum.
    /// </summary>
    public static LaunchOptionType? PyObjectToOptionType(PyObject typeObj)
    {
        var typeName = typeObj.GetAttr("__name__").As<string>();
        return typeName switch
        {
            "bool" => LaunchOptionType.Bool,
            "int" => LaunchOptionType.Int,
            "str" => LaunchOptionType.String,
            _ => null
        };
    }

    public async Task<List<LaunchOptionDefinition>> GetArgsAsync()
    {
        await pyRunner.Initialize();

        return await pyRunner.RunInThreadWithLock(() =>
            {
                using PyModule scope = Py.CreateScope();
                dynamic sys = scope.Import("sys");
                dynamic argparse = scope.Import("argparse");
                // Add root path to sys.path
                sys.path.insert(0, rootPath);
                // Import module
                PyObject argsModule = scope.Import(moduleName);
                var argsDict = argsModule.GetAttr("__dict__").As<PyDict>();
                // Find ArgumentParser object in module
                dynamic? argParser = null;
                var argParserType = argparse.ArgumentParser;
                foreach (PyObject obj in argsDict.Values())
                {
                    if (obj.IsInstance(argParserType))
                    {
                        argParser = obj;
                        break;
                    }
                }

                if (argParser == null)
                {
                    throw new ArgumentException($"Could not find ArgumentParser object in module '{moduleName}'");
                }

                // Loop through arguments
                var definitions = new List<LaunchOptionDefinition>();

                foreach (var action in argParser._actions)
                {
                    var name = (action.dest as PyObject)?.As<string>();
                    if (name == null)
                    {
                        throw new Exception("Argument option did not have a `dest` value");
                    }

                    var optionStrings = ((PyObject)action.option_strings).As<string[]>();
                    var dest = (action.dest as PyObject)?.As<string>();
                    // var nArgs = (action.nargs as PyObject)?.As<int>();
                    var isConst = (action.@const as PyObject)?.IsTrue() ?? false;
                    var isRequired = (action.required as PyObject)?.IsTrue() ?? false;
                    var type = action.type as PyObject;
                    // Bool types will have a type of None (null)
                    var optionType = type == null ? LaunchOptionType.Bool : PyObjectToOptionType(type);
                    if (optionType == null)
                    {
                        Logger.Warn("Skipping option {Dest} with type {Name}", dest, type);
                        continue;
                    }

                    // Parse default
                    var @default = action.@default as PyObject;
                    var defaultValue = @default != null ? PyObjectToOptionValue(@default) : null;

                    var help = (action.help as PyObject)?.As<string>();

                    definitions.Add(new LaunchOptionDefinition
                        {
                            Name = help ?? name,
                            Description = help,
                            Options = new List<string> { optionStrings[0] },
                            // ReSharper disable once ConstantNullCoalescingCondition
                            Type = optionType ?? LaunchOptionType.Bool,
                            DefaultValue = defaultValue,
                            MinSelectedOptions = isRequired ? 1 : 0
                        }
                    );
                }

                return definitions;
            }
        );
    }
}
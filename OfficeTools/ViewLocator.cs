using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace OfficeTools;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param == null) return null;
        var name = param.GetType().Name!.Replace("ViewModel", "");
        var type = Type.GetType("OfficeTools.Views." + name);
        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => true;
}
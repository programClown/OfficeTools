using OfficeTools.ViewModels;
using System;
using System.ComponentModel;
using System.Text;

namespace OfficeTools.Controls;

public class ExceptionDialogViewModel : ViewModelBase
{
    public Exception? Exception { get; set; }

    public string? Message => Exception?.Message;

    public string? ExceptionType => Exception?.GetType().Name ?? "";

    [Localizable(false)]
    public string? FormatAsMarkdown()
    {
        var msgBuilder = new StringBuilder();
        msgBuilder.AppendLine();

        if (Exception is not null)
        {
            msgBuilder.AppendLine("## Exception");
            msgBuilder.AppendLine($"```{ExceptionType}: {Message}```");

            if (Exception.InnerException is not null)
            {
                msgBuilder.AppendLine(
                    $"```{Exception.InnerException.GetType().Name}: {Exception.InnerException.Message}```"
                );
            }
        }
        else
        {
            msgBuilder.AppendLine("## Exception");
            msgBuilder.AppendLine("```(None)```");
        }

        if (Exception?.StackTrace is not null)
        {
            msgBuilder.AppendLine("### Stack Trace");
            msgBuilder.AppendLine($"```{Exception.StackTrace}```");
        }

        if (Exception?.InnerException is { StackTrace: not null } innerException)
        {
            msgBuilder.AppendLine($"```{innerException.StackTrace}```");
        }

        return msgBuilder.ToString();
    }
}
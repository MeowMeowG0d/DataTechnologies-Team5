using Spectre.Console;
using DataTechnologies.Validation;

namespace DataTechnologies.UI;

public static class ConsoleDate
{
    public static DateTime AskValidDate(string prompt, bool mustBeFuture = false)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>(prompt);

            if (DateValidator.TryParse(input, out var date, out var error, mustBeFuture))
                return date;

            AnsiConsole.MarkupLine($"[red]{error}[/]");
        }
    }
}

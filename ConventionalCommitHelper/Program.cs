using System;
using Spectre.Console;
using System.Diagnostics;

namespace ConventionalCommitHelper;

public static class Program
{
    private const int Width = 80;
    private static readonly Color Accent = new(187, 134, 252);

    private class CommitType(string type, string desc)
    {
        public string Type { get; set; } = type;
        public string Description { get; set; } = desc;
    }


    public static void Main(string[] args)
    {
        HeaderPanel();
        if (NextSection("Start commit creation process ?"))
        {
            AnsiConsole.WriteLine("[grey]Operation canceled.[/]");
            return;
        }
        
        string commitType = ShowTypePanel();
        string commitRange =  ShowRangePanel();
        string commitTitle = ShowTitlePanel();
        string commitDescription = ShowDescPanel();
        
        string commit = ShowSummaryPanel(commitType, commitRange, commitTitle, commitDescription);

        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            ArgumentList = { "commit", "-m", commit}
        };

        using var process = Process.Start(psi);
        process?.WaitForExit();
    }

    private static string ShowEditModePanel()
    {
        AnsiConsole.Clear();
        var editPanel = new Panel(
                new Rows(
                    new Markup("[bold #BB86FC]Entered a edit mode of the commit.[/]"),
                    new Markup("[grey]Navigate with ↑ / ↓ and confirm with Enter.[/]")
                )
            )
            .Header("[bold #BB86FC] EDIT MODE [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(foreground: Accent))
            .Padding(2, 0);
        AnsiConsole.Write(editPanel);

        var edit = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(11)
                .HighlightStyle(new Style(
                        foreground: Color.Black,
                        background: Accent,
                        decoration: Decoration.Bold
                    )
                )
                .AddChoices("Type", "Title", "Description")
        );
        return edit;
    }

    private static void HeaderPanel()
    {
        Rows content = new(
            new Markup(
                """
                Create consistent, readable Git commit messages using the [bold #BB86FC]Conventional Commits[/] specification.
                """),
            Align.Right(
                new Markup(
                    $"[grey italic]v0.1.0  •  {DateTime.Now:dd.MM.yyyy HH:mm}[/]")
            )
        );
        Panel headerPanel = new Panel(content)
            .Header("[bold #BB86FC] CONVENTIONAL COMMIT HELPER [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(foreground: Accent))
            .Padding(2, 1);
        headerPanel.Width = Width;
        AnsiConsole.Write(headerPanel);
    }

    private static bool NextSection(string prompt)
    {
        var shouldContinue = AnsiConsole.Confirm($"[bold #BB86FC]{prompt}[/]");
        return !shouldContinue;
    }

    private static string ShowTypePanel()
    {
        AnsiConsole.Clear();
        Panel selectionPanel = new Panel(
                new Rows(
                    new Markup("[bold #BB86FC]Choose a commit type[/]"),
                    new Markup("[grey]Navigate with ↑ / ↓ and confirm with Enter.[/]")
                )
            )
            .Header("[bold #BB86FC] COMMIT TYPE [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(foreground: Accent))
            .Padding(2, 1);
        selectionPanel.Width = Width;
        AnsiConsole.Write(selectionPanel);

        List<CommitType> commitTypes = new List<CommitType>([
            new CommitType("feat", "Introduce a new feature"),
            new CommitType("fix", "Fix a bug"),
            new CommitType("refactor", "Restructure code without changing behavior"),
            new CommitType("perf", "Improve performance"),
            new CommitType("style", "Formatting changes only, no logic changes"),
            new CommitType("docs", "Documentation changes"),
            new CommitType("test", "Add or update tests"),
            new CommitType("build", "Change build system or dependencies"),
            new CommitType("ci", "Change CI/CD configuration"),
            new CommitType("chore", "Maintenance tasks that do not modify application logic"),
            new CommitType("revert", "Revert a previous commit")
        ]);

        var commitType = AnsiConsole.Prompt(
            new SelectionPrompt<CommitType>()
                .PageSize(11)
                .HighlightStyle(
                    new Style(
                        foreground: Color.Black,
                        background: Accent,
                        decoration: Decoration.Bold
                    )
                )
                .AddChoices(commitTypes)
                .UseConverter(type => $"[bold]{type.Type,-10}[/] ➤ {type.Description}")
        );

        AnsiConsole.MarkupLine(
            $"\n[grey]Selected type:[/] [bold #BB86FC]{Markup.Escape(commitType.Type)}[/]"
        );
        
        return commitType.Type;
    }

    private static string ShowTitlePanel()
    {
        AnsiConsole.Clear();
        var titlePanel = new Panel(
                new Rows(
                    new Markup("[bold #BB86FC]Enter a short title of the change.[/]"),
                    new Markup("[grey]Use imperative mood, e.g. Add login validation.[/]")
                )
            )
            .Header("[bold #BB86FC] COMMIT TITLE [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(foreground: Accent))
            .Padding(2, 0);

        AnsiConsole.Write(titlePanel);
        var title = AnsiConsole.Ask<string>("[bold #BB86FC]Enter a commit title: [/]");
        return title;
    }
    
    private static string ShowSummaryPanel(string cTy, string cRa, string cTi,  string cDe)
    {
        AnsiConsole.Clear();
        string commit =
            $"""
             {cTy}({cRa}): {cTi}
             Desc: {(cDe == "" ? "-/-" : cDe)}
             """;
        
        var summaryPanel = new Panel(
                new Rows(
                    new Markup("[bold #BB86FC]Successfully created commit[/] \n"),
                    new Markup($"""
                                [bold #EFBF04]Commit message:[/] 
                                {commit}
                                """),
                    new Markup("\n[grey]Want you enter edit mode for following commit?.[/]")
                )
            )
            .Header("[bold #BB86FC] COMMIT SUMMARY [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(foreground: Accent))
            .Padding(2, 0);

        AnsiConsole.Write(summaryPanel);

        if (!NextSection("Enter edit mode ?"))
        {
            var edit = ShowEditModePanel();
            switch (edit)
            {
                case  "Type": cTy = ShowTypePanel(); break;
                case  "Title": cTi =  ShowTitlePanel(); break; 
                case  "Description": cDe = ShowDescPanel(); break;
            }

            ShowSummaryPanel(cTy, cRa, cTi, cDe);
        }
        return commit;
    }

    private static string ShowDescPanel()
    {
        AnsiConsole.Clear();
        var descPanel = new Panel(
                new Rows(
                    new Markup("[bold #BB86FC]Enter a short description of the change.[/]"),
                    new Markup("[grey]Use imperative mood, e.g. Add login validation.[/]")
                )
            )
            .Header("[bold #BB86FC] COMMIT DESCRIPTION [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(foreground: Accent))
            .Padding(2, 0);
        AnsiConsole.Write(descPanel);
        
        var description = AnsiConsole.Ask<string>("[bold #BB86FC]Description[/]");
        return description;
    }
    
    private static string ShowRangePanel()
    {
        AnsiConsole.Clear();
        var descPanel = new Panel(
                new Rows(
                    new Markup("[bold #BB86FC]Enter a range of the change.[/]"),
                    new Markup("[grey]Use short, descriptive sentence, e.g. 'PlayerMovement'.[/]")
                )
            )
            .Header("[bold #BB86FC] COMMIT RANGE [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(foreground: Accent))
            .Padding(2, 0);
        AnsiConsole.Write(descPanel);
        
        var range = AnsiConsole.Ask<string>("[bold #BB86FC]Range[/]");
        return range;
    }

}
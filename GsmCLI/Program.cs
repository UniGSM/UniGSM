using System.CommandLine;

var fileOption = new Option<FileInfo?>(
    name: "--file",
    description: "The file to read and display on the console.");

var rootCommand = new RootCommand("DayZ GSM CLI")
{
    new Command("server", "Server commands"),
    new Command("backup", "Backup commands"),
    new Command("settings", "Settings commands")
};

return await rootCommand.InvokeAsync(args);
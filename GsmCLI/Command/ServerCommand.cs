using System.CommandLine;

namespace GsmCLI.Command;

public class ServerCommand : System.CommandLine.Command
{
    public ServerCommand() : base("server", "Server commands")
    {
        AddCommand(new ServerListCommand());
    }

    internal class ServerListCommand : System.CommandLine.Command
    {
        private static readonly Argument<int> PageArgument =
            new("page", () => 1, "The page number to display");

        public ServerListCommand() : base("list", "List all servers")
        {
            AddArgument(PageArgument);

            this.SetHandler(ListServers, PageArgument);
        }

        private static void ListServers(int page)
        {
            throw new NotImplementedException();
        }
    }

    internal class ServerAddCommand : System.CommandLine.Command
    {
        private static readonly Argument<string> NameArgument =
            new("name", "The name of the server");

        private static readonly Argument<string> BindIpArgument =
            new("bind-ip", () => "0.0.0.0", "The IP address to bind to");

        private static readonly Argument<uint> GamePortArgument =
            new("game-port", () => 2302, "The game port to use");

        private static readonly Argument<uint> QueryPortArgument =
            new("query-port", () => 27016, "The query port to use");

        private static readonly Argument<uint> RconPortArgument =
            new("rcon-port", () => 2305, "The RCON port to use");

        private static readonly Argument<string> MapArgument =
            new("map", () => "dayzOffline.chernarusplus", "The map to use");

        private static readonly Argument<uint> SlotsArgument =
            new("slots", () => 32, "The number of slots to use");

        private static readonly Argument<bool> AutoStartArgument =
            new("auto-start", () => true, "Whether to automatically start the server");

        private static readonly Argument<bool> AutoRestartArgument =
            new("auto-restart", () => true, "Whether to automatically restart the server");

        private static readonly Argument<bool> AutoUpdateArgument =
            new("auto-update", () => true, "Whether to automatically update the server");

        private static readonly Argument<bool> DoLogsArgument =
            new("do-logs", () => true, "Whether to log server output");

        private static readonly Argument<bool> AdminLogArgument =
            new("admin-log", () => true, "Whether to log admin actions");

        private static readonly Argument<bool> NetLogArgument =
            new("net-log", () => true, "Whether to log network traffic");

        private static readonly Argument<string> AdditionalStartParamsArgument =
            new("additional-start-params", () => "", "Additional parameters to pass to the server");

        public ServerAddCommand() : base("add", "Add a new server")
        {
            AddArgument(NameArgument);
            AddArgument(BindIpArgument);
            AddArgument(GamePortArgument);
            AddArgument(QueryPortArgument);
            AddArgument(RconPortArgument);
            AddArgument(MapArgument);
            AddArgument(SlotsArgument);
            AddArgument(AutoStartArgument);
            AddArgument(AutoRestartArgument);
            AddArgument(AutoUpdateArgument);
            AddArgument(DoLogsArgument);
            AddArgument(AdminLogArgument);
            AddArgument(NetLogArgument);
            AddArgument(AdditionalStartParamsArgument);

            this.SetHandler(AddServer, NameArgument);
        }

        private static void AddServer(string name)
        {
            throw new NotImplementedException();
        }
    }
}
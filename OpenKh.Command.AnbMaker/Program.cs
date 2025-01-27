using McMaster.Extensions.CommandLineUtils;
using NLog;
using OpenKh.Command.AnbMaker.Commands;
using System.Reflection;

namespace OpenKh.Command.AnbMaker
{
    [Command("OpenKh.Command.AnbMaker")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(AnbCommand))]
    [Subcommand(typeof(ExportRawCommand))]
    [Subcommand(typeof(AnbExCommand))]
    [Subcommand(typeof(DumpNodeTreeCommand))]
    [Subcommand(typeof(DumpSrtCommand))]
    [Subcommand(typeof(RenderNodeTreeCommand))]
    [Subcommand(typeof(DumpMdlxSrtCommand))]
    internal class Program
    {
        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "?";

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        static int Main(string[] args)
        {
            try
            {
                try
                {
                    return CommandLineApplication.Execute<Program>(args);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                    return 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                    return 1;
                }
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}

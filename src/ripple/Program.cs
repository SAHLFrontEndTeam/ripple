using System;
using FubuCore.CommandLine;
using System.Diagnostics;

namespace ripple
{
    internal class Program
    {
        private static bool success;

        private static int Main(string[] args)
        {
            try
            {
                Debugger.Break();
                var factory = new CommandFactory();
                factory.RegisterCommands(typeof (Program).Assembly);

                var executor = new CommandExecutor(factory);
                success = executor.Execute(args);
            }
            catch (CommandFailureException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + e.Message);
                Console.ResetColor();
                return 1;
            }
            catch (RippleFatalError)
            {
                return 1;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex);
                Console.ResetColor();
                return 1;
            }
            
            return success ? 0 : 1;
        }
    }


}
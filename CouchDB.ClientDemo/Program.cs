using System;
using System.Collections.Generic;
using System.Linq;

namespace CouchDB.ClientDemo
{
    class Program
    {
        private static readonly ConsoleCommands _commands = new ConsoleCommands();

        static void Main(string[] args)
        {
            while(true)
            {
                var commands = GetAllCommands();
                Console.WriteLine("Available commands: {0}", string.Join("; ", commands));

                Console.WriteLine("Input command:");
                var inputCommand = Console.ReadLine();
                if (inputCommand.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                TryExecuteCommand(inputCommand);
            }
        }

        private static IEnumerable<string> GetAllCommands()
        {
            var commandsType = _commands.GetType();
            var allMethods = commandsType.GetMethods();
            return allMethods
                .Where(m => m.DeclaringType.Equals(commandsType))
                .Select(m => m.Name);
        }

        private static void TryExecuteCommand(string inputCommand)
        {
            try
            {
                var matchingMethod = _commands.GetType().GetMethods()
                    .Where(m => m.Name.Equals(inputCommand, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();

                if (matchingMethod != null)
                    matchingMethod.Invoke(_commands, null);
            }
            catch(Exception ex)
            {
                Console.WriteLine(SerializationHelper.Serialize(ex));
            }
        }
    }
}

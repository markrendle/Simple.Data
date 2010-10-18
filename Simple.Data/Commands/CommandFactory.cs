using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Commands
{
    class CommandFactory
    {
        private static readonly List<ICommand> Commands = new List<ICommand>
                                                              {
                                                                  new FindCommand(),
                                                                  new FindByCommand(),
                                                                  new FindAllByCommand(),
                                                                  new FindAllCommand(),
                                                                  new UpdateByCommand(),
                                                                  new InsertCommand(),
                                                                  new DeleteCommand()
                                                              };

        public static ICommand GetCommandFor(string method)
        {
            return Commands.SingleOrDefault(command => command.IsCommandFor(method));
        }
    }
}

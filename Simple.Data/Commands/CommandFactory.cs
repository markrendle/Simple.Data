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
                                                                  new FindAllCommand(),
                                                                  new FindAllByCommand(),
                                                                  new AllCommand(),
                                                                  new UpdateByCommand(),
                                                                  new UpdateCommand(),
                                                                  new InsertCommand(),
                                                                  new DeleteByCommand(),
                                                                  new QueryByCommand(),
                                                              };

        public static ICommand GetCommandFor(string method)
        {
            return Commands.SingleOrDefault(command => command.IsCommandFor(method));
        }
    }
}

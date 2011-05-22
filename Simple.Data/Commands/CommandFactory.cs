using System.Collections.Concurrent;
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
                                                                  new UpdateAllCommand(),
                                                                  new UpdateCommand(),
                                                                  new InsertCommand(),
                                                                  new DeleteByCommand(),
                                                                  new DeleteAllCommand(),
                                                                  new QueryByCommand(),
                                                                  new CountCommand(),
                                                              };

        private static readonly ConcurrentDictionary<string, ICommand> Cache = new ConcurrentDictionary<string, ICommand>();

        public static ICommand GetCommandFor(string method)
        {
            return Cache.GetOrAdd(method, m => Commands.SingleOrDefault(command => command.IsCommandFor(method)));
        }
    }
}

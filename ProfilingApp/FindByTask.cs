using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProfilingApp
{
    using Simple.Data;

    class FindByTask : IProfileTask
    {
        public void Run()
        {
            for (int i = 1; i < 101; i++)
            {
                GetPostAndPrintTitle(i);
            }
        }

        private static void GetPostAndPrintTitle(int i)
        {
            var post = Database.OpenConnection(Properties.Settings.Default.ConnectionString).Posts.FindById(i);
            Console.WriteLine(post.Title);
        }
    }
}

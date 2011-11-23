using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProfilingApp
{
    using Simple.Data;

    class FindByTask : IProfileTask
    {
        private readonly dynamic _db = Database.OpenConnection(Properties.Settings.Default.ConnectionString);
        public void Run()
        {
            for (int i = 1; i < 101; i++)
            {
                GetPostAndPrintTitle(i);
            }
        }

        private void GetPostAndPrintTitle(int i)
        {
            var post = _db.Posts.FindById(i);
            Console.WriteLine(post.Title);
        }
    }
}

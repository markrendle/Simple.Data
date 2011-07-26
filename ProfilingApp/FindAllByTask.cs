namespace ProfilingApp
{
    using System;
    using Simple.Data;

    class FindAllByTask : IProfileTask
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
            var post = Database.OpenConnection(Properties.Settings.Default.ConnectionString).Posts.FindAllById(i).FirstOrDefault();
            Console.WriteLine(post.Title);
        }
    }
}
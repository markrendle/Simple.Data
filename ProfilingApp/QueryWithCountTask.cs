namespace ProfilingApp
{
    using System;
    using Simple.Data;

    class QueryWithCountTask : IProfileTask
    {
        private readonly dynamic _db = Database.OpenConnection(Properties.Settings.Default.ConnectionString);
        public void Run()
        {
            for (int i = 0; i < 100; i++)
            {
                GetPostAndPrintTitle(i);
            }
        }
        private void GetPostAndPrintTitle(int i)
        {
            Future<int> count;
            var post = _db.Posts.All()
                .WithTotalCount(out count)
                .Skip(i)
                .Take(1)
                .FirstOrDefault();

            Console.WriteLine(string.Format("{0}/{1}: {2}", i + 1, count.Value, post.Title));
        }
    }
}
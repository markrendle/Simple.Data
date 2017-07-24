using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Shitty.Data;

namespace ProfilingApp
{
    public class CastTask : IProfileTask
    {
        private readonly dynamic _db = Database.OpenConnection(Properties.Settings.Default.ConnectionString);

        public void Run()
        {
            var watch = Stopwatch.StartNew();

            List<Post> posts = _db.Posts.All().ToList<Post>();
            Console.WriteLine(posts.Count);
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }
    }

    public class Post
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
    }
}
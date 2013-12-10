using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PerformanceTestConsole
{
    using System.Data;
    using Simple.Data.Ado;

    class Post
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastChangeDate { get; set; }
        public int? Counter1 { get; set; }
        public int? Counter2 { get; set; }
        public int? Counter3 { get; set; }
        public int? Counter4 { get; set; }
        public int? Counter5 { get; set; }
        public int? Counter6 { get; set; }
        public int? Counter7 { get; set; }
        public int? Counter8 { get; set; }
        public int? Counter9 { get; set; }

    }

    class Program
    {

        public static readonly string ConnectionString = "Data Source=.;Initial Catalog=tempdb;Integrated Security=true";

        public static SqlConnection GetOpenConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        static void RunPerformanceTests()
        {
            var test = new PerformanceTests();
            Console.WriteLine("Running 500 iterations that load up a post entity");
            test.Run(500);
        }

        static void Main(string[] args)
        {

            EnsureDBSetup();
            RunPerformanceTests();
            if (Debugger.IsAttached) Console.ReadKey();
        }

        private static void EnsureDBSetup()
        {
            using (var cnn = GetOpenConnection())
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = @"
if (OBJECT_ID('Posts') is null)
begin
	create table Posts
	(
		Id int identity primary key, 
		[Text] varchar(max) not null, 
		CreationDate datetime not null, 
		LastChangeDate datetime not null,
		Counter1 int,
		Counter2 int,
		Counter3 int,
		Counter4 int,
		Counter5 int,
		Counter6 int,
		Counter7 int,
		Counter8 int,
		Counter9 int
	)
	   
	set nocount on 

	declare @i int
	declare @c int

	declare @id int

	set @i = 0

	while @i < 5000
	begin 
		
		insert Posts ([Text],CreationDate, LastChangeDate) values (replicate('x', 2000), GETDATE(), GETDATE())
		set @id = @@IDENTITY
		
		set @i = @i + 1
	end
end
";
                cmd.Connection = cnn;
                cmd.ExecuteNonQuery();
            }
        }

        private static void RunTests()
        {
            var tester = new Tests();
            foreach (var method in typeof(Tests).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Console.Write("Running " + method.Name);
                method.Invoke(tester, null);
                Console.WriteLine(" - OK!");
            }
        }
    }
    class PerformanceTests
    {

        class Test
        {
            public static Test Create(Action<int> iteration, string name)
            {
                return new Test { Iteration = iteration, Name = name };
            }

            public Action<int> Iteration { get; set; }
            public string Name { get; set; }
            public Stopwatch Watch { get; set; }
        }

        class Tests : List<Test>
        {
            public void Add(Action<int> iteration, string name)
            {
                Add(Test.Create(iteration, name));
            }

            public void Run(int iterations)
            {
                // warmup 
                foreach (var test in this)
                {
                    test.Iteration(iterations + 1);
                    test.Watch = new Stopwatch();
                    test.Watch.Reset();
                }

                var rand = new Random();
                for (int i = 1; i <= iterations; i++)
                {
                    foreach (var test in this.OrderBy(ignore => rand.Next()))
                    {
                        test.Watch.Start();
                        test.Iteration(i);
                        test.Watch.Stop();
                    }
                }

                foreach (var test in this.OrderBy(t => t.Watch.ElapsedMilliseconds))
                {
                    Console.WriteLine(test.Name + " took " + test.Watch.ElapsedMilliseconds + "ms");
                }
            }
        }

        public void Run(int iterations)
        {
            var tests = new Tests();
            var simpleDb = Simple.Data.Database.OpenConnection(Program.ConnectionString);
            SqlConnection connection = Program.GetOpenConnection();
            simpleDb.UseSharedConnection(connection);
            simpleDb.Posts.FindById(1);
            tests.Add(id => simpleDb.Posts.FindById(id), "Dynamic Simple.Data Query");

            // HAND CODED 


            var postCommand = new SqlCommand();
            postCommand.Connection = connection;
            postCommand.CommandText = @"select top 1 Id, [Text], [CreationDate], LastChangeDate, 
                Counter1,Counter2,Counter3,Counter4,Counter5,Counter6,Counter7,Counter8,Counter9 from Posts where Id = @Id";
            var idParam = postCommand.Parameters.Add("@Id", System.Data.SqlDbType.Int);

            tests.Add(id =>
            {
                idParam.Value = id;

                using (var reader = postCommand.ExecuteReader())
                {
                    reader.Read();
                    var post = new Post();
                    post.Id = reader.GetInt32(0);
                    post.Text = reader.GetNullableString(1);
                    post.CreationDate = reader.GetDateTime(2);
                    post.LastChangeDate = reader.GetDateTime(3);

                    post.Counter1 = reader.GetNullableValue<int>(4);
                    post.Counter2 = reader.GetNullableValue<int>(5);
                    post.Counter3 = reader.GetNullableValue<int>(6);
                    post.Counter4 = reader.GetNullableValue<int>(7);
                    post.Counter5 = reader.GetNullableValue<int>(8);
                    post.Counter6 = reader.GetNullableValue<int>(9);
                    post.Counter7 = reader.GetNullableValue<int>(10);
                    post.Counter8 = reader.GetNullableValue<int>(11);
                    post.Counter9 = reader.GetNullableValue<int>(12);
                }
            }, "hand coded");

            tests.Run(iterations);

        }

    }
    static class TestAssertions
    {

        public static void IsEquals<T>(this T obj, T other)
        {
            if (!obj.Equals(other))
            {
                throw new ApplicationException(string.Format("{0} should be equals to {1}", obj, other));
            }
        }

        public static void IsSequenceEqual<T>(this IEnumerable<T> obj, IEnumerable<T> other)
        {
            if (!obj.SequenceEqual(other))
            {
                throw new ApplicationException(string.Format("{0} should be equals to {1}", obj, other));
            }
        }

        public static void IsFalse(this bool b)
        {
            if (b)
            {
                throw new ApplicationException("Expected false");
            }
        }

        public static void IsNull(this object obj)
        {
            if (obj != null)
            {
                throw new ApplicationException("Expected null");
            }
        }

    }

    class Tests
    {

        SqlConnection connection = Program.GetOpenConnection();


    }
    static class SqlDataReaderHelper
    {
        public static string GetNullableString(this SqlDataReader reader, int index)
        {
            object tmp = reader.GetValue(index);
            if (tmp != DBNull.Value)
            {
                return (string)tmp;
            }
            return null;
        }

        public static Nullable<T> GetNullableValue<T>(this SqlDataReader reader, int index) where T : struct
        {
            object tmp = reader.GetValue(index);
            if (tmp != DBNull.Value)
            {
                return (T)tmp;
            }
            return null;
        }
    }
}

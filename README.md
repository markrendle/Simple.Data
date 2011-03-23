# Fix
An ultra-lightweight web glue for .NET, written in C#.
## What?
Fix joins together web servers, request handlers and "infixes", or middleware, in such a way that the implementations of each don't need to know anything about each other, or, indeed, about Fix itself.
### Example?
This is a Console application which runs a web server using Fix:

    class Program
    {
        static void Main()
        {
            using (var server = new Server("http://*:81/"))
            {
                var fixer = new Fixer(server.Start, server.Stop);
                fixer.AddHandler(new RequestPrinter().PrintRequest);
                fixer.Start();
                Console.Write("Running. Press Enter to stop.");
                Console.ReadLine();
                fixer.Stop();
            }
        }
    }
Fixer, Server and RequestPrinter are all in separate assemblies, with no dependencies between them. The Console application has references to all the assemblies, and uses Fix to hook everything up.
### More example?
    class Program
    {
        static void Main()
        {
            using (var server = new Server("http://*:81/"))
            {
                var fixer = new Fixer(server.Start, server.Stop);
                fixer.AddHandler(new RequestPrinter().PrintRequest);
                fixer.AddHandler(new InfoPrinter().PrintInfo);
                fixer.AddInfix(new MethodDownshifter().DownshiftMethod);
                fixer.Start();
                Console.Write("Running. Press Enter to stop.");
                Console.ReadLine();
                fixer.Stop();
            }
        }
    }
In this case, we are adding two handlers, either of which could serve the request. We're also adding an Infix, which can modify the request before it is passed to the handlers.
##Why?
Partly because it's interesting to boil something like a web application server down to the bare minimum like this.

More importantly, by relying entirely on .NET standard Action and Func delegates, Fix eliminates unnecessary coupling between classes and assemblies, as well as dependencies on itself.
So you could write your own Fixer class and use that to wire up any servers, handlers or modules that would work with this "reference implementation".

Another benefit is that because Fix takes a functional approach to the problem, it is friendlier to functional languages like F# and Clojure.
And functional languages are ideal for writing web applications, which aren't supposed to maintain any state anyway.
Ideally, a request will come in and be turned into a response by a series of operations.

###Action and Func, eh?
Yes, and the actual signatures are hideous. What I've done in this code is to add "using" aliases for them, to make it easier to read:

    using RequestHandler = System.Action<string, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[], System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[]>>;
    using ResponseHandler = System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[]>;
    using Infix = System.Action<string, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[], System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[]>, System.Delegate>;
##Production-ready?
Good grief, no. There's a lot of discussion going on around this area at the moment (e.g. [the OWIN project](http://owin.github.com/owin))
and this is my contribution. The delegate signatures used are by no means ideal, particularly the *byte[]* type being used for the request
and response bodies, which should probably be a *Func of byte[]* or a *Task of byte[]* or something.

I'd love to hear people's thoughts on this. Best way is to catch me on [Twitter](http://twitter.com/markrendle).
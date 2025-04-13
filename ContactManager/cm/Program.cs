using System;
using ContactManager;

namespace cm
{
    class Program
    {
        static void Main()
        {
            ContactStore store = new ContactStore();
            //store = Store_MessageLogged;
            Repl repl = new Repl(Console.In, Console.Out, store);

            repl.Run();
        }
        private static void Store_MessageLogged(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);

        }
    }
}
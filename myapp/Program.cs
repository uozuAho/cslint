// unnecessary import
using System.IO;
using mylib;

// block scoped namespace, lowercase, wrong namespace. Want uppercased & file scoped
namespace cslint
{
    public static class Program
    {
        public static async Task<int> Main()
        {
            var asdf = await DoThing();
            Console.WriteLine(asdf);

            // redundant qualifier
            mylib.Class1.SayHello();

            // long line
            var alskdjflaksjdflkjasjdfljkasdjfalskdfj = 123456789 + 123456789 + 123456789 + 123456789 + 123456789 + 123456789;
            Console.WriteLine(alskdjflaksjdflkjasjdfljkasdjfalskdfj);

            // missing await. why doesn't this trigger CS4014? https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs4014
            // I want to await this, not _ = DoThing();
            var x = DoThing();

            // "" is easier to read than String.Empty
            Console.WriteLine("");

            return 0;
        }

        // naming: Async suffix
        private static async Task<int> DoThing()
        {
            await Task.Delay(1);
            // if block should be in curlies
            if (1 == 1) Console.Write(1);
            SomeSyncMethod();
            return 1;

            // unnecessary whitespace


        }

        // don't do async void
        private static async void BadAsyncVoidFunc()
        {
            await Task.Delay(1);
        }

        private static void SomeSyncMethod()
        {
            // dotnet format quirk:
            // I want this method to become async, and to await the call to
            // BadAsyncVoidFunc. However, dotnet format updates the call to:
            // _ = BadAsyncVoidFunc();
            // and keeps this method sync. Am I wrong?
            // Also, this takes two passes of dotnet format to fix
            BadAsyncVoidFunc();
        }

        // regions suck
        #region AASDF
        #endregion
    }
    // more unnecessary whitespace

}



// even more unnecessary whitespace

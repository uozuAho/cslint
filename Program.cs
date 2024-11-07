// unnecessary import
using System.IO;

namespace cslint
{
    public static class Program
    {
        public static async Task<int> Main()
        {
            var asdf = await DoThing();
            Console.WriteLine(asdf);

            // long line
            var alskdjflaksjdflkjasjdfljkasdjfalskdfj = 123456789 + 123456789 + 123456789 + 123456789 + 123456789 + 123456789;
            Console.WriteLine(alskdjflaksjdflkjasjdfljkasdjfalskdfj);

            // missing await. why doesn't this trigger CS4014? https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs4014
            var x = DoThing();
            return 0;
        }

        // naming: Async suffix
        private static async Task<int> DoThing()
        {
            await Task.Delay(1);
            // if block should be in curlies
            if (1 == 1) Console.Write(1);
            return 1;

            // unnecessary whitespace

        }

        // don't do async void
        private static async void BadAsyncVoidFunc()
        {
            await Task.Delay(1);
        }

        // regions suck
        #region AASDF
        #endregion
    }
}

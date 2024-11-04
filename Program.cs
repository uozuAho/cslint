namespace cslint
{
    public static class Program
    {
        public static async Task<int> Main()
        {
            Console.WriteLine("yo");
            var asdf = await DoThing();

            // long line
            var alskdjflaksjdflkjasjdfljkasdjfalskdfj = 123456789 + 123456789 + 123456789 + 123456789 + 123456789 + 123456789;
            Console.WriteLine(alskdjflaksjdflkjasjdfljkasdjfalskdfj);

            // missing await
            var x = DoThing();
            return 0;
        }

        // naming: Async suffix
        private static async Task<int> DoThing()
        {
            await Task.Delay(1);
            // if block should be in curlies
            if (1 == 1) Console.Write("yep");
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

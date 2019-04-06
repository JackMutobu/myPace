using System;
using Windows.UI.Xaml;

namespace myPace.Wasm
{
    public class Program
    {
        private static App _app;

        static void Main(string[] args)
        {
            //Commented on webassembly
            Windows.UI.Xaml.Application.Start(_ => _app = new App());
        }
    }
}

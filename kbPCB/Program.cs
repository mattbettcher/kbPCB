using System;

namespace kbPCB
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            kbPCB app = new kbPCB();
            app.Run();
        }
    }
}

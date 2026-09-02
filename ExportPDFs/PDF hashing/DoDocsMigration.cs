using Serilog;

namespace PDF_hashing
{
    internal partial class Program
    {
        private static void DoDocsMigration()
        {
            Log.Warning("DoDocsMigration is starting.");
            Console.WriteLine("DoDocsMigration is starting");


            Log.Warning("DoDocsMigration has ended.");
            Console.WriteLine("DoDocsMigration has ended");
        }
    }
}
using System;
using System.Threading;

namespace DBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
            Thread.Sleep(-1);
        }
    }
}

using System;
using System.Threading;

namespace ENTERTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            Console.WriteLine("Waiting for ENTER");

            Console.ReadLine();

            Console.WriteLine("ENTER received, shutting down");

            Thread.Sleep(3000);
        }
    }
}

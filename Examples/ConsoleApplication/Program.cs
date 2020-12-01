using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting test application.");


            for(int i = 0;i < 100;i++)
            {
                Console.WriteLine(i.ToString());
                Thread.Sleep(500);
            }

            Console.WriteLine("Exit test application.");

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InputExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to our Input test application.");


            do
            {
                int first = -1;

                do
                {
                    Console.WriteLine("Send me a first number between 0 and 100: ");

                    String s = Console.ReadLine();

                    int.TryParse(s, out first);

                    if (first < 0 | first > 100)
                        first = -1;

                } while (first == -1);


                int second = -1;

                do
                {
                    Console.WriteLine("Send me a second number between 0 and 100: ");

                    String s = Console.ReadLine();

                    int.TryParse(s, out second);

                    if (second < 0 | second > 100)
                        second = -1;

                } while (second == -1);


                int sum = first + second;

                Console.WriteLine("The sum of both is: " + sum.ToString());

                Console.WriteLine("Start over ? (y/n)");

            } while (Console.ReadLine() == "y");

            Console.WriteLine("Ok, will close the application now.");

            Thread.Sleep(3000);


        }
    }
}

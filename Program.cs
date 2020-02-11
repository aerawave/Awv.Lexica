using Lexica.Parsing;
using System;
using System.IO;

namespace Lexica.Local
{
    class Program
    {
        static void Main(string[] args)
        {
            var example = "Have a `ri(1,100)`(chance)% chance when struck in combat of increasing armor by `ri(1,999)`(armor) for `randomt(1s,1d)`. That chance is: `chance`";
            var exampleTokens = new LexParser(example).Transpile();

            Console.WriteLine($"Input string: {exampleTokens.ToString()}");
            Console.WriteLine();
            var engine = new LexEngine();
            engine.RegisterLibrary<SystemLibrary>();
            engine.RegisterLibrary<WoWLibrary>();
            for (var i = 0; i < 15;i++)
            {
                Console.WriteLine($"Example {i + 1}: {exampleTokens.Build(engine)}");
            }
            Console.ReadLine();
        }
    }
}

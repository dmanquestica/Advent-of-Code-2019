using Shared_Utilities;
using System;
using System.Linq;
using System.Numerics;

namespace Day_22
{
    public class Program
    {
        //public static void Main(string[] args)
        //{
        //    var test = new UnitTest1(args[0]);
        //    Console.WriteLine($"Part 1: {test.Test2()}");
        //    Console.ReadKey();
        //}
    }

    public class UnitTest1
    {
        private string Path;

        public UnitTest1(string path)
        {
            Path = path;
        }

        public BigInteger Test2()
        {
            var lines = Utilities.ReadFile(Path).Reverse().ToArray();
            BigInteger deckSize = 119315717514047L;

            BigInteger a = 1;
            BigInteger b = 0;

            foreach (var line in lines)
            {
                switch (line)
                {
                    case var s when s.StartsWith("deal with increment"):
                        var param = int.Parse(s.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last());

                        // modInverse for prime base https://en.wikipedia.org/wiki/Modular_multiplicative_inverse
                        var inversePower = BigInteger.ModPow(param, deckSize - 2, deckSize);
                        a = a * inversePower % deckSize;
                        b = b * inversePower % deckSize;

                        break;
                    case var s when s.StartsWith("cut"):
                        // cutting just shifts the B parameter - the offset
                        var i = BigInteger.Parse(s.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last());

                        if (i < 0)
                        {
                            i += deckSize;
                        }
                        b += i;
                        break;
                    case var s when s.StartsWith("deal into new"):
                        // reverse changes sign (offset changes by one before sign inverse)
                        a = -a;
                        b = -++b;
                        break;
                }

                a %= deckSize;
                b %= deckSize;
                while (b < 0)
                    b += deckSize;
                while (a < 0)
                    a += deckSize;
            }

            const long N = 101741582076661;

            // we are looking for an N-th power of the equation describing the function composition:
            // (a*x + b)^N
            // ax + b
            // a^2x + ab + b
            // a^3x + a^2b + ab + b
            // a^4x + a^3b + a^2b + ab + b etc.
            // in general, the nth term looks like
            // a^nx + a^(n - 1)b + a^(n - 2)b + ... + a^2b + ab + b
            // which can be factored into
            // a^nx + b(1 - a^n) / (1 - a)

            var part1 = 2020 * BigInteger.ModPow(a, N, deckSize);
            var part2 = b * (BigInteger.ModPow(a, N, deckSize) - 1);
            var part3 = BigInteger.ModPow(a - 1, deckSize - 2, deckSize);

            var ans = (part1 + part2 * part3) % deckSize;
            return ans;
        }

    }
}
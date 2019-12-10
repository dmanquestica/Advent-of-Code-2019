using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Day_5
{
    enum Mode
    {
        Position,
        Immediate
    }

    class Program
    {
        static void Main(string[] args)
        {
            var inputValue = GetInputValue();
            var input = Utilities.ReadFileAsString(args[0]);
            ProcessCodes(input.Split(',').Select(int.Parse).ToArray(), inputValue);
            Console.ReadKey();
        }

        static void ProcessCodes(int[] intCodes, int inputValue)
        {
            var x = 0;

            while (x < intCodes.Length)
            {
                var opCode = 0;
                var param1 = 0;
                var param2 = 0;
                var param1Mode = Mode.Position;
                var param2Mode = Mode.Position;
                var instLength = 0;
                var currentCode = intCodes[x].ToString();

                if (currentCode == "99") break;

                switch (currentCode.Length)
                {
                    case 1:
                        opCode = int.Parse(currentCode);
                        break;
                    case 2:
                        opCode = int.Parse(currentCode[1].ToString());
                        param1Mode = (currentCode[0] == '0') ? Mode.Position : Mode.Immediate;
                        break;
                    case 3:
                        opCode = int.Parse(currentCode.Substring(1));
                        param1Mode = (currentCode[0] == '0') ? Mode.Position : Mode.Immediate;
                        break;
                    case 4:
                        opCode = int.Parse(currentCode.Substring(2));
                        param1Mode = (currentCode[1] == '0') ? Mode.Position : Mode.Immediate;
                        param2Mode = (currentCode[0] == '0') ? Mode.Position : Mode.Immediate;
                        break;
                    case 5:
                        opCode = int.Parse(currentCode.Substring(3));
                        param1Mode = (currentCode[2] == '0') ? Mode.Position : Mode.Immediate;
                        param2Mode = (currentCode[1] == '0') ? Mode.Position : Mode.Immediate;
                        break;
                }

                if (opCode != 3)
                    param1 = (param1Mode == Mode.Position) ? intCodes[intCodes[x + 1]] : intCodes[x + 1];

                if (opCode != 3 && opCode != 4)
                    param2 = (param2Mode == Mode.Position) ? intCodes[intCodes[x + 2]] : intCodes[x + 2];

                switch (opCode)
                {
                    case 1:
                        instLength = 4;
                        intCodes[intCodes[x + 3]] = param1 + param2;
                        break;
                    case 2:
                        instLength = 4;
                        intCodes[intCodes[x + 3]] = param1 * param2;
                        break;
                    case 3:
                        instLength = 2;
                        intCodes[intCodes[x + 1]] = inputValue;
                        break;
                    case 4:
                        instLength = 2;
                        Console.WriteLine($"Output Code: {param1}");
                        break;
                    case 5:
                        if (param1 != 0)
                            x = param2;
                        else
                            instLength = 3;
                        break;
                    case 6:
                        if (param1 == 0)
                            x = param2;
                        else
                            instLength = 3;
                        break;
                    case 7:
                        instLength = 4;
                        intCodes[intCodes[x + 3]] = (param1 < param2) ? 1 : 0;
                        break;
                    case 8:
                        instLength = 4;
                        intCodes[intCodes[x + 3]] = (param1 == param2) ? 1 : 0;
                        break;
                }
                x += instLength;
            }
        }

        static int GetInputValue()
        {
            Console.WriteLine("Please provide the numeric id of the system to test:");

            int inputValue;
            while (!int.TryParse(Console.ReadLine(), out inputValue))
            {
                Console.WriteLine("The id you have entered is not numeric, please try again.");
            }

            return inputValue;
        }
    }
}

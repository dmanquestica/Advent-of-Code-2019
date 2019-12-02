using Shared_Utilities;
using System;
using System.Linq;

namespace Day_2
{
	public class Program
	{
		public static int NUMBER_OF_VALUES_IN_INSTRUCTIONS = 4;

		public static void Main(string[] args)
		{
			var instructions = Utilities.ReadFileAsString(args[0]);

			// Part 1
			var ary = instructions.Split(',').Select(Int32.Parse).ToArray();

			var errorCode = "1202";

			var replace1 = int.Parse(errorCode.Substring(0, 2));
			var replace2 = int.Parse(errorCode.Substring(2, 2));

			Console.WriteLine(ReturnCode(ary, replace1, replace2));

			// Part 2

			int[] ary2 = new int[ary.Length];
			for (int i = 1; i < 100; ++i)
			{
				for (int j = 1; j < 100; ++j)
				{
					ary2 = instructions.Split(',').Select(Int32.Parse).ToArray();
					if (ReturnCode(ary2, j, i) == 19690720)
						goto Found;
				}
			}
		Found:
			Console.WriteLine($"Noun {ary2[1]}, Verb {ary2[2]}: {100 * ary2[1] + ary2[2]}");
			Console.ReadKey();
		}

		public static int ReturnCode(int[] ary, int noun, int verb)
		{
			ary[1] = noun;
			ary[2] = verb;

			var pointerOp = 0;
			var pointer1 = 1;
			var pointer2 = 2;
			var pointer3 = 3;

			while (ary[pointerOp] != 99)
			{
				if (ary[pointerOp] == 1)
					ary[ary[pointer3]] = ary[ary[pointer1]] + ary[ary[pointer2]];
				if (ary[pointerOp] == 2)
					ary[ary[pointer3]] = ary[ary[pointer1]] * ary[ary[pointer2]];
				pointerOp += NUMBER_OF_VALUES_IN_INSTRUCTIONS;
				pointer1 += NUMBER_OF_VALUES_IN_INSTRUCTIONS;
				pointer2 += NUMBER_OF_VALUES_IN_INSTRUCTIONS;
				pointer3 += NUMBER_OF_VALUES_IN_INSTRUCTIONS;
			}
			return ary[0];
		}
	}
}

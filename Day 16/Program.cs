using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Day_16
{
	class Program
	{
		static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0])
				.ToCharArray()
				.Select(c => Int64.Parse(c.ToString()))
				.ToArray();

			var basePattern = new Int64[] { 0, 1, 0, -1 };

			Part1((long[])input.Clone(), basePattern);
			Part2((long[])input.Clone());
			Console.ReadKey();
		}

		private static void Part1(Int64[] input, Int64[] pattern)
		{
			for (int i = 0; i < 100; i++)
			{
				for (int j = 0; j < input.Length; j++)
				{
					var newPattern = new List<Int64>();
					foreach (int k in pattern)
					{
						for (int l = 0; l < j + 1; l++)
						{
							newPattern.Add(k);
						}
					}
					Int64 total = 0;
					for (int k = 0; k < input.Length; k++)
					{
						total += input[k] * newPattern[(k + 1) % newPattern.Count];
					}
					input[j] = Math.Abs(total) % 10;
				}
			}
			for (int i = 0; i < 8; i++)
			{
				Console.Write(input[i]);
			}
			Console.WriteLine();
		}

		private static void Part2(Int64[] input)
		{
			var realInput = new List<Int64>();
			for (int i = 0; i < 10000; i++)
			{
				for (int j = 0; j < input.Length; j++)
				{
					realInput.Add(input[j]);
				}
			}
			var messageOffset = string.Empty;
			for (int i = 0; i < 7; i++)
			{
				messageOffset += realInput[i];
			}
			int offset = int.Parse(messageOffset);
			Int64[] arr = realInput.ToArray();
			for (int i = 0; i < 100; i++)
			{
				for (int j = arr.Length - 1; j > offset - 1; j--)
				{
					arr[j - 1] += arr[j];
					arr[j - 1] = arr[j - 1] % 10;
				}
			}
			for (int i = 0; i < 8; i++)
			{
				Console.Write(arr[offset + i]);
			}
			Console.WriteLine();
		}
	}
}

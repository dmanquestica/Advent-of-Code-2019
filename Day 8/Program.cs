using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Day_8
{
	class Layer
	{
		public int[] Data;

		public Layer(string input)
		{
			Data = input.ToCharArray().Select(c => int.Parse(c.ToString())).ToArray();
		}

		public int CountZeros()
		{
			return Data.Count(i => i == 0);
		}

		public int CountOnes()
		{
			return Data.Count(i => i == 1);
		}

		public int CountTwos()
		{
			return Data.Count(i => i == 2);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0]);

			var layers = new List<Layer>();

			for (int i = 0; i < input.Length; i += (25 * 6))
			{
				var layer = new Layer(input.Substring(i, 25 * 6));
				layers.Add(layer);
			}

			Part1(layers);
			Part2(layers);
			Console.ReadKey();
		}

		public static void Part1(List<Layer> layers)
		{
			var minZeros = int.MaxValue;

			foreach (var l in layers)
				minZeros = Math.Min(minZeros, l.CountZeros());

			var minZeroLayer = layers.Where(l => l.CountZeros() == minZeros).FirstOrDefault();

			Console.WriteLine($"Part 1: {minZeroLayer.CountOnes() * minZeroLayer.CountTwos()}");
		}

		public static void Part2(List<Layer> layers)
		{
			var image = new int[25 * 6];

			for (int i = 0; i < 25 * 6; ++i)
				image[i] = 2;

			foreach (var l in layers)
			{
				for (int i = 0; i < 25 * 6; ++i)
				{
					if (l.Data[i] == 0 && image[i] == 2)
						image[i] = 0;
					else if (l.Data[i] == 1 && image[i] == 2)
						image[i] = 1;
				}
			}
			Console.WriteLine("Part 2:");
			PrintMessage(image);
		}

		public static void PrintMessage(int[] image)
		{
			for (int j = 0; j < 6; ++j)
			{
				for (int i = 0; i < 25; ++i)
				{
					if (image[j * 25 + i] == 1)
						Console.Write("1");
					else
						Console.Write(" ");
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}
	}
}

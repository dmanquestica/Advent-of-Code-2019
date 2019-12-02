using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_1
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var input = Utilities.ReadFile(args[0]);

			// Part 1
			var sum = 0;

			foreach (var mass in input)
			{
				sum += CalculateFuel(mass, 3, -2);
			}

			Console.WriteLine(sum);

			// Part 2

			sum = 0;
			int i = 0;

			foreach (var mass in input)
			{
				var partialFuel = 0;

				var fuel = CalculateFuel(mass, 3, -2);
				partialFuel += fuel;
				do
				{
					fuel = CalculateFuel(fuel, 3, -2);
					partialFuel += fuel;
				} while (fuel > 6);

				sum += partialFuel;
			}

			Console.WriteLine(sum);
			Console.ReadKey();

		}

		public static int CalculateFuel(string input, int divisor, int adjustment)
		{
			return int.Parse(input) / divisor + adjustment;
		}

		public static int CalculateFuel(int input, int divisor, int adjustment)
		{
			return input / divisor + adjustment;
		}
	}
}

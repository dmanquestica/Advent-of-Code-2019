using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_4
{
	class Program
	{
		static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0]);

			var split = input.Split('-');
			var min = int.Parse(split[0]);
			var max = int.Parse(split[1]);

			var candidates = new List<int>();

			for (int i = min; i <= max; ++i)
			{
				if (IsAlwaysIncreasing(i) && ContainsADouble(i))
					candidates.Add(i);
			}

			Console.WriteLine($"Part 1: {candidates.Count()}");

			candidates.RemoveAll(c => !OnlyAdjacentDigits(c));

			Console.WriteLine($"Part 2: {candidates.Count()}");

			Console.ReadKey();
		}

		public static bool IsAlwaysIncreasing(int number)
		{
			var password = number.ToString();
			return password[0] <= password[1] && password[1] <= password[2]
				&& password[2] <= password[3] && password[3] <= password[4]
				&& password[4] <= password[5];
		}

		public static bool ContainsADouble(int number)
		{
			var password = number.ToString();
			return password[0] == password[1] || password[1] == password[2]
				|| password[2] == password[3] || password[3] == password[4]
				|| password[4] == password[5];
		}

		public static bool OnlyAdjacentDigits(int number)
		{
			var password = number.ToString();
			return (password[0] == password[1] && password[1] != password[2])
				|| (password[0] != password[1] && password[1] == password[2] && password[2] != password[3])
				|| (password[1] != password[2] && password[2] == password[3] && password[3] != password[4])
				|| (password[2] != password[3] && password[3] == password[4] && password[4] != password[5])
				|| (password[3] != password[4] && password[4] == password[5]);
		}
	}
}

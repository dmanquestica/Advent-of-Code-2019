using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Day_14
{
	class Chemical
	{
		public int Amount;
		public string Name;

		public Chemical(int amount, string name)
		{
			Amount = amount;
			Name = name;
		}
		public Chemical(string amount, string name) : 
			this(int.Parse(amount), name)
		{	
		}
	}

	class Reaction
	{
		public List<Chemical> Reactants;
		public Chemical Product;
		public int Order = 0;

		public static Dictionary<string, Reaction> ReactionList;
		public static List<Reaction>[] Orders;

		public Reaction()
		{
			Reactants = new List<Chemical>();
		}

		private void SortReactions()
		{
			Order = 1;
			foreach (var c in Reactants)
			{
				if (c.Name != "ORE")
				{
					var r = ReactionList[c.Name];
					if (r.Order == 0)
						r.SortReactions();
					if (Order <= r.Order)
						Order = r.Order + 1;
				}
			}
		}

		public static void TopoSort()
		{
			var final = ReactionList["FUEL"];
			final.SortReactions();
			var last = final.Order;

			Orders = new List<Reaction>[last];
			for (int i = 0; i < last; i++)
				Orders[i] = new List<Reaction>();
			foreach (var r in ReactionList.Values)
				Orders[r.Order - 1].Add(r);
		}
	}

	class Program
	{
		private static void ParseInput(IList<string> input)
		{
			Reaction.ReactionList = new Dictionary<string, Reaction>();
			var re = new Regex(@"(\d+) ([A-Z]+)");
			foreach (var line in input)
			{
				var r = new Reaction();
				foreach (Match match in re.Matches(line))
				{
					var count = match.Groups.Count;
					for (int i = 0; i < count; i += 3)
					{
						var c = new Chemical(match.Groups[i + 1].Value, match.Groups[i + 2].Value);
						r.Reactants.Add(c);
					}
				}
				r.Product = r.Reactants[r.Reactants.Count - 1];
				r.Reactants.RemoveAt(r.Reactants.Count - 1);
				Reaction.ReactionList[r.Product.Name] = r;
			}
		}

		public static Int64 GetOreQuantity(Int64 fuel)
		{
			var reactantList = new Dictionary<string, Int64>();
			foreach (var r in Reaction.ReactionList.Values)
			{
				reactantList[r.Product.Name] = 0;
			}
			reactantList["FUEL"] = fuel;
			reactantList["ORE"] = 0;

			int lastStage = Reaction.ReactionList["FUEL"].Order;

			for (int i = lastStage; i > 0; i--)
			{
				foreach (var r in Reaction.Orders[i - 1])
				{
					Int64 total = reactantList[r.Product.Name];
					Int64 unit = r.Product.Amount;
					Int64 quantity = total / unit;
					if (total % unit > 0)
						quantity++;
					foreach (var c in r.Reactants)
						reactantList[c.Name] += c.Amount * quantity;
				}
			}
			return reactantList["ORE"];
		}

		public static void Main(string[] args)
		{
			var input = Utilities.ReadFile(args[0]);
			ParseInput(input);

			// Part 1
			Reaction.TopoSort();
			Int64 orePerFuel = GetOreQuantity(1);
			Console.WriteLine($"Part 1: {orePerFuel}");

			// Part 2
			Int64 ore = 1000000000000;
			Int64 lowerBound = ore / orePerFuel;
			Int64 upperBound = ore / 2;

			while (GetOreQuantity(upperBound) <= ore)
				upperBound *= 2;
			while (upperBound > lowerBound + 1)
			{
				Int64 pivot = (upperBound + lowerBound) / 2;
				if (GetOreQuantity(pivot) <= ore)
					lowerBound = pivot;
				else
					upperBound = pivot;
			}
			Console.WriteLine($"Part 2: {lowerBound}");

			Console.ReadKey();
		}
	}
}

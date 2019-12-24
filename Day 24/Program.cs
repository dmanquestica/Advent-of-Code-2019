using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_24
{
	public class Program
	{
		// Part 1
		public static char[] Map1;
		public static HashSet<string> States;

		// Part 2
		public static Dictionary<int, char[]> Maps;
		public static Dictionary<int, char[]> MapsTemp;
		public static char[] Map2;
		public static char[] NewMap2;
		public static HashSet<int> Visited;
		public static HashSet<int> VisitedTemp;

		static void Main(string[] args)
		{
			var input = Utilities.ReadFile(args[0]);

			Part1(input);
			Part2(input);
			Console.ReadKey();
		}

		static void Part1(IList<string> input)
		{
			States = new HashSet<string>();

			InitializeMap(input, ref Map1);
			Display();
			while (true)
			{
				if (States.Contains(new string(Map1)))
				{
					break;
				}
				else
				{
					States.Add(new string(Map1));
					Step();
				}
			}
			Display();
			Console.WriteLine($"Part 1: {CalcBioDiversity()}");
		}
		static void InitializeMap(IList<string> input, ref char[] map)
		{
			map = new char[25];

			for (var j = 0; j < input.Count; ++j)
			{
				for (var i = 0; i < input[0].Length; ++i)
					map[j * 5 + i] = input[j][i];
			}
		}
		static void Step()
		{
			var dir = new (int dx, int dy)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };

			var NewMap = new char[25];

			for (var j = 0; j < 5; ++j)
			{
				for (var i = 0; i < 5; ++i)
				{
					var adjacents = new List<(int x, int y)>();
					foreach (var n in dir)
					{
						var nx = i + n.dx;
						var ny = j + n.dy;

						if (0 <= nx && 0 <= ny && nx < 5 && ny < 5)
							adjacents.Add((nx, ny));
					}
					if (Map1[j * 5 + i] == '#' && adjacents.Where(c => Map1[c.y * 5 + c.x] == '#').Count() != 1)
						NewMap[j * 5 + i] = '.';
					else if (Map1[j * 5 + i] == '.' && (adjacents.Where(c => Map1[c.y * 5 + c.x] == '#').Count() == 1
													|| adjacents.Where(c => Map1[c.y * 5 + c.x] == '#').Count() == 2))
						NewMap[j * 5 + i] = '#';
					else
						NewMap[j * 5 + i] = Map1[j * 5 + i];
				}
			}
			NewMap.CopyTo(Map1, 0);
		}
		static void Display()
		{
			for (var j = 0; j < 5; ++j)
			{
				for (var i = 0; i < 5; ++i)
					Console.Write(Map1[j * 5 + i]);
				Console.WriteLine();
			}
			Console.WriteLine();
		}
		static double CalcBioDiversity()
		{
			var result = 0d;

			for (int i = 0; i < Map1.Length; ++i)
				result += Map1[i] == '#' ? Math.Pow(2, i) : 0;
			return result;
		}

		static void Part2(IList<string> input)
		{
			InitializeMap(input, ref Map2);
			Maps = new Dictionary<int, char[]>();
			MapsTemp = new Dictionary<int, char[]>();
			Maps[500] = Map2;
			Visited = new HashSet<int>
			{
				500
			};
			for (int step = 0; step < 200; ++step)
			{
				int bugCount = 0;
				VisitedTemp = new HashSet<int>();
				foreach (var lvl in Visited)
				{
					Step(lvl - 1);
					Step(lvl);
					Step(lvl + 1);
				}
				foreach (var lvl in VisitedTemp)
				{
					CopyLevel(lvl);
					bugCount += CountBugs(lvl);
				}
				Console.WriteLine($"Bug count after {step+1}: {bugCount}");
				Visited = VisitedTemp;
			}
		}

		static void Step(int level)
		{
			if (VisitedTemp.Count() == level) return;

			VisitedTemp.Add(level);
			NewMap2 = new char[25];
			MapsTemp[level] = NewMap2;

			for (int j = 0; j < 5; ++j)
			{
				for (int i = 0; i < 5; ++i)
				{
					if (j == 2 && i == 2) continue;
					int n = Adjacents(level, i, j);
					if (Maps.ContainsKey(level) && (char)Maps[level].GetValue(j * 5 + i) == '#')
					{
						if (n != 1) MapsTemp[level][j * 5 + i] = '.';
						else MapsTemp[level][j * 5 + i] = '#';
					}
					else
					{
						if (n == 1 || n ==2 ) MapsTemp[level][j * 5 + i] = '#';
						else MapsTemp[level][j * 5 + i] = '.';
					}
				}
			}
		}

		static bool HasBugs(int level, int i, int j) {
			if (!Maps.ContainsKey(level)) return false;
			return Maps[level][j * 5 + i] == '#';
		}

		static int Adjacents(int lvl, int a, int b)
		{
			int s = 0;
			for (int i = a - 1; i <= a + 1; ++i)
				for (int j = b - 1; j <= b + 1; ++j)
				{
					if (j == b && i == a) continue;
					if (i < 0 || j < 0 || i >= 5 || j >= 5) continue;
					if (i == 2 && j == 2) continue;
					if (Math.Abs(i - a) + Math.Abs(j - b) != 1) continue;
					s += HasBugs(lvl, i, j) ? 1 : 0;
				}
			if (a == 0) s += HasBugs(lvl - 1, 1, 2) ? 1 : 0;
			if (a == 5 - 1) s += HasBugs(lvl - 1, 3, 2) ? 1 : 0;
			if (b == 0) s += HasBugs(lvl - 1, 2, 1) ? 1 : 0;
			if (b == 5 - 1) s += HasBugs(lvl - 1, 2, 3) ? 1 : 0;
			if (a == 1 && b == 2)
			{
				for (int i = 0; i < 5; ++i)
					s += HasBugs(lvl + 1, 0, i) ? 1 : 0;
			}
			if (a == 3 && b == 2)
			{
				for (int i = 0; i < 5; ++i)
					s += HasBugs(lvl + 1, 5 - 1, i) ? 1 : 0;
			}
			if (a == 2 && b == 1)
			{
				for (int i = 0; i < 5; ++i)
					s += HasBugs(lvl + 1, i, 0) ? 1 : 0;
			}
			if (a == 2 && b == 3)
			{
				for (int i = 0; i < 5; ++i)
					s += HasBugs(lvl + 1, i, 5 - 1) ? 1 : 0;
			}
			return s;
		}

		static void CopyLevel(int lvl)
		{
			Maps[lvl] = new char[25];
			MapsTemp[lvl].CopyTo(Maps[lvl], 0);
		}

		static int CountBugs(int lvl)
		{
			return Maps[lvl].Where(c => c.Equals('#')).Count();
		}
	}
}

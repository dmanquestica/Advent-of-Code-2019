using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_3
{
	public class Point
	{
		public int X;
		public int Y;
		public int dist = 0;

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}

		public void GoRight(int x)
		{
			X += x;
			dist += x;
		}

		public void GoLeft(int x)
		{
			X -= x;
			dist += x;
		}

		public void GoUp(int y)
		{
			Y += y;
			dist += y;
		}

		public void GoDown(int y)
		{
			Y -= y;
			dist += y;
		}
	}

	public class WireSegment {

		public char Direction;
		public int Length;

		public WireSegment(char dir, int length)
		{
			Direction = dir;
			Length = length;
		}
	}

	public class Program
	{
		public static int GRID_SIZE = 15;
		public static int STARTING_POINT = 1;

		public static void Main(string[] args)
		{
			var input = Utilities.ReadFile(args[0]);

			var wire1 = input[0];
			var wire2 = input[1];

			var wireSegments1 = new List<WireSegment>();
			foreach (var s in wire1.Split(','))
			{
				wireSegments1.Add(new WireSegment(s[0], int.Parse(s.Substring(1))));
			}

			var wireSegments2 = new List<WireSegment>();
			foreach (var s in wire2.Split(','))
			{
				wireSegments2.Add(new WireSegment(s[0], int.Parse(s.Substring(1))));
			}

			var point1 = new Point(0, 0);
			var point2 = new Point(0, 0);

			var route1points = new List<(int x, int y, int dist)>();
			var route2points = new List<(int x, int y, int dist)>();
			var intersections = new List<(int x, int y, int dist)>();

			foreach (var s in wireSegments1)
			{
				switch (s.Direction)
				{
					case 'R':
						for (int i = 0; i < s.Length; ++i)
						{
							point1.GoRight(1);
							if (!route1points.Exists(r => r.x == point1.X && r.y == point1.Y))
								route1points.Add((point1.X, point1.Y, point1.dist));
						}
						break;

					case 'L':
						for (int i = 0; i < s.Length; ++i)
						{
							point1.GoLeft(1);
							if (!route1points.Exists(r => r.x == point1.X && r.y == point1.Y))
								route1points.Add((point1.X, point1.Y, point1.dist));
						}
						break;

					case 'D':
						for (int i = 0; i < s.Length; ++i)
						{
							point1.GoDown(1);
							if (!route1points.Exists(r => r.x == point1.X && r.y == point1.Y))
								route1points.Add((point1.X, point1.Y, point1.dist));
						}
						break;

					case 'U':
						for (int i = 0; i < s.Length; ++i)
						{
							point1.GoUp(1);
							if (!route1points.Exists(r => r.x == point1.X && r.y == point1.Y))
								route1points.Add((point1.X, point1.Y, point1.dist));
						}
						break;
					default:
						break;
				}
			}

			foreach (var s in wireSegments2)
			{
				switch (s.Direction)
				{
					case 'R':
						for (int i = 0; i < s.Length; ++i)
						{
							point2.GoRight(1);
							if (!route2points.Exists(r => r.x == point2.X && r.y == point2.Y))
								route2points.Add((point1.X, point2.Y, point2.dist));
						}
						break;

					case 'L':
						for (int i = 0; i < s.Length; ++i)
						{
							point2.GoLeft(1);
							if (!route2points.Exists(r => r.x == point2.X && r.y == point2.Y))
								route2points.Add((point1.X, point2.Y, point2.dist));
						}
						break;

					case 'D':
						for (int i = 0; i < s.Length; ++i)
						{
							point2.GoDown(1);
							if (!route2points.Exists(r => r.x == point2.X && r.y == point2.Y))
								route2points.Add((point1.X, point2.Y, point2.dist));
						}
						break;

					case 'U':
						for (int i = 0; i < s.Length; ++i)
						{
							point2.GoUp(1);
							if (!route2points.Exists(r => r.x == point2.X && r.y == point2.Y))
								route2points.Add((point1.X, point2.Y, point2.dist));
						}
						break;
					default:
						break;
				}
			}

			//intersections.AddRange(route1points.Intersect(route2points.Select(s => s.x 

			var firstIntersection = intersections.Min(c => c.dist);
			var itemMin = intersections.Where(x => x.dist == firstIntersection).FirstOrDefault();
			var route1WithFirstIntersection = route1points.Where(j => j.x == itemMin.x && j.y == itemMin.y).FirstOrDefault();

			Console.WriteLine($"Part 2: {itemMin.dist + route1WithFirstIntersection.dist}");

			Console.ReadKey();
		}

		public static int DistanceFromStart(int x, int y)
		{
			return Math.Abs(x - 0) + Math.Abs(y - 0);
		}
	}
}

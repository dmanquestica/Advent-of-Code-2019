using System;
using System.Collections.Generic;
using System.Linq;
using Shared_Utilities;

public class Point
{
	public int X;
	public int Y;
	public int Distance;

	public Point(int x, int y)
	{
		X = x;
		Y = y;
		Distance = 0;
	}

	public void GoRight(int x = 1)
	{
		X += x;
		++Distance;
	}
	public void GoLeft(int x = 1)
	{
		X -= x;
		++Distance;
	}

	public void GoUp(int y = 1)
	{
		Y += y;
		++Distance;
	}
	public void GoDown(int y = 1)
	{
		Y -= y;
		++Distance;
	}

	public Point Copy()
	{
		return new Point(X, Y)
		{
			Distance = Distance
		};
	}

	public override bool Equals(object obj)
	{
		return obj == null ? false : X == ((Point)obj).X && Y == ((Point)obj).Y;
	}

	public override int GetHashCode()
	{
		return (X + Y).GetHashCode();
	}
}

public class Program
{
	public static void Main(string[] args)
	{
		var input = Utilities.ReadFile(args[0]);

		var wire1 = input[0];
		var wire2 = input[1];

		var segments1 = wire1.Split(',').ToList();
		var segments2 = wire2.Split(',').ToList();

		var line1 = DrawLine(segments1);
		var line2 = DrawLine(segments2);

		var distanceTotal = int.MaxValue;

		var intersections = line1.Intersect(line2).ToList();

		// Check all intersections to find out the combined steps
		foreach (var p in intersections)
		{
			var dist1 = line1.IndexOf(p) + 1;
			var dist2 = line2.IndexOf(p) + 1;
			distanceTotal = Math.Min(dist1 + dist2, distanceTotal);
		}

		// Check all intersections to calculate Manhattan distance
		var distances = new List<int>();
		foreach (Point p in intersections)
		{
			distances.Add(CalculateManhattanDistance(0, p.X, 0, p.Y));
		}

		//Solution 1
		Console.WriteLine($"Part 1: {distances.Min().ToString()}");
		//Solution 2
		Console.WriteLine($"Part 2: {distanceTotal}");
		Console.ReadKey();
	}

	public static List<Point> DrawLine(List<string> path)
	{
		// Origin
		var lastPoint = new Point(0, 0);
		var line = new List<Point>();

		foreach (string instruction in path)
		{
			EvaluateInstruction(lastPoint, instruction, line);
			lastPoint = line.Last();
		}

		return line;
	}

	public static void EvaluateInstruction(Point point, string instruction, List<Point> line)
	{
		var operation = instruction.Substring(0, 1);
		var distance = int.Parse(instruction.Substring(1));

		switch (operation)
		{
			case "U":
				for (int i = 0; i < distance; i++)
				{
					point.GoUp();
					line.Add(point.Copy());
				}
				break;
			case "L":
				for (int i = 0; i < distance; i++)
				{
					point.GoLeft();
					line.Add(point.Copy());
				}
				break;
			case "R":
				for (int i = 0; i < distance; i++)
				{
					point.GoRight();
					line.Add(point.Copy());
				}
				break;
			case "D":
				for (int i = 0; i < distance; i++)
				{
					point.GoDown();
					line.Add(point.Copy());
				}
				break;
		}
	}

	public static int CalculateManhattanDistance(int x1, int x2, int y1, int y2)
	{
		return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
	}
}

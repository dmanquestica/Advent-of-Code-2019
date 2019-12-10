using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_10
{
	public class Asteroid
	{
		public int X;
		public int Y;
		// Part 1
		public List<Asteroid> Visible;
		// Part 2
		public double Angle;

		public Asteroid(int x, int y)
		{
			X = x;
			Y = y;
			Visible = new List<Asteroid>();
			Angle = 0;
		}

		public double AngleTo(Asteroid other)
		{
			if (X != other.X || Y != other.Y)
				return -Math.Atan2((X - other.X), (Y - other.Y));
			return 0;
		}
	}

	class Program
	{
		public static List<Asteroid> asteroidMap;

		static void Main(string[] args)
		{
			var input = Utilities.ReadFile(args[0]);

			asteroidMap = new List<Asteroid>();
			for (int j = 0; j < input.Count; ++j)
			{
				for (int i = 0; i < input[0].Length; ++i)
				{
					if (input[j][i] == '#')
						asteroidMap.Add(new Asteroid(i, j));
				}
			}

			// Part 1
			foreach (var a in asteroidMap)
				CountVisible(a);

			var baseLocation = asteroidMap.Where(a => a.Visible.Count() == asteroidMap.Max(b => b.Visible.Count())).FirstOrDefault();

			Console.WriteLine($"Part 1: {baseLocation.Visible.Count()}");

			// Part 2
			var vaporizationGoal = 200;
			// If the base can see less than the vaporizationGoal, remove all asteroid and recount the number of visible ones
			while (baseLocation.Visible.Count() < vaporizationGoal)
			{
				foreach (var a in baseLocation.Visible)
				{
					asteroidMap.Remove(a);
					vaporizationGoal--;
				}
				CountVisible(baseLocation);
			}

			// Calculate the angle of the visible asteroid to the baseLocation
			foreach (var a in baseLocation.Visible)
				a.Angle = a.AngleTo(baseLocation);
			
			// Sort asteroid by the angle
			baseLocation.Visible.Sort((a, b) => a.Angle.CompareTo(b.Angle));

			var goalAsteroid = baseLocation.Visible[vaporizationGoal - 1];

			Console.WriteLine($"Part 2: {goalAsteroid.X * 100 + goalAsteroid.Y}");
			Console.ReadKey();
		}

		private static bool SameAngle(Asteroid origin, Asteroid a, Asteroid b)
		{
			if (origin.AngleTo(a) == origin.AngleTo(b))
				return true;
			return false;
		}

		private static bool HasLineOfSight(Asteroid source, Asteroid destination)
		{
			int dx = Math.Abs(destination.X - source.X);
			int dy = Math.Abs(destination.Y - source.Y);
			// Pythagorean distance
			double candidateDistance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
			foreach (var a in asteroidMap)
			{
				if (a != source && a != destination)
				{
					int dxa = Math.Abs(a.X - source.X);
					int dya = Math.Abs(a.Y - source.Y);
					// Pythagorean distance
					double existingDistance = Math.Sqrt(Math.Pow(dxa, 2) + Math.Pow(dya, 2));
					if (existingDistance <= candidateDistance && SameAngle(source, destination, a))
						return false;
				}
			}
			return true;
		}

		// Part 1
		private static int CountVisible(Asteroid source)
		{
			foreach (var a in asteroidMap)
			{
				if (a != source && HasLineOfSight(source, a))
					source.Visible.Add(a);
			}
			return source.Visible.Count();
		}
	}
}

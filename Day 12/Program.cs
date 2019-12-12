using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day_12
{
	[Serializable]
	class 月
	{
		public int InitialX; // For debugging
		public int InitialY; // For debugging
		public int InitialZ; // For debugging
		public int X;
		public int Y;
		public int Z;
		public (int X, int Y, int Z) Velocity;
		public List<月> 姐妹;

		public 月(int x, int y, int z)
		{
			InitialX = x; // For debugging
			InitialY = y; // For debugging
			InitialZ = z; // For debugging
			X = x;
			Y = y;
			Z = z;
			Velocity = (0, 0, 0);
			姐妹 = new List<月>();
		}

		public (int X, int Y, int Z) GravityTo(月 m)
		{
			return (GravityTo(m, 0),
					GravityTo(m, 1),
					GravityTo(m, 2));
		}

		public int GravityTo(月 m, int axis)
		{
			int result = 0;
			switch (axis)
			{
				case 0:
					result = m.X - X == 0 ? 0 : m.X - X > 0 ? 1 : -1;
					break;
				case 1:
					result = m.Y - Y == 0 ? 0 : m.Y - Y > 0 ? 1 : -1;
					break;
				case 2:
					result = m.Z - Z == 0 ? 0 : m.Z - Z > 0 ? 1 : -1;
					break;
			}
			return result;
		}

		public void AdjustPosition(int axis)
		{
			switch (axis)
			{
				case 0:
					X += Velocity.X;
					break;
				case 1:
					Y += Velocity.Y;
					break;
				case 2:
					Z += Velocity.Z;
					break;
			}
		}

		public int PotentialEnergy()
		{
			return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
		}

		public int KineticEnergy()
		{
			return Math.Abs(Velocity.X) + Math.Abs(Velocity.Y) + Math.Abs(Velocity.Z);
		}
	}

	public class Program
	{
		static void Main(string[] args)
		{
			var input = Utilities.ReadFile(args[0]);

			var moons = Reset(input);

			// Part 1
			var steps = 1000;

			for (int i = 0; i < steps; ++i)
			{
				CalculateVelocity(moons);
				Step(moons);
			}
			Console.WriteLine($"Part 1: {moons.Sum(m => m.KineticEnergy() * m.PotentialEnergy())}");

			// Part 2
			// Compute each axes' cycle separately and calculate Lowest Common Factor amongst the steps required per axes' cycle
			moons = Reset(input);
			var states = new HashSet<(int, int, int, int, int, int, int, int)>();
			int xSteps = 0;
			while (true)
			{
				var curXState = (moons[0].X,		  moons[1].X,		   moons[2].X,			moons[3].X,             // Position
								 moons[0].Velocity.X, moons[1].Velocity.X, moons[2].Velocity.X, moons[3].Velocity.X);	// Velocity
				if (states.Contains(curXState))
					break;
				else
				{
					states.Add(curXState);
					CalculateVelocity(moons, 0);
					Step(moons, 0);
					xSteps++;
				}
			}

			moons = Reset(input);
			states = new HashSet<(int, int, int, int, int, int, int, int)>();
			int ySteps = 0;
			while (true)
			{
				var curYState = (moons[0].Y,		  moons[1].Y,		   moons[2].Y,			moons[3].Y,             // Position
								 moons[0].Velocity.Y, moons[1].Velocity.Y, moons[2].Velocity.Y, moons[3].Velocity.Y);	// Velocity
				if (states.Contains(curYState))
					break;
				else
				{
					states.Add(curYState);
					CalculateVelocity(moons, 1);
					Step(moons, 1);
					ySteps++;
				}
			}

			moons = Reset(input);
			states = new HashSet<(int, int, int, int, int, int, int, int)>();
			int zSteps = 0;
			while (true)
			{
				var curZState = (moons[0].Z,          moons[1].Z,          moons[2].Z,          moons[3].Z,				// Position
								 moons[0].Velocity.Z, moons[1].Velocity.Z, moons[2].Velocity.Z, moons[3].Velocity.Z);   // Velocity
				if (states.Contains(curZState))
					break;
				else
				{
					states.Add(curZState);
					CalculateVelocity(moons, 2);
					Step(moons, 2);
					zSteps++;
				}
			}

			Console.WriteLine($"Part 2: {LowestCommonMultiple(xSteps, LowestCommonMultiple(ySteps, zSteps))}");

			Console.ReadKey();
		}

		static List<月> Reset(IList<string> input)
		{
			var pattern = @"\<x=(-?[\d]{1,}), y=(-?[\d]{1,}), z=(-?[\d]{1,})\>";

			var moons = new List<月>();
			foreach (var s in input)
			{
				var match = Regex.Match(s, pattern);
				moons.Add(new 月(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value)));
			}

			foreach (var m in moons)
			{
				m.姐妹.AddRange(moons.Where(n => n != m));
			}

			return moons;
		}

		static void Step(List<月> moons)
		{
			Step(moons, 0);
			Step(moons, 1);
			Step(moons, 2);
		}

		static void Step(List<月> moons, int axis)
		{
			foreach (var m in moons)
			{
				m.AdjustPosition(axis);
			}
		}

		static void CalculateVelocity(List<月> moons)
		{
			CalculateVelocity(moons, 0);
			CalculateVelocity(moons, 1);
			CalculateVelocity(moons, 2);
		}

		static void CalculateVelocity(List<月> moons, int axis)
		{
			foreach (var m in moons)
			{
				var Xvelocity = m.Velocity.X;
				var Yvelocity = m.Velocity.Y;
				var Zvelocity = m.Velocity.Z;

				switch (axis)
				{
					case 0:
						Xvelocity += m.姐妹.Sum(n => m.GravityTo(n).X);
						break;
					case 1:
						Yvelocity += m.姐妹.Sum(n => m.GravityTo(n).Y);
						break;
					case 2:
						Zvelocity += m.姐妹.Sum(n => m.GravityTo(n).Z);
						break;

				}
				m.Velocity = (Xvelocity, Yvelocity, Zvelocity);
			}
		}

		static long LowestCommonMultiple(long a, long b)
		{
			return (a * b) / GreatestCommonDenominator(a, b);
		}

		static long GreatestCommonDenominator(long a, long b)
		{
			while (a != b) {
				if (a < b)
					b -= a;
				else
					a -= b;
			}
			return a;
		}
	}
}

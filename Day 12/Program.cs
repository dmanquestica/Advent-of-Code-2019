using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day_12
{
	[Serializable]
	class Moon
	{
		public int InitialX;
		public int InitialY;
		public int InitialZ;
		public int X;
		public int Y;
		public int Z;
		public (int x, int y, int z) Velocity;
		public List<Moon> Siblings;

		public Moon(int x, int y, int z)
		{
			InitialX = x;
			InitialY = y;
			InitialZ = z;
			X = x;
			Y = y;
			Z = z;
			Velocity = (0, 0, 0);
			Siblings = new List<Moon>();
		}

		public (int x, int y, int z) GravityTo(Moon m)
		{
			return (GravityTo(m, 0),
					GravityTo(m, 1),
					GravityTo(m, 2));
		}

		public int GravityTo(Moon m, int axis)
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
					X += Velocity.x;
					break;
				case 1:
					Y += Velocity.y;
					break;
				case 2:
					Z += Velocity.z;
					break;
			}
		}

		public int PotentialEnergy()
		{
			return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
		}

		public int KineticEnergy()
		{
			return Math.Abs(Velocity.x) + Math.Abs(Velocity.y) + Math.Abs(Velocity.z);
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
				AdjustPositions(moons);
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
								 moons[0].Velocity.x, moons[1].Velocity.x, moons[2].Velocity.x, moons[3].Velocity.x);	// Velocity
				if (states.Contains(curXState))
					break;
				else
				{
					states.Add(curXState);
					CalculateVelocity(moons, 0);
					AdjustPositions(moons, 0);
					xSteps++;
				}
			}

			moons = Reset(input);
			states = new HashSet<(int, int, int, int, int, int, int, int)>();
			int ySteps = 0;
			while (true)
			{
				var curYState = (moons[0].Y,		  moons[1].Y,		   moons[2].Y,			moons[3].Y,             // Position
								 moons[0].Velocity.y, moons[1].Velocity.y, moons[2].Velocity.y, moons[3].Velocity.y);	// Velocity
				if (states.Contains(curYState))
					break;
				else
				{
					states.Add(curYState);
					CalculateVelocity(moons, 1);
					AdjustPositions(moons, 1);
					ySteps++;
				}
			}

			moons = Reset(input);
			states = new HashSet<(int, int, int, int, int, int, int, int)>();
			int zSteps = 0;
			while (true)
			{
				var curZState = (moons[0].Z,          moons[1].Z,          moons[2].Z,          moons[3].Z,				// Position
								 moons[0].Velocity.z, moons[1].Velocity.z, moons[2].Velocity.z, moons[3].Velocity.z);   // Velocity
				if (states.Contains(curZState))
					break;
				else
				{
					states.Add(curZState);
					CalculateVelocity(moons, 2);
					AdjustPositions(moons, 2);
					zSteps++;
				}
			}

			Console.WriteLine($"Part 2: {LowestCommonFactor(xSteps, LowestCommonFactor(ySteps, zSteps))}");

			Console.ReadKey();
		}

		static List<Moon> Reset(IList<string> input)
		{
			var pattern = @"\<x=(-?[\d]{1,}), y=(-?[\d]{1,}), z=(-?[\d]{1,})\>";

			var moons = new List<Moon>();
			foreach (var s in input)
			{
				var match = Regex.Match(s, pattern);
				moons.Add(new Moon(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value)));
			}

			foreach (var m in moons)
			{
				m.Siblings.AddRange(moons.Where(n => n != m));
			}

			return moons;
		}

		static void AdjustPositions(List<Moon> moons)
		{
			AdjustPositions(moons, 0);
			AdjustPositions(moons, 1);
			AdjustPositions(moons, 2);
		}

		static void AdjustPositions(List<Moon> moons, int axis)
		{
			foreach (var m in moons)
			{
				m.AdjustPosition(axis);
			}
		}

		static void CalculateVelocity(List<Moon> moons)
		{
			CalculateVelocity(moons, 0);
			CalculateVelocity(moons, 1);
			CalculateVelocity(moons, 2);
		}

		static void CalculateVelocity(List<Moon> moons, int axis)
		{
			foreach (var m in moons)
			{
				var Xvelocity = m.Velocity.x;
				var Yvelocity = m.Velocity.y;
				var Zvelocity = m.Velocity.z;

				switch (axis)
				{
					case 0:
						Xvelocity += m.Siblings.Sum(n => m.GravityTo(n).x);
						break;
					case 1:
						Yvelocity += m.Siblings.Sum(n => m.GravityTo(n).y);
						break;
					case 2:
						Zvelocity += m.Siblings.Sum(n => m.GravityTo(n).z);
						break;

				}
				m.Velocity = (Xvelocity, Yvelocity, Zvelocity);
			}
		}

		static long LowestCommonFactor(long a, long b)
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

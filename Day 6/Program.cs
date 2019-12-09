using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_6
{
	public class OrbitingBody
	{
		public string Name { get; set; }
		public OrbitingBody Parent { get; set; }
		public List<OrbitingBody> Orbiting { get; set; }
		public int Distance { get; set; }

		public OrbitingBody(string name)
		{
			Name = name;
			Parent = null;
			Orbiting = new List<OrbitingBody>();
			Distance = 0;
		}

		public void AddOrbitingPlanet(OrbitingBody newPlanet)
		{
			newPlanet.Parent = this;
			newPlanet.Distance = Distance + 1;
			foreach (var planet in newPlanet.Orbiting)
				planet.IncreaseDistance(newPlanet.Distance + 1);
			Orbiting.Add(newPlanet);
		}

		public void IncreaseDistance(int newDistance)
		{
			Distance = newDistance;
			foreach (var planet in Orbiting)
				planet.IncreaseDistance(Distance + 1);
		}
	}

	public class Program
	{
		static void Main(string[] args)
		{
			var input = Utilities.ReadFile(args[0]);

			var orbitingBodyList = new List<OrbitingBody>();

			foreach (var s in input)
			{
				var relation = s.Split(')');
				if (!orbitingBodyList.Exists(p => p.Name == relation[0]))
					orbitingBodyList.Add(new OrbitingBody(relation[0]));
				if (!orbitingBodyList.Exists(p => p.Name == relation[1]))
					orbitingBodyList.Add(new OrbitingBody(relation[1]));
			}

			foreach (var s in input)
			{
				var planetRelative = s.Split(')');
				orbitingBodyList.Where(p => p.Name == planetRelative[0]).FirstOrDefault().AddOrbitingPlanet(orbitingBodyList.Where(p => p.Name == planetRelative[1]).FirstOrDefault());
			}

			Console.WriteLine($"Part 1: {orbitingBodyList.Sum(p => p.Distance)}");

			// Find YOU
			var you = orbitingBodyList.Where(p => p.Name == "YOU").FirstOrDefault();
			// Santa
			var santa = orbitingBodyList.Where(p => p.Name == "SAN").FirstOrDefault();

			var yourPath = new List<OrbitingBody>();
			var santasPath = new List<OrbitingBody>();

			// Path to the COM
			while (you.Parent != null)
			{
				you = you.Parent;
				yourPath.Add(you);
			}

			// Path to the COM
			while (santa.Parent != null)
			{
				santa = santa.Parent;
				santasPath.Add(santa);
			}

			// Check for common orbiting body on way to COM 
			for (var i = 0; i < yourPath.Count(); ++i)
			{
				if (santasPath.Any(p => p == yourPath[i])) {
					Console.WriteLine($"Part 2: {i + santasPath.IndexOf(yourPath[i])}");
					break;
				}
			}

			Console.ReadKey();
		}
	}
}

using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Day_20
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var input = Utilities.ReadFile(args[0]);
			Part1(input);
			Part2(input);
			Console.ReadKey();
		}

		public static void Part1(IList<string> input)
		{
			var maze = ReadInput(input);
			var (portals, start, end) = ConnectPortals(maze);

			var positions = new Queue<(Coords, int)>();
			var explored = new HashSet<Coords>();

			positions.Enqueue((start, 0));
			var shortest = 0;

			while (positions.Any())
			{
				var (position, distance) = positions.Dequeue();

				if (position.Equals(end))
				{
					shortest = distance;
					break;
				}

				if (explored.Contains(position))
				{
					continue;
				}

				explored.Add(position);

				var reachableNormally =
					position.Around().Where(p => maze.TryGetValue(p, out var t) && t.Type == TileType.Open);

				foreach (var p in reachableNormally)
				{
					positions.Enqueue((p, distance + 1));
				}

				// portals
				if (portals.TryGetValue(position, out var portalTarget))
				{
					positions.Enqueue((portalTarget, distance + 1));
				}
			}
			Console.WriteLine($"Part 1: {shortest}");
		}

		public static void Part2(IList<string> input)
		{
			var maze = ReadInput(input);
			var (portals, start, end) = ConnectPortals(maze);

			var positions = new Queue<(Coords p, int lvl, int dist)>();
			var explored = new HashSet<(Coords, int)>();

			positions.Enqueue((start, 0, 0));
			var shortest = 0;

			const int minX = 2;
			const int minY = 2;
			var maxX = portals.Keys.Concat(portals.Values).Select(x => x.X).Max();
			var maxY = portals.Keys.Concat(portals.Values).Select(y => y.Y).Max();

			while (positions.Any())
			{
				var (position, level, distance) = positions.Dequeue();

				if (position.Equals(end) && level == 0)
				{
					shortest = distance;
					break;
				}

				if (explored.Contains((position, level)))
				{
					continue;
				}
				else
				{
					explored.Add((position, level));
				}

				var reachableNormally = position.Around().Where(p => maze.TryGetValue(p, out var t) && t.Type == TileType.Open);

				foreach (var p in reachableNormally)
				{
					positions.Enqueue((p, level, distance + 1));
				}

				// portals
				if (portals.TryGetValue(position, out var portalTarget))
				{
					if (position.X == minX || position.X == maxX || position.Y == minY || position.Y == maxY)
					{
						// outer portal
						if (level > 0)
						{
							positions.Enqueue((portalTarget, level - 1, distance + 1));
						}
					}
					else
					{
						// inner portal
						positions.Enqueue((portalTarget, level + 1, distance + 1));
					}
				}
			}
			Console.WriteLine($"Part 1: {shortest}");
		}

		private static (Dictionary<Coords, Coords> Portals, Coords Start, Coords End) ConnectPortals(Dictionary<Coords, Tile> maze)
		{
			var list = new Dictionary<string, HashSet<Coords>>();

			foreach (var (p, tile) in maze.Where(q => q.Value.Type == TileType.Portal).Select(q => (q.Key, q.Value)))
			{
				if (!p.Around().Any(q => maze.TryGetValue(q, out var t) && t.Content == '.'))
				{
					continue;
				};

				var otherTile = p.Around().Single(q => maze.TryGetValue(q, out var t) && t.Type == TileType.Portal);

				var name = new string(new[] { tile.Content, maze[otherTile].Content }.OrderBy(q => q).ToArray());

				if (list.TryGetValue(name, out var positions))
				{
					positions.Add(p);
				}
				else
				{
					list[name] = new HashSet<Coords> { p };
				}
			}

			var result = new Dictionary<Coords, Coords>();

			if (list.Any(p => p.Value.Count > 2))
			{
				// I was lucky, there were no portal pairs called AB and BA, that would throw my portal connection off
				throw new Exception("Duplicate portal names detected");
			}

			foreach (var value in list.Where(p => p.Value.Count == 2).Select(p => p.Value))
			{
				var openTile1 = value.First().Around()
					.Single(p => maze.TryGetValue(p, out var t) && t.Content == '.');
				var openTile2 = value.Last().Around()
					.Single(p => maze.TryGetValue(p, out var t) && t.Content == '.');

				result.Add(openTile1, openTile2);
				result.Add(openTile2, openTile1);
			}

			var startTile = list["AA"].Single().Around().Single(p => maze.TryGetValue(p, out var t) && t.Content == '.');
			var endTile = list["ZZ"].Single().Around().Single(p => maze.TryGetValue(p, out var t) && t.Content == '.');

			return (result, startTile, endTile);
		}

		private static Dictionary<Coords, Tile> ReadInput(IList<string> lines)
		{
			var dictionary = new Dictionary<Coords, Tile>();

			for (var row = 0; row < lines.Count; row++)
			{
				for (var col = 0; col < lines[0].Length; col++)
				{
					var p = new Coords { X = col, Y = row };

					var tile = lines[row][col];

					if (tile == ' ')
					{
						continue;
					}

					var tileType = TileType.Blank;
					if (tile == '#')
						tileType = TileType.Wall;
					else if (tile == '.')
						tileType = TileType.Open;
					else if ('A' <= tile && tile <= 'Z')
						tileType = TileType.Portal;

					dictionary.Add(p, new Tile()
					{
						Content = tile,
						Type = tileType
					});
				}
			}

			return dictionary;
		}
	}

	public struct Tile
	{
		public char Content { get; set; }

		public TileType Type { get; set; }
	}

	public struct Coords : IEquatable<Coords>
	{
		public int X { get; set; }

		public int Y { get; set; }

		public bool Equals(Coords other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Coords)obj);
		}

		public override int GetHashCode()
		{
			return new { X, Y }.GetHashCode();
		}

		public IEnumerable<Coords> Around()
		{
			yield return new Coords { X = X, Y = Y - 1 };
			yield return new Coords { X = X - 1, Y = Y };
			yield return new Coords { X = X + 1, Y = Y };
			yield return new Coords { X = X, Y = Y + 1 };
		}
	}


	public enum TileType
	{
		Open,
		Wall,
		Portal,
		Blank
	}
}
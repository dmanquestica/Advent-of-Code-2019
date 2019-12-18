using iText.IO.Util;
using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Day_18
{
    class Program
    {
        public static void Main(string[] args)
        {
            var d = new Day18(args);

            d.Part1();
            d.Part2();
            Console.ReadKey();
        }
    }

    public class Day18
    {
        public string Part1File;
        public string Part2File;

        public Day18(string[] args)
        {
            Part1File = args[0];
            Part2File = args[1];
        }

        public void Part1()
        {
            var map = Utilities.ReadFile(Part1File).ToArray();

            var keys = map.SelectMany(_ => _.Where(char.IsLower)).ToList();

            var dictionary = new Dictionary<Coords, List<ReachableKey>>
            {
                [PositionOf('@', map)] = ReachableKeys(map, PositionOf('@', map))
            };

            foreach (var k in keys)
                dictionary[PositionOf(k, map)] = ReachableKeys(map, PositionOf(k, map));

            var minimumSteps = GetKeys(map, dictionary, GetPositions(map, keys), new[] { '@' });
            Console.WriteLine(minimumSteps);

        }
        public void Part2()
        {
            var lines = Utilities.ReadFile(Part2File).ToArray();

            var keys = lines.SelectMany(_ => _.Where(char.IsLower)).ToList();

            var dictionary = new Dictionary<Coords, List<ReachableKey>>();
            
            // Cheating a bit here. Labeled robots instead using generic @.
            for (var i = '1'; i <= '4'; i++)
                dictionary[PositionOf(i, lines)] = ReachableKeys(lines, PositionOf(i, lines));

            foreach (var k in keys)
                dictionary[PositionOf(k, lines)] = ReachableKeys(lines, PositionOf(k, lines));

            var minimumSteps = GetKeys(lines, dictionary, GetPositions(lines, keys), new[] { '1', '2', '3', '4' });
            Console.WriteLine(minimumSteps);

        }

        private Dictionary<char, Coords> GetPositions(string[] map, List<char> keys)
        {
            var dict = new Dictionary<char, Coords>();

            foreach (var k in keys)
                dict.Add(k, PositionOf(k, map));

            return dict;
        }

        private int GetKeys(string[] map, Dictionary<Coords, List<ReachableKey>> keyPaths, Dictionary<char, Coords> positions, char[] robots)
        {
            var pos = robots.Select(c => PositionOf(c, map)).ToArray();
            var currentMinimum = int.MaxValue;

            var startingSet = new CoordsSet();
            for (var index = 0; index < pos.Length; index++)
            {
                var p = pos[index];
                startingSet[index + 1] = p;
            }

            var q = new Queue<RobotState>();
            q.Enqueue(new RobotState { Positions = startingSet, OwnedKeys = 0 });

            var visited = new Dictionary<(CoordsSet, int), int>();
            var finishValue = 0;
            for (var i = 0; i < positions.Count; ++i)
                finishValue |= (int)Math.Pow(2, i);

            while (q.Any())
            {
                var state = q.Dequeue();

                var valueTuple = (state.Positions, state.OwnedKeys);
                if (visited.TryGetValue(valueTuple, out var steps))
                {
                    if (steps <= state.Steps)
                        continue;

                    // this is the crucial bit
                    // if the current state is a better path to a known state, update -
                    // this will cull more future paths, leading to faster convergence
                    visited[valueTuple] = state.Steps;
                }
                else
                    visited.Add(valueTuple, state.Steps);

                if (state.OwnedKeys == finishValue)
                {
                    currentMinimum = Math.Min(currentMinimum, state.Steps);
                    continue;
                }

                for (int i = 1; i <= robots.Length; i++)
                {
                    foreach (var k in keyPaths[state.Positions[i]])
                    {
                        var ki = (int)Math.Pow(2, k.Key - 'a');
                        if ((state.OwnedKeys & ki) == ki || (k.Obstacles & state.OwnedKeys) != k.Obstacles)
                            continue;

                        var newOwned = state.OwnedKeys | ki;

                        var newPos = state.Positions.Clone();
                        newPos[i] = positions[k.Key];
                        q.Enqueue(new RobotState
                        {
                            Positions = newPos,
                            OwnedKeys = newOwned,
                            Steps = state.Steps + k.Distance
                        });
                    }
                }
            }

            return currentMinimum;
        }

        private struct RobotState
        {
            public CoordsSet Positions { get; set; }

            public int OwnedKeys { get; set; }

            public int Steps { get; set; }
        }

        private Coords PositionOf(char c, string[] map)
        {
            var startingLine = map.Single(_ => _.Contains(c));
            var startingColumn = startingLine.IndexOf(c);
            var startingRow = Array.IndexOf(map, startingLine);

            return new Coords { X = startingColumn, Y = startingRow };
        }

        // Normal BFS algorithm
        private List<ReachableKey> ReachableKeys(string[] map, Coords start)
        {
            var list = new List<ReachableKey>();
            var visited = new HashSet<Coords>();

            var nodes = new Queue<Coords>();
            var distances = new Queue<int>();
            var obstacles = new Queue<int>();
            nodes.Enqueue(start);
            distances.Enqueue(0);
            obstacles.Enqueue(0);

            while (nodes.Any())
            {
                var pos = nodes.Dequeue();
                var dist = distances.Dequeue();
                var obst = obstacles.Dequeue();

                if (visited.Contains(pos))
                {
                    continue;
                }

                visited.Add(pos);

                var c = map[pos.Y][pos.X];

                if (c == '@' || c == '1' || c == '2' || c == '3' || c == '4')
                {
                    c = '.';
                }

                if (char.IsLower(c))
                {
                    list.Add(new ReachableKey { Distance = dist, Key = c, Obstacles = obst });

                    foreach (var p in pos.Around())
                    {
                        nodes.Enqueue(p);
                        distances.Enqueue(dist + 1);
                        obstacles.Enqueue(obst);
                    }
                }
                else if (char.IsUpper(c))
                {
                    foreach (var p in pos.Around())
                    {
                        nodes.Enqueue(p);
                        distances.Enqueue(dist + 1);
                        obstacles.Enqueue(obst |= (int)Math.Pow(2, (char.ToLower(c) - 'a')));
                    }
                }
                else if (c == '.')
                {
                    foreach (var p in pos.Around())
                    {
                        nodes.Enqueue(p);
                        distances.Enqueue(dist + 1);
                        obstacles.Enqueue(obst);
                    }
                }
            }

            return list;
        }

        private class ReachableKey
        {
            public char Key { get; set; }

            public int Distance { get; set; }

            public int Obstacles { get; set; }
        }

        // For Part 2
        private struct CoordsSet : IEquatable<CoordsSet>
        {
            public Coords C1 { get; set; }

            public Coords C2 { get; set; }

            public Coords C3 { get; set; }

            public Coords C4 { get; set; }

            public Coords this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 2:
                            return C2;
                        case 3:
                            return C3;
                        case 4:
                            return C4;
                        default:
                            return C1;
                    }
                }
                set
                {
                    switch (index)
                    {
                        case 1:
                            C1 = value;
                            break;
                        case 2:
                            C2 = value;
                            break;
                        case 3:
                            C3 = value;
                            break;
                        case 4:
                            C4 = value;
                            break;
                    }
                }
            }

            public CoordsSet Clone()
            {
                return new CoordsSet
                {
                    C1 = C1,
                    C2 = C2,
                    C3 = C3,
                    C4 = C4
                };
            }

            public bool Equals(CoordsSet other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(C1, other.C1) && Equals(C2, other.C2) && Equals(C3, other.C3) && Equals(C4, other.C4);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((CoordsSet)obj);
            }

            public override int GetHashCode()
            {
                return new { C1, C2, C3, C4 }.GetHashCode();
            }
        }

        private struct Coords : IEquatable<Coords>
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
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Coords)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y);
            }

            public IEnumerable<Coords> Around()
            {
                yield return new Coords { X = X, Y = Y - 1 };
                yield return new Coords { X = X - 1, Y = Y };
                yield return new Coords { X = X + 1, Y = Y };
                yield return new Coords { X = X, Y = Y + 1 };
            }
        }
    }
}
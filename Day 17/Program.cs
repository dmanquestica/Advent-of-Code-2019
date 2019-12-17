using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_17
{
	class Coords
	{
		public Int64 X;
		public Int64 Y;
		public int Distance;
		public Tile Tile;

		public Coords(Int64 x, Int64 y, int distance, Tile tile)
		{
			X = x;
			Y = y;
			Distance = distance;
			Tile = tile;
		}
	}

	public enum ParameterMode
	{
		Positional = 0,
		Immediate = 1,
		Relative = 2
	}

	public enum Tile
	{
		Scaffold = 35,
		Empty = 46,
		RobotUp = 94,
		RobotDown = 86,
		RobotLeft = 60,
		RobotRight = 62
	}

	class Intcode
	{
		private readonly Queue<Int64> InputQueue = new Queue<Int64>();
		private readonly Queue<Int64> OutputQueue = new Queue<Int64>();

		public Intcode OutputMachine = null;

		public Int64[] Memory { get; set; }
		private Int64 InstructionPointer = 0;
		private Int64 RelativeBase = 0;

		public bool Waiting = false;
		public bool Halted = false;
		public bool SentOutput = false;

		Int64 OpCode;

		public Intcode(Int64[] initialMemory)
		{
			Memory = new Int64[initialMemory.Count() + 10000];
			initialMemory.CopyTo(Memory, 0);
		}

		public void SendInput(Int64 input)
		{
			InputQueue.Enqueue(input);
			Waiting = false;
		}

		public Queue<Int64> GetOutputQueue()
		{
			return OutputQueue;
		}

		public Int64 GetOutput()
		{
			return OutputQueue.Dequeue();
		}

		private void Output(Int64 i)
		{
			OutputQueue.Enqueue(i);
			if (OutputMachine != null)
			{
				OutputMachine.SendInput(OutputQueue.Dequeue());
			}
			SentOutput = true;
		}

		private Int64 GetMem(Int64 address)
		{
			return Memory[address];
		}

		private void SetMem(Int64 address, Int64 value)
		{
			Memory[address] = value;
		}

		private ParameterMode AccessMode(int idx)
		{
			int mode = (int)GetMem(InstructionPointer) / 100;
			for (int i = 1; i < idx; i++)
				mode /= 10;
			return (ParameterMode)(mode % 10);
		}

		private void SetParam(int idx, Int64 value)
		{
			Int64 param = GetMem(InstructionPointer + idx);
			switch (AccessMode(idx))
			{
				case ParameterMode.Positional: // position mode
					SetMem(param, value);
					break;
				case ParameterMode.Immediate: // immediate mode -- should never occur
					throw new Exception("Intcode immediate mode not allowed in setting memory");
				case ParameterMode.Relative: // relative mode
					SetMem(RelativeBase + param, value);
					break;
				default:
					throw new Exception("Invalid Intcode parameter mode");
			}
		}

		private Int64 GetParam(int idx)
		{
			Int64 param = GetMem(InstructionPointer + idx);
			switch (AccessMode(idx))
			{
				case ParameterMode.Positional:
					return GetMem(param);
				case ParameterMode.Immediate: // immediate mode
					return param;
				case ParameterMode.Relative: // relative mode
					return GetMem(RelativeBase + param);
				default:
					throw new Exception("Invalid Intcode parameter mode");
			}
		}

		private void ParseOpCode(Int64 opCode)
		{
			OpCode = (opCode % 100);
		}

		public void Step()
		{
			ParseOpCode(GetMem(InstructionPointer));
			switch (OpCode)
			{
				case 1:
					Addition();
					break;
				case 2:
					Multiply();
					break;
				case 3:
					GetInput();
					break;
				case 4:
					SetOutput();
					break;
				case 5:
					JumpIfTrue();
					break;
				case 6:
					JumpIfFalse();
					break;
				case 7:
					LessThan();
					break;
				case 8:
					IsEquals();
					break;
				case 9:
					AdjustRelativeBase();
					break;
				case 99:
					Halted = true;
					break;
				default:
					throw new System.Exception("Unknown Int Code opcode " + OpCode + " at position " + InstructionPointer);
			}
		}

		#region OpCode Operations
		public void Addition()
		{
			SetParam(3, GetParam(1) + GetParam(2));
			InstructionPointer += 4;
		}

		public void Multiply()
		{
			SetParam(3, GetParam(1) * GetParam(2));
			InstructionPointer += 4;
		}

		public void GetInput()
		{
			if (InputQueue.Count > 0)
			{
				SetParam(1, InputQueue.Dequeue());
				InstructionPointer += 2;
			}
			else
			{
				Waiting = true;
			}
		}

		public void SetOutput()
		{
			Output(GetParam(1));
			InstructionPointer += 2;
		}

		public void JumpIfTrue()
		{
			if (GetParam(1) == 0)
				InstructionPointer += 3;
			else
				InstructionPointer = GetParam(2);
		}

		public void JumpIfFalse()
		{
			if (GetParam(1) == 0)
				InstructionPointer = GetParam(2);
			else
				InstructionPointer += 3;
		}

		public void LessThan()
		{
			if (GetParam(1) < GetParam(2))
				SetParam(3, (Int64)1);
			else
				SetParam(3, (Int64)0);
			InstructionPointer += 4;
		}

		public void IsEquals()
		{
			if (GetParam(1) == GetParam(2))
				SetParam(3, (Int64)1);
			else
				SetParam(3, (Int64)0);
			InstructionPointer += 4;
		}

		public void AdjustRelativeBase()
		{
			RelativeBase += GetParam(1);
			InstructionPointer += 2;
		}
		#endregion

		public void RunToOutput()
		{
			SentOutput = false;
			while (!Halted && !Waiting && !SentOutput)
				Step();
		}

		public void Run()
		{
			while (!Halted && !Waiting)
				Step();
			if (Waiting)
			{
			}
		}
	}

	class ASCIIRobot
	{
		public Int64[] Program;
		public Intcode Brain;
		public Coords Coordinates;
		public Tile Direction;

		public ASCIIRobot(Int64[] input, Coords coords = null)
		{
			Program = new Int64[(Int64)input.Length];
			input.CopyTo(Program, 0);
			Coordinates = coords;
			if (coords == null)
				Brain = new Intcode(Program);
		}

		public ASCIIRobot Move(Tile direction, HashSet<(Int64, Int64)> explored)
		{
			Brain = new Intcode(Program);
			Direction = direction;

			var nextCoord = new Coords(Coordinates.X, Coordinates.Y, Coordinates.Distance + 1, direction);

			switch (direction)
			{
				case Tile.RobotUp:
					nextCoord.Y++;
					direction = Tile.RobotUp;
					break;
				case Tile.RobotDown:
					nextCoord.Y--;
					direction = Tile.RobotDown;
					break;
				case Tile.RobotRight:
					nextCoord.X++;
					direction = Tile.RobotRight;
					break;
				case Tile.RobotLeft:
					nextCoord.X--;
					direction = Tile.RobotLeft;
					break;
			}
			if (explored.Contains((nextCoord.X, nextCoord.Y)))
				return null;

			explored.Add((nextCoord.X, nextCoord.Y));

			Brain.SendInput((long)direction);
			Brain.Run();
			var output = Brain.GetOutput();

			if ((Tile)output == Tile.Empty)
				return null;
			if ((Tile)output == Tile.Scaffold)
				return new ASCIIRobot(Brain.Memory, nextCoord);
			else throw new Exception("Unknown result");
		}
	}


	class Program
	{
		public static List<Coords> Map = new List<Coords>();

		static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0]).Split(',').Select(Int64.Parse).ToArray();

			Int64[] realInput = new Int64[input.Length];
			input.CopyTo(realInput, 0);

			// Part 1
			Part1(realInput);

			// Part 2
			realInput = new long[input.Length];
			input.CopyTo(realInput, 0);
			Part2(realInput);

			Console.ReadKey();
		}

		static void Part1(Int64[] input)
		{
			var bot = new ASCIIRobot(input, null);
			bot.Brain.Run();
			var output = bot.Brain.GetOutputQueue();

			var map = new List<Int64>();
			while (output.Any())
			{
				map.Add(output.Dequeue());
			}
			Display(map);

			var mapCoords = new List<Coords>();

			long x = 0;
			long y = 0;

			foreach (var l in map)
			{
				if (l == 10)
				{
					++y;
					x = 0;
					continue;
				}
				else
				{
					Map.Add(new Coords(x++, y, 0, (Tile)l));
				}
			}

			long maxX = Map.Max(c => c.X);
			long maxY = Map.Max(c => c.Y);

			int sum = 0;

			for (int j = 1; j < maxY - 1; ++j)
			{
				for (int i = 1; i < maxX - 1; ++i)
				{
					var current = Map.Where(c => c.X == i && c.Y == j).First();
					if (current.Tile == Tile.Scaffold)
					{
						if (Map.Exists(c => (current.X == c.X - 1 && current.Y == c.Y && c.Tile == Tile.Scaffold))
							&& Map.Exists(c => (current.X == c.X + 1 && current.Y == c.Y && c.Tile == Tile.Scaffold))
							&& Map.Exists(c => (current.X == c.X && current.Y == c.Y - 1 && c.Tile == Tile.Scaffold))
							&& Map.Exists(c => (current.X == c.X && current.Y == c.Y + 1 && c.Tile == Tile.Scaffold)))
							sum += i * j;
					}
				}
			}
			Console.WriteLine($"Part 1: {sum}");
		}

		static void Part2(Int64[] input)
		{
			var M = new Int64[] { 'A', ',', 'B', ',', 'B', ',', 'A', ',', 'B', ',', 'C', ',', 'A', ',', 'C', ',', 'B', ',', 'C', '\n' };
			var A = new Int64[] { 'L', ',', '4', ',', 'L', ',', '6', ',', 'L', ',', '8', ',', 'L', ',', '1', '2', '\n' };
			var B = new Int64[] { 'L', ',', '8', ',', 'R', ',', '1', '2', ',', 'L', ',', '1', '2', '\n' };
			var C = new Int64[] { 'R', ',', '1', '2', ',', 'L', ',', '6', ',', 'L', ',', '6', ',', 'L', ',', '8', '\n' };
			var V = new Int64[] { 'n', 10 };

			input[0] = 2;

			var bot = new ASCIIRobot(input, null);

			bot.Brain.Run();

			var output = bot.Brain.GetOutputQueue();

			var map = new List<Int64>();
			while (output.Any())
			{
				map.Add(output.Dequeue());
			}
			Display(map);

			foreach (var c in M)
			{
				bot.Brain.SendInput(c);
				Console.Write((char)c);
			}

			bot.Brain.Run();

			map = new List<Int64>();
			while (output.Any())
			{
				map.Add(output.Dequeue());
			}
			Display(map);

			foreach (var c in A)
			{
				bot.Brain.SendInput(c);
				Console.Write((char)c);
			}

			bot.Brain.Run();

			map = new List<Int64>();
			while (output.Any())
			{
				map.Add(output.Dequeue());
			}
			Display(map);

			foreach (var c in B)
			{
				bot.Brain.SendInput(c);
				Console.Write((char)c);
			}

			bot.Brain.Run();

			map = new List<Int64>();
			while (output.Any())
			{
				map.Add(output.Dequeue());
			}
			Display(map);

			foreach (var c in C)
			{
				bot.Brain.SendInput(c);
				Console.Write((char)c);
			}

			bot.Brain.Run();

			map = new List<Int64>();
			while (output.Any())
			{
				map.Add(output.Dequeue());
			}
			Display(map);

			foreach (var c in V)
			{
				bot.Brain.SendInput(c);
				Console.Write((char)c);
			}

			bot.Brain.Run();

			map = new List<Int64>();
			while (output.Any())
			{
				map.Add(output.Dequeue());
			}
			Display(map);

			bot.Brain.Run();

			if (bot.Brain.GetOutputQueue().Any())
			{
				Console.WriteLine($"Part 2: {bot.Brain.GetOutputQueue().Dequeue()}");
			}
		}

		static void Display(List<Int64> map)
		{
			foreach (var l in map)
			{
				if (l == 10)
					Console.WriteLine();
				else
					Console.Write((char)l);
			}
		}
	}
}

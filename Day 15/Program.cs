using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_15
{
	class Coords
	{
		public Int64 X;
		public Int64 Y;
		public int Distance;

		public Coords(Int64 x, Int64 y, int distance)
		{
			X = x;
			Y = y;
			Distance = distance;
		}
	}

	public enum ParameterMode
	{
		Positional = 0,
		Immediate = 1,
		Relative = 2
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
			Memory = new Int64[initialMemory.Count()];
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

	class Drone
	{
		public Int64[] Program;
		public Intcode Brain;
		public Coords Coordinates;
		public bool AtTarget;

		public Drone(Int64[] input, Coords coords, bool atTarget = false)
		{
			Program = new Int64[(Int64)input.Length];
			input.CopyTo(Program, 0);
			AtTarget = atTarget;
			Coordinates = coords;
		}

		public Drone Move(int direction, HashSet<(Int64, Int64)> explored)
		{
			Brain = new Intcode(Program);

			var nextCoord = new Coords(Coordinates.X, Coordinates.Y, Coordinates.Distance + 1);

			switch (direction)
			{
				case 1:
					nextCoord.Y++;
					break;
				case 2:
					nextCoord.Y--;
					break;
				case 3:
					nextCoord.X++;
					break;
				case 4:
					nextCoord.X--;
					break;
			}
			if (explored.Contains((nextCoord.X, nextCoord.Y)))
				return null;

			explored.Add((nextCoord.X, nextCoord.Y));

			Brain.SendInput(direction);
			Brain.Run();
			var output = Brain.GetOutput();

			if (output == 0) 
				return null;
			if (output == 2) 
				return new Drone(Brain.Memory, nextCoord, true);
			if (output == 1) 
				return new Drone(Brain.Memory, nextCoord);
			else throw new Exception("Unknown result");
		}
	}

	class Program
	{
		public static HashSet<(Int64, Int64)> Explored;
		public static Queue<Drone> Positions;
		public static Drone onTarget;

		static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0]).Split(',').Select(Int64.Parse).ToArray();

			Console.WriteLine($"Part 1: {Part1(input)}");
			Console.WriteLine($"Part 2: {Part2(input)}");
			Console.ReadKey();
		}

		// BFS to Oxgen System
		public static int Part1(Int64[] input)
		{
			Explored = new HashSet<(Int64, Int64)>();
			Positions = new Queue<Drone>();

			var drone = new Drone(input, new Coords(0,0,0));
			Positions.Enqueue(drone);

			while (Positions.Count > 0)
			{
				var head = Positions.Dequeue();

				for (int n = 1; n <= 4; ++n)
				{
					var newDrone = head.Move(n, Explored);
					if (newDrone == null)
						continue;
					if (newDrone.AtTarget)
					{
						onTarget = newDrone;
						return newDrone.Coordinates.Distance;
					}
					Positions.Enqueue(newDrone);
				}
			}
			return -1;
		}

		public static int Part2(Int64[] input)
		{
			if (onTarget == null)
				Part1(input);
			return FillSpace(onTarget);
		}
		
		// BFS from Oxygen System
		public static int FillSpace(Drone drone)
		{
			Explored = new HashSet<(Int64, Int64)>();
			drone.Coordinates = new Coords(drone.Coordinates.X, drone.Coordinates.Y, 0);

			Positions = new Queue<Drone>();
			Positions.Enqueue(drone);

			Drone head = null;
			while (Positions.Count > 0)
			{
				head = Positions.Dequeue();

				for (int n = 1; n <= 4; ++n)
				{
					var newDrone = head.Move(n, Explored);
					if (newDrone == null)
						continue;
					Positions.Enqueue(newDrone);
				}

			}
			return head.Coordinates.Distance;
		}
	}
}

using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_13
{
	using Coords = Tuple<Int64, Int64>;
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

		private Int64[] Memory { get; set; }
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

	class Arcade
	{
		public Dictionary<Coords, Int64> PlayGrid;
		public Intcode Brain;
		Coords paddleCoords;
		Coords ballCoords;
		public Int64 Score;
		public int facing = 0;

		public Arcade(Int64[] input)
		{
			PlayGrid = new Dictionary<Coords, Int64>();
			Brain = new Intcode(input);
		}

		private Int64 GetTile(Coords coords)
		{
			return PlayGrid.ContainsKey(coords) ? PlayGrid[coords] : 0;
		}

		public void ProcessOutputs()
		{
			while (Brain.GetOutputQueue().Count > 0)
			{
				var x = Brain.GetOutput();
				var y = Brain.GetOutput();
				var tile = Brain.GetOutput();

				if (x == -1 && y == 0)
					Score = tile;

				var p = new Coords(x, y);

				if (!PlayGrid.ContainsKey(p))
					PlayGrid.Add(p, tile);
				else
					PlayGrid[p] = tile;

				if (tile == 3)
					paddleCoords = p;
				else if (tile == 4)
					ballCoords = p;
			}
		}

		public void SetupPlayArea()
		{
			Brain.Run();
			ProcessOutputs();
		}

		public void PlayGame()
		{
			Brain.Run();
			while (Brain.Waiting)
			{
				ProcessOutputs();

				Brain.SendInput(MoveJoystick());

				Brain.Run();
			}
			ProcessOutputs();
		}

		private Int64 MoveJoystick()
		{
			return ballCoords.Item1.CompareTo(paddleCoords.Item1);
		}

		public int CountTiles(Int64 type)
		{
			return PlayGrid.Where(t => t.Value == type).Count();
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0]).Split(',').Select(Int64.Parse).ToArray();
			// Part 1
			var a = new Arcade(input);
			a.SetupPlayArea();
			Console.WriteLine($"Part 1: {a.CountTiles(2)}");

			// Part 2
			var b = new Arcade(input);
			input[0] = 2;
			b = new Arcade(input);
			b.PlayGame();
			Console.WriteLine($"Part 2: {b.Score}");

			Console.ReadKey();
		}
	}
}

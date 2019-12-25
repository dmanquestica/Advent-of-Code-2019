using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Day_25
{
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

		private Int64 GetMem(Int64 Address)
		{
			return Memory[Address];
		}

		private void SetMem(Int64 Address, Int64 value)
		{
			Memory[Address] = value;
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

	class Computer
	{
		private Intcode Brain;
		private Int64[] Program;
		public List<char> Buffer;
		private bool ManualPlay;
		private Queue<string> Recording;

		public Computer(Int64[] program, bool manualPlay = false)
		{
			Program = new Int64[program.Length];
			program.CopyTo(Program, 0);
			Brain = new Intcode(Program);
			ManualPlay = manualPlay;
			Recording = new Queue<string>();
		}

		public void Reset()
		{
			Brain = new Intcode(Program);
		}

		public void SendInput(string instruction)
		{
			Recording.Enqueue(instruction);
			foreach (var c in instruction.ToCharArray())
			{
				Brain.SendInput(c);
				if (!ManualPlay)
					Console.Write(c);
			}
			Brain.SendInput('\n');
			if (!ManualPlay)
				Console.WriteLine();
		}

		public void Run()
		{
			Brain.Run();
		}

		public void Output()
		{
			Buffer = new List<char>();
			var output = Brain.GetOutputQueue();
			while (output.Any())
			{
				var l = output.Dequeue();
				if (l == 10)
				{
					Buffer.Add('\n');
					Console.WriteLine();
				}
				else if (l < 256)
				{
					Buffer.Add((char)l);
					Console.Write((char)l);
				}
				else
				{
					foreach (var c in l.ToString())
						Buffer.Add((char)c);
					Console.WriteLine(l);
				}
			}
		}

		public void PlayRecording()
		{
			while (Recording.Any())
				Console.WriteLine(Recording.Dequeue());
		}

		public bool IsHalted()
		{
			return Brain.Halted;
		}

		public bool IsWaiting()
		{
			return Brain.Waiting;
		}

		public bool HasOutput()
		{
			return Brain.GetOutputQueue().Any();
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0]).Split(',').Select(Int64.Parse).ToArray();

			//ManualPlay(input);

			var steps = Utilities.ReadFile(args[1]);

			AutomaticPlay(input, steps);
		}

		static void AutomaticPlay(Int64[] input, IList<string> steps)
		{
			var mud = new Computer(input, true);
			var queue = new Queue<string>(steps);

			while (true)
			{
				mud.Run();
				if (mud.HasOutput())
				{
					mud.Output();
					if (mud.IsWaiting() && !mud.IsHalted())
					{
						var command = queue.Dequeue();
						Console.WriteLine(command);
						mud.SendInput(command);
					}
					else if (mud.IsHalted())
					{
						mud.Output();
						Console.ReadKey();
						break;
					}
				}
			}
		}

		static void ManualPlay(Int64[] input)
		{
			// Manual Play
			var mud = new Computer(input, true);

			while (true)
			{
				mud.Run();
				if (mud.HasOutput())
				{
					mud.Output();
					if (mud.IsWaiting() && !mud.IsHalted())
					{
						var command = Console.ReadLine();
						mud.SendInput(command);
					}
					else if (mud.IsHalted())
					{
						mud.Output();
						mud.PlayRecording();
						Console.ReadKey();
						break;
					}
				}
			}
		}
	}
}

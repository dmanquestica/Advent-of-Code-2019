﻿using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_23
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

	class Computer
	{
		public Int64[] Program;
		public Intcode Brain;

		public Computer(Int64[] program)
		{
			Program = new Int64[program.Length];
			program.CopyTo(Program, 0);
			Brain = new Intcode(Program);
		}

		public void Reset()
		{
			Brain = new Intcode(Program);
		}

		public void SendInput(Int64 instruction)
		{
			Brain.SendInput(instruction);
		}

		public void SendInput(Int64[] command)
		{
			foreach (var instruction in command) 
				Brain.SendInput(instruction);
		}
		public void Run()
		{
			Brain.Run();			
		}
	}

	class Program
	{
		public static Dictionary<Int64, Computer> Network;
		public static Dictionary<Int64, Queue<(Int64 x, Int64 y)>> Packets;
		public static (Int64 x, Int64 y) NAT;
		public static HashSet<Int64> NATLog;

		static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0]);

			Console.WriteLine($"Part 1: {Part1(input)}");
			Console.WriteLine($"Part 2: {Part2(input)}");
			Console.ReadKey();
		}

		static string Part1(string input)
		{
			var instructions = input.Split(',').Select(Int64.Parse).ToArray();

			Network = new Dictionary<Int64, Computer>();
			Packets = new Dictionary<Int64, Queue<(Int64 x, Int64 y)>>();

			for (var i = 0; i < 50; ++i)
			{
				Network[i] = new Computer(instructions);
				Network[i].SendInput(i);
				Network[i].Run();
			}

			while (true)
			{
				for (var i = 0; i < 50; ++i)
				{
					var current = Network[i];
					if (!Packets.ContainsKey(i))
						Packets[i] = new Queue<(long x, long y)>();
					for (var x = 0; x < Packets[i].Count(); ++x)
					{
						var packet = Packets[i].Dequeue();
						current.SendInput(new Int64[] { packet.x, packet.y });
					}

					current.Run();
					if (current.Brain.Waiting)
						current.SendInput(-1);

					var output = current.Brain.GetOutputQueue();
					while (output.Any())
					{
						var address = output.Dequeue();
						var x = output.Dequeue();
						var y = output.Dequeue();

						if (address == 255)
							return y.ToString();

						if (!Packets.ContainsKey(address))
							Packets[address] = new Queue<(long x, long y)>();
						Packets[address].Enqueue((x, y));
					}
				}
			}
		}

		static string Part2(string input)
		{
			var instructions = input.Split(',').Select(Int64.Parse).ToArray();

			Network = new Dictionary<Int64, Computer>();
			Packets = new Dictionary<Int64, Queue<(Int64 x, Int64 y)>>();
			NATLog = new HashSet<Int64>();

			for (var i = 0; i < 50; ++i)
			{
				Network[i] = new Computer(instructions);
				Network[i].SendInput(i);
				Network[i].Run();
			}

			while (true)
			{
				for (var i = 0; i < 50; ++i)
				{
					var current = Network[i];
					if (!Packets.ContainsKey(i))
						Packets[i] = new Queue<(long x, long y)>();
					for (var x = 0; x < Packets[i].Count(); ++x)
					{
						var packet = Packets[i].Dequeue();
						current.SendInput(new Int64[] { packet.x, packet.y });
					}

					current.Run();
					if (current.Brain.Waiting)
						current.SendInput(-1);

					var output = current.Brain.GetOutputQueue();
					while (output.Any())
					{
						var address = output.Dequeue();
						var x = output.Dequeue();
						var y = output.Dequeue();

						if (address == 255)
						{
							NAT = (x, y);
							continue;
						}
						Packets[address].Enqueue((x, y));
					}

					if (Packets.Sum(kvp => kvp.Value.Count) == 0)
					{
						/* detected idle network, put NAT on queue */
						if (NATLog.Contains(NAT.y))
						{
							return NAT.y.ToString();
						}
						else
						{
							NATLog.Add(NAT.y);
							Packets[0].Enqueue(NAT);
						}
					}
				}
			}
		}
	}
}

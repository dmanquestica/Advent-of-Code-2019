﻿using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Day_11
{
	using Coords = Tuple<int, int>;
	using INT = Int64;
	public enum ParameterMode
	{
		Positional = 0,
		Immediate = 1,
		Relative = 2
	}

	class Intcode
	{
		private readonly Queue<INT> InputQueue = new Queue<INT>();

		public INT OutputValue = 0;
		public Intcode OutputMachine = null;

		private long[] Memory { get; set; }
		private INT InstructionPointer = 0;
		private INT RelativeBase = 0;

		public bool Waiting = false;
		public bool Halted = false;
		public bool SentOutput = false;

		INT OpCode;

		public Intcode(INT[] initialMemory)
		{
			Memory = new long[initialMemory.Count() + 10000];
			initialMemory.CopyTo(Memory, 0);
		}

		public void SendInput(INT input)
		{
			InputQueue.Enqueue(input);
			Waiting = false;
		}

		private void Output(INT i)
		{
			OutputValue = i;
			if (OutputMachine != null)
			{
				OutputMachine.SendInput(i);
			}
			SentOutput = true;
		}

		private INT GetMem(INT address)
		{
			return Memory[address];
		}

		private void SetMem(INT address, INT value)
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

		private void SetParam(int idx, INT value)
		{
			INT param = GetMem(InstructionPointer + idx);
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

		private INT GetParam(int idx)
		{
			INT param = GetMem(InstructionPointer + idx);
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

		private void ParseOpCode(INT opCode)
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
				SetParam(3, (INT)1);
			else
				SetParam(3, (INT)0);
			InstructionPointer += 4;
		}

		public void IsEquals()
		{
			if (GetParam(1) == GetParam(2))
				SetParam(3, (INT)1);
			else
				SetParam(3, (INT)0);
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

	class Paintbot
	{
		public Dictionary<Coords, int> HullGrid;
		public Intcode Brain;
		public Coords Cursor;
		public int facing = 0;

		public Paintbot(INT[] input)
		{
			HullGrid = new Dictionary<Coords, int>();
			Brain = new Intcode(input);
			Cursor = new Coords(0, 0);
		}

		public int GetColor(Coords coords)
		{
			return HullGrid.ContainsKey(coords) ? HullGrid[coords] : 0;
		}

		public int GetColor()
		{
			return GetColor(Cursor);
		}

		public void SetColor(int color)
		{
			HullGrid[Cursor] = color;
		}

		public void TurnLeft()
		{
			facing = (facing + 3) % 4;
		}

		public void TurnRight()
		{
			facing = (facing + 1) % 4;
		}

		public void StepForward()
		{
			switch (facing)
			{
				case 0:
					Cursor = new Coords(Cursor.Item1, Cursor.Item2 + 1);
					break;
				case 1:
					Cursor = new Coords(Cursor.Item1 + 1, Cursor.Item2);
					break;
				case 2:
					Cursor = new Coords(Cursor.Item1, Cursor.Item2 - 1);
					break;
				case 3:
					Cursor = new Coords(Cursor.Item1 - 1, Cursor.Item2);
					break;
			}
		}

		public void Run()
		{
			Brain.RunToOutput();
			while (!Brain.Halted)
			{
				if (Brain.Waiting)
				{
					Brain.SendInput(GetColor());
				}
				else
				{
					SetColor((int)Brain.OutputValue);
					Brain.RunToOutput();
					if (Brain.OutputValue == 0)
						TurnLeft();
					else
						TurnRight();
					StepForward();
				}
				Brain.RunToOutput();
			}
		}

		public int CountPainted()
		{
			return HullGrid.Count;
		}

		public void Display()
		{
			int minx = 0;
			int maxx = 0;
			int miny = 0;
			int maxy = 0;
			foreach (var coords in HullGrid.Keys)
			{
				if (coords.Item1 < minx) minx = coords.Item1;
				if (coords.Item1 > maxx) maxx = coords.Item1;
				if (coords.Item2 < miny) miny = coords.Item2;
				if (coords.Item2 > maxy) maxy = coords.Item2;
			}

			for (int y = maxy; y >= miny; y--)
			{
				for (int x = minx; x <= maxx; x++)
				{
					Console.Write(GetColor(new Coords(x, y)) > 0 ? '#' : ' ');
				}
				Console.WriteLine();
			}
		}
	}

	class Program
	{
		public static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0]).Split(',').Select(INT.Parse).ToArray();
			Paintbot p = new Paintbot(input);
			p.Run();
			Console.WriteLine($"Part 1: {p.CountPainted()}");

			p = new Paintbot(input);
			p.SetColor(1);
			p.Run();
			Console.WriteLine("Part 2:");
			p.Display();
			Console.ReadKey();
		}

	}
}



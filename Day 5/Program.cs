﻿using Shared_Utilities;
using System;
using System.Linq;

namespace Day_5
{
	class Memory
	{
		public int[] Allocated;

		public Memory(int[] data)
		{
			Allocated = data;
		}

		public int GetData(int location)
		{
			return Allocated[location];
		}

		public void SetData(int location, int value)
		{
			Allocated[location] = value;
		}
	}

	class IntCodeComputer
	{
		public Memory mem;
		public int pointer;

		public IntCodeComputer(int[] data)
		{
			mem = new Memory(data);

			pointer = 0;
		}

		public void Run()
		{
			while (Machine())
			{
			}
		}

		private bool Machine()
		{
			var opcode = mem.GetData(pointer).ToString();
			opcode = opcode.PadLeft(5,'0');

			int instruction = int.Parse(opcode.Substring(3));

			if (instruction == 3)
			{
				opcode = opcode.Replace('0', '1');
			}

			//Fetch parameters
			int parameter1 = 0;
			int parameter2 = 0;
			int parameter3 = 0;

			if (instruction < 10)
			{
				if (opcode[2] == '1')
				{
					parameter3 = mem.GetData(pointer + 1);
				}
				else
				{
					parameter3 = mem.GetData(mem.GetData(pointer + 1));
				}
			}

			if (instruction < 3 || (instruction > 4 && instruction != 99))
			{
				if (opcode[1] == '1')
				{
					parameter2 = mem.GetData(pointer + 2);
				}
				else
				{
					parameter2 = mem.GetData(mem.GetData(pointer + 2));
				}
			}

			if (instruction < 3 || (instruction > 6 && instruction != 99))
			{
				parameter1 = mem.GetData(pointer + 3);
			}


			//Run the opcode

			switch (instruction)
			{
				case 1:
					Addition(parameter3, parameter2, parameter1);
					break;
				case 2:
					Multiplication(parameter3, parameter2, parameter1);
					break;
				// Additions for Part 1
				case 3:
					Input(parameter3);
					break;
				case 4:
					Output(parameter3);
					break;
				// Modifications for Part 2
				case 5:
					JumpIfTrue(parameter3, parameter2);
					break;
				case 6:
					JumpIfNotTrue(parameter3, parameter2);
					break;
				case 7:
					LessThan(parameter3, parameter2, parameter1);
					break;
				case 8:
					Equals(parameter3, parameter2, parameter1);
					break;

				case 99:
					return false;
			}

			return true;
		}

		private void Addition(int parameter3, int parameter2, int parameter1)
		{
			mem.SetData(parameter1, parameter3 + parameter2);
			pointer += 4;
		}

		private void Multiplication(int parameter3, int parameter2, int parameter1)
		{
			mem.SetData(parameter1, parameter3 * parameter2);
			pointer += 4;
		}

		private void Input(int address)
		{
			Console.WriteLine("Computer wants an input: ");
			mem.SetData(address, int.Parse(Console.ReadLine()));
			pointer += 2;
		}

		private void Output(int value)
		{
			Console.WriteLine("Output from computer: " + value);
			pointer += 2;
		}

		private void JumpIfTrue(int parameter3, int parameter2)
		{
			if (parameter3 != 0)
			{
				pointer = parameter2;
			}
			else
			{
				pointer += 3;
			}
		}

		private void JumpIfNotTrue(int parameter3, int parameter2)
		{
			if (parameter3 == 0)
			{
				pointer = parameter2;
			}
			else
			{
				pointer += 3;
			}

		}

		private void LessThan(int parameter3, int parameter2, int parameter1)
		{
			if (parameter3 < parameter2)
			{
				mem.SetData(parameter1, 1);
			}
			else
			{
				mem.SetData(parameter1, 0);
			}
			pointer += 4;
		}

		private void Equals(int parameter3, int parameter2, int parameter1)
		{
			if (parameter3 == parameter2)
			{
				mem.SetData(parameter1, 1);
			}
			else
			{
				mem.SetData(parameter1, 0);
			}
			pointer += 4;
		}

		public int ReadMemory(int address)
		{
			return mem.GetData(address);
		}
	}

	class Program
	{
		public static void Main(string[] args)
		{
			var input = Utilities.ReadFileAsString(args[0]);
			var program = input.Split(',').Select(int.Parse).ToArray();

			var computer = new IntCodeComputer(program);

			computer.Run();
			Console.ReadKey();
		}
	}
}
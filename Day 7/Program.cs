using Day_7;
using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Day_7
{
	public class IntCodeMachine
	{
		public List<int> outputs = new List<int>();

		private int[] program = new int[0];

		public int[] ProgramCode
		{
			get { return program; }
		}

		public int[] inputs;
		int inputCursor = 0;

		public int RunProgram(int[] programCode, int[] input, bool pauseOnOutput = false)
		{
			inputCursor = 0;
			this.inputs = input;
			program = programCode;
			bool isrunning = true;
			while (isrunning)
			{
				var x = program[CurrentPositionPointer];

				ParseOpCode(x);

				switch (OpCode)
				{
					case 1:
						Addition();
						break;
					case 2:
						Multiply();
						break;
					case 3:
						Input();
						break;
					case 4:
						Output();
						if (pauseOnOutput)
							return 0;
						break;
					case 5:
						JumpIfTrue();
						break;
					case 6:
						JumpIfFalse();
						break;
					case 7:
						IsLessThan();
						break;
					case 8:
						IsEquals();
						break;
					case 99:
						isrunning = false;
						break;
					default:
						Console.WriteLine("Program Halted Unexpectedly");
						isrunning = false;
						break;
				}
			}

			return 1;
		}

		int CurrentPositionPointer = 0;
		int OpCode;
		int SecondParamMode;
		int FirstParamMode;

		private void ParseOpCode(int opCode)
		{
			SecondParamMode = (opCode / 1000 % 100 % 10);
			FirstParamMode = (opCode / 100 % 10);
			OpCode = (opCode % 100);
		}

		//add
		//three parameters
		public void Addition()
		{
			var p1 = program[CurrentPositionPointer + 1];
			var p2 = program[CurrentPositionPointer + 2];
			var p3 = program[CurrentPositionPointer + 3];

			if (FirstParamMode == 0)
			{
				p1 = program[p1];
			}
			if (SecondParamMode == 0)
			{
				p2 = program[p2];
			}

			program[p3] = p1 + p2;
			CurrentPositionPointer += 4;
		}

		//multiply
		//three parameters
		public void Multiply()
		{
			var p1 = program[CurrentPositionPointer + 1];
			var p2 = program[CurrentPositionPointer + 2];
			var p3 = program[CurrentPositionPointer + 3];

			if (FirstParamMode == 0)
			{
				p1 = program[p1];
			}
			if (SecondParamMode == 0)
			{
				p2 = program[p2];
			}

			program[p3] = p1 * p2;
			CurrentPositionPointer += 4;
		}

		//read input
		//one parameter
		public void Input()
		{
			var p1 = program[CurrentPositionPointer + 1];
			program[p1] = inputs[inputCursor];

			inputCursor++;

			CurrentPositionPointer += 2;
		}

		//output
		//one parameter
		public void Output()
		{
			var p1 = program[CurrentPositionPointer + 1];

			if (FirstParamMode == 0)
			{
				p1 = program[p1];
			}

			outputs.Add(p1);
			CurrentPositionPointer += 2;
		}

		//jump if true 
		//two parameters
		public void JumpIfTrue()
		{
			var p1 = program[CurrentPositionPointer + 1];
			var p2 = program[CurrentPositionPointer + 2];

			if (FirstParamMode == 0)
			{
				p1 = program[p1];
			}
			if (SecondParamMode == 0)
			{
				p2 = program[p2];
			}

			if (p1 != 0)
			{
				CurrentPositionPointer = p2;
			}
			else
			{
				CurrentPositionPointer += 3;
			}

		}

		//jump if false
		//two parameters
		public void JumpIfFalse()
		{
			var p1 = program[CurrentPositionPointer + 1];
			var p2 = program[CurrentPositionPointer + 2];

			if (FirstParamMode == 0)
			{
				p1 = program[p1];
			}
			if (SecondParamMode == 0)
			{
				p2 = program[p2];
			}

			if (p1 == 0)
			{
				CurrentPositionPointer = p2;
			}
			else
			{
				CurrentPositionPointer += 3;
			}
		}

		//is less than
		//three parameters
		public void IsLessThan()
		{
			var p1 = program[CurrentPositionPointer + 1];
			var p2 = program[CurrentPositionPointer + 2];
			var p3 = program[CurrentPositionPointer + 3];

			if (FirstParamMode == 0)
			{
				p1 = program[p1];
			}
			if (SecondParamMode == 0)
			{
				p2 = program[p2];
			}

			program[p3] = (p1 < p2 ? 1 : 0);
			CurrentPositionPointer += 4;
		}

		//equals
		//three parameters
		public void IsEquals()
		{
			var p1 = program[CurrentPositionPointer + 1];
			var p2 = program[CurrentPositionPointer + 2];
			var p3 = program[CurrentPositionPointer + 3];

			if (FirstParamMode == 0)
			{
				p1 = program[p1];
			}
			if (SecondParamMode == 0)
			{
				p2 = program[p2];
			}

			program[p3] = (p1 == p2 ? 1 : 0);
			CurrentPositionPointer += 4;
		}
	}


	public class Program
	{
		static void Main(string[] args)
		{
			Part1(args[0]);
			Part2(args[0]);
			Console.ReadKey();
		}

		public static void Part1(string codeFile)
		{
			var phases = new int[] { 0, 1, 2, 3, 4 };
			var output = 0;
			var outputs = new List<int>();
			var phasesettings = new int[] { 0, 1, 2, 3, 4 };

			List<string> range = new List<string>();
			GetPermutations(phases, ref range);
			int finaloutput = 0;

			foreach (var r in range)
			{
				for (int i = 0; i < 5; i++)
				{
					phasesettings = r.ToList()
						.Select(x => int.Parse(x.ToString()))
						.ToArray();
					var code = Utilities.ReadFileAsString(codeFile).Split(',').Select(int.Parse).ToArray();
					var amp = new IntCodeMachine();
					amp.RunProgram(code, new int[] { phasesettings[i], output });
					output = amp.outputs.Last();
					outputs.Add(output);
				}
				finaloutput = Math.Max(finaloutput, outputs.Last());
				outputs.Clear();
				output = 0;
			}
			Console.WriteLine(finaloutput);
		}

		public static void Part2(string codeFile)
		{
			var output = 0;
			var outputs = new List<int>();
			var phasesettings = new int[] { 9, 8, 7, 6, 5 };

			List<string> range = new List<string>();
			GetPermutations(phasesettings, ref range);
			int finaloutput = 0;

			foreach (var r in range)
			{
				phasesettings = r.ToList()
						.Select(x => int.Parse(x.ToString()))
						.ToArray();

				var amp1 = new IntCodeMachine();
				var amp2 = new IntCodeMachine();
				var amp3 = new IntCodeMachine();
				var amp4 = new IntCodeMachine();
				var amp5 = new IntCodeMachine();

				var code1 = Utilities.ReadFileAsString(codeFile).Split(',').Select(int.Parse).ToArray();
				var code2 = Utilities.ReadFileAsString(codeFile).Split(',').Select(int.Parse).ToArray();
				var code3 = Utilities.ReadFileAsString(codeFile).Split(',').Select(int.Parse).ToArray();
				var code4 = Utilities.ReadFileAsString(codeFile).Split(',').Select(int.Parse).ToArray();
				var code5 = Utilities.ReadFileAsString(codeFile).Split(',').Select(int.Parse).ToArray();

				bool initSetup = true;
				while (true)
				{
					var isdone = 0;

					int[] inp1 = initSetup ? new int[] { phasesettings[0], output } : new int[] { output };
					isdone += amp1.RunProgram(code1, inp1, true);
					output = amp1.outputs.Last();

					int[] inp2 = initSetup ? new int[] { phasesettings[1], output } : new int[] { output };
					isdone += amp2.RunProgram(code2, inp2, true);
					output = amp2.outputs.Last();

					int[] inp3 = initSetup ? new int[] { phasesettings[2], output } : new int[] { output };
					isdone += amp3.RunProgram(code3, inp3, true);
					output = amp3.outputs.Last();

					int[] inp4 = initSetup ? new int[] { phasesettings[3], output } : new int[] { output };
					isdone += amp4.RunProgram(code4, inp4, true);
					output = amp4.outputs.Last();

					int[] inp5 = initSetup ? new int[] { phasesettings[4], output } : new int[] { output };
					isdone += amp5.RunProgram(code5, inp5, true);
					output = amp5.outputs.Last();
					outputs.Add(output);

					if (initSetup)
					{
						initSetup = false;
					}

					if (isdone != 0)
					{
						break;
					}
				}
				finaloutput = Math.Max(finaloutput, outputs.Last());
				outputs.Clear();
				output = 0;
			}

			Console.WriteLine(finaloutput);
		}

		#region Create Permutations

		private static void Swap(ref int a, ref int b)
		{
			if (a == b) return;

			var temp = a;
			a = b;
			b = temp;
		}

		public static void GetPermutations(int[] list, ref List<string> output)
		{
			int x = list.Length - 1;
			GetPer(list, 0, x, ref output);
		}

		private static void GetPer(int[] list, int k, int m, ref List<string> output)
		{
			if (k == m)
			{
				output.Add(string.Join(string.Empty, list.Select(c => c.ToString()).ToArray()));
			}
			else
				for (int i = k; i <= m; i++)
				{
					Swap(ref list[k], ref list[i]);
					GetPer(list, k + 1, m, ref output);
					Swap(ref list[k], ref list[i]);
				}
		}
		#endregion
	}
}


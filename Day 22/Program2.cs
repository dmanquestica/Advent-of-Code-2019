using Shared_Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Day_22
{
	class Card
	{
		public Int64 Name;

		public Card(Int64 name)
		{
			Name = name;
		}
	}

	class Deck
	{
		private LinkedList<Card> Cards;
		private Int64 Size;

		public Deck(Int64 size)
		{
			Cards = new LinkedList<Card>();
			Size = size;

			for (Int64 i = 0; i < Size; ++i)
			{
				Cards.AddLast(new Card(i));
			}
		}

		public void Reset()
		{
			Cards = new LinkedList<Card>();

			for (var i = 0; i < Size; ++i)
			{
				Cards.AddLast(new Card(i));
			}
		}

		public void NewStack()
		{
			Cards = new LinkedList<Card>(Cards.Reverse());
		}

		public void CutTop(Int64 number)
		{
			var queue = new Queue<Card>();

			Int64 i = 0;
			while (i < number)
			{
				queue.Enqueue(Cards.First());
				Cards.RemoveFirst();
				++i;
			}

			while (queue.Any())
			{
				Cards.AddLast(queue.Dequeue());
			}
		}

		public void CutBottom(Int64 number)
		{
			Int64 absNumber = Math.Abs(number);

			var queue = new Queue<Card>();

			Int64 i = 0;
			while (i < absNumber)
			{
				queue.Enqueue(Cards.Last());
				Cards.RemoveLast();
				++i;
			}

			while (queue.Any())
			{
				Cards.AddFirst(queue.Dequeue());
			}
		}

		public void Increment(Int64 number)
		{
			var table = Array.CreateInstance(typeof(Card), Cards.Count());

			Int64 index = 0;
			while (Cards.Any())
			{
				if (table.GetValue(index) == null)
				{
					table.SetValue(Cards.First(), index);
					Cards.RemoveFirst();
					index = (index + number) % table.Length;
				}
				else
				{
					throw new Exception("Already a card in this spot");
				}
			}
			Cards = new LinkedList<Card>();

			for (Int64 i = 0; i < table.Length; ++i)
			{
				Cards.AddLast((Card)table.GetValue(i));
			}
		}

		public override string ToString()
		{
			return string.Join(",", Cards.ToArray().Select(c => c.Name));
		}

		public Int64 GetIndexOfCard(Int64 name)
		{
			return Array.FindIndex(Cards.ToArray(), c => c.Name == name);
		}

		public Int64 Count()
		{
			return Cards.Count();
		}
	}

	class Program2
	{
		public static Deck deck;

		static void Main(string[] args)
		{
			var input = Utilities.ReadFile(args[0]);
			var instructions = input.Select(s => s.Split(' ')).ToList();

			Part1(instructions, 10007, 1, 2019);
			Part2(instructions, 119315717514047L, 101741582076661L, 2020);
			Console.ReadKey();
		}

		/// <summary>
		/// Naive method
		/// </summary>
		/// <param name="instructions"></param>
		/// <param name="deckSize"></param>
		/// <param name="shuffles"></param>
		/// <param name="position"></param>
		static void Part1(List<string[]> instructions, int deckSize, int shuffles, int position)
		{
			deck = new Deck(deckSize);

			for (int i = 0; i < shuffles; ++i)
			{
				foreach (var s in instructions)
				{
					if (s.Length == 2)
					{
						if (Int64.Parse(s[1]) < 0)
							deck.CutBottom(Int64.Parse(s[1]));
						else
							deck.CutTop(Int64.Parse(s[1]));
					}
					else if (s.Length == 4)
					{
						if (s[3] == "stack")
							deck.NewStack();
						else
							deck.Increment(Int64.Parse(s[3]));
					}
					else
						throw new Exception("Unknown instruction format");
				}
			}
			Console.WriteLine($"Part 1: {deck.GetIndexOfCard(position)}");
		}

		/// <summary>
		/// Math method
		/// </summary>
		/// <param name="instructions"></param>
		/// <param name="deckSize"></param>
		/// <param name="shuffles"></param>
		/// <param name="position"></param>
		static void Part2(List<string[]> instructions, BigInteger deckSize, BigInteger shuffles, BigInteger position)
		{
			var lines = instructions.Reverse<string[]>();

			BigInteger a = 1;
			BigInteger b = 0;

			foreach (var s in lines)
			{
				if (s.Length == 2)
				{
					// cutting just shifts the B parameter - the offset
					var i = BigInteger.Parse(s.Last());

					while (i < 0)
						i += deckSize;
					b += i;
				}
				else if (s[3] == "stack")
				{
					// reverse changes sign (offset changes by one before sign inverse)
					a = -a;
					b = -++b;
				}
				else
				{
					// modInverse for prime base https://en.wikipedia.org/wiki/Modular_multiplicative_inverse
					var inversePower = BigInteger.ModPow(BigInteger.Parse(s.Last()), deckSize - 2, deckSize);
					a = a * inversePower % deckSize;
					b = b * inversePower % deckSize;
				}
				a %= deckSize;
				b %= deckSize;
				while (b < 0)
					b += deckSize;
				while (a < 0)
					a += deckSize;
			}
			// we are looking for an N-th power of the equation describing the function composition:
			// (a*x + b)^N
			// ax + b
			// a^2x + ab + b
			// a^3x + a^2b + ab + b
			// a^4x + a^3b + a^2b + ab + b etc.
			// in general, the nth term looks like
			// a^nx + a^(n - 1)b + a^(n - 2)b + ... + a^2b + ab + b
			// which can be factored into
			// a^nx + b(1 - a^n) / (1 - a)

			var part1 = position * BigInteger.ModPow(a, shuffles, deckSize);
			var part2 = b * (BigInteger.ModPow(a, shuffles, deckSize) - 1);
			var part3 = BigInteger.ModPow(a - 1, deckSize - 2, deckSize);

			var answer = (part1 + part2 * part3) % deckSize;
			Console.WriteLine($"Part 2: {answer}");
		}
	}
}

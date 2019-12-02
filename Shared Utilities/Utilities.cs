using System;
using System.Collections.Generic;
using System.IO;

namespace Shared_Utilities
{

	public static class Utilities
	{
		public static IList<string> ReadFile(string path)
		{
			var list = new List<string>();
			var file = path;

			using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				using (var sr = new StreamReader(fs))
				{
					while (!sr.EndOfStream)
						list.Add(sr.ReadLine());
				}
			}

			return list;
		}

		public static string ReadFileAsString(string path)
		{
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				using (var sr = new StreamReader(fs))
					return sr.ReadToEnd();
			}
		}
	}
}

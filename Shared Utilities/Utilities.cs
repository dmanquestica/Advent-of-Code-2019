using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
		public static T DeepClone<T>(this T obj)
		{
			using (var ms = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(ms, obj);
				ms.Position = 0;

				return (T)formatter.Deserialize(ms);
			}
		}
	}
}

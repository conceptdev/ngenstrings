using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace ngenstrings
{
	public class LocalizedStringTable : Dictionary<string,LocalizedString>
	{
		const string DEFAULT_FILE_EXTENSION = "strings";
		public string FileName
		{ get; private set; }
		public LocalizedStringTable (string fileName)
		{
			FileName = fileName;
		}

		public bool WriteStringsFile (string fromAssemblyName)
		{
			var sb = new StringBuilder();
			foreach (var ls in this.Values)
			{
				if (!ls.IsEmpty)
					sb.Append(ls.ToString());
			}

			var filename = CombineFilenameExtension(FileName, DEFAULT_FILE_EXTENSION);
			string s = sb.ToString();
			File.WriteAllText(filename
				,LocalizedString.FileHeaderString (fromAssemblyName) 
				,Encoding.UTF8);
			File.AppendAllText(filename,s, Encoding.UTF8);
			Console.WriteLine(String.Format("File '{0}' written.", filename));
			return true;
		}
		private string CombineFilenameExtension (string filename, string extension)
		{
			return filename + "." + extension;
		}
	}
}

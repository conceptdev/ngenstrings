using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
/*
Yes, I agree that this class is pretty dodgy. the PList XML export is lazy
*/
namespace ngenstrings
{
	/// <summary>
	/// Dictionary of keys with default values and comments;
	/// sorted so that it is easy to compare in a file diff tool.
	/// </summary>
	public class LocalizedStringTable : SortedDictionary<string,LocalizedString>
	{
		/// <summary>strings</summary>
		const string DEFAULT_FILE_EXTENSION = "strings";

		public string FileName
		{ get; private set; }
		public LocalizedStringTable (string fileName)
		{
			FileName = fileName;
		}

		/// <summary>
		/// TODO: refactor into seperate per-OutputType methods
		/// </summary>
		public bool WriteStringsFile (string fromAssemblyName, OutputFormat format)
		{
			return WriteStringsFile (fromAssemblyName, format, "");
		}

		/// <summary>
		/// TODO: refactor into seperate per-OutputType methods
		/// </summary>
		public bool WriteStringsFile (string fromAssemblyName, OutputFormat format, string languageCode)
		{
			var filename = CombineFilenameExtension(FileName, DEFAULT_FILE_EXTENSION);
			if (!String.IsNullOrEmpty(languageCode)) filename = languageCode + "_" + filename;

			switch (format)
			{
				case OutputFormat.Strings:
					var sb = new StringBuilder();
		
					sb.Append(LocalizedString.FileHeaderCStyleString(fromAssemblyName));
		
					foreach (var ls in this.Values)
					{
						if (!ls.IsEmpty)
						{
							sb.Append(ls.ToCStyleString());
						}
					}
					string s = sb.ToString();
					File.WriteAllText(filename, s, Encoding.UTF8);
					break;
				case OutputFormat.Xml:
					var list = new List<LocalizedString>();
					foreach (var v in this.Values) list.Add(v);
					filename = CombineFilenameExtension(FileName, "xml");
					XmlSerializer serializer = new XmlSerializer(typeof (List<LocalizedString>));
					System.IO.TextWriter writer = new System.IO.StreamWriter (filename);
					serializer.Serialize(writer, list);
					writer.Close();
					break;
				case OutputFormat.PList:
					var sb1 = new StringBuilder();
		
					sb1.Append(LocalizedString.FileHeaderXmlString(fromAssemblyName));
					foreach (var ls in this.Values)
					{
						if (!ls.IsEmpty)
						{
							sb1.Append(ls.ToPListString());
						}
					}
					sb1.Append(LocalizedString.FileFooterXmlString());
					string s1 = sb1.ToString();
					filename = CombineFilenameExtension(FileName, "plist");
					File.WriteAllText(filename, s1, Encoding.UTF8);		break;
				default:
					throw new NotImplementedException("OutputFormat "+ format + " not implemented in LocalizedStringTable");
			}
			Console.WriteLine(String.Format("File '{0}' written.", filename));
			return true;
		}
		private string CombineFilenameExtension (string filename, string extension)
		{
			return filename + "." + extension;
		}


	}
}

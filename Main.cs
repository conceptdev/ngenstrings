using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using System.Text;
using System.Collections.Generic;
/* The original genstrings works like this; 
most of these options probably don't make sense for ngengstrings.

Usage: genstrings [OPTION] file1.[mc] ... filen.[mc]

Options
 -j                       sets the input language to Java.
 -a                       append output to the old strings files.
 -s substring             substitute 'substring' for NSLocalizedString.
 -skipTable tablename     skip over the file for 'tablename'.
 -noPositionalParameters  turns off positional parameter support.
 -u                       allow unicode characters.
 -macRoman                read files as MacRoman not UTF-8.
 -q                       turns off multiple key/value pairs warning.
 -bigEndian               output generated with big endian byte order.
 -littleEndian            output generated with little endian byte order.
 -o dir                   place output files in 'dir'.
*/
namespace ngenstrings
{
	/// <summary>
	/// mono ngenstrings.exe Localization02.exe
	/// </summary>
	class MainClass
	{
		const string DEFAULT_FILE_NAME = "Localizable";
		const string DEFAULT_FILE_EXTENSION = "strings";
		public static void Main (string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: ngenstrings assemblyname");
				Console.WriteLine("");
				Environment.ExitCode = 1;
				return; 
			}
			string assemblyName = args[0];
			AssemblyDefinition assembly = AssemblyFactory.GetAssembly(assemblyName);
			Console.WriteLine("ngenstrings");
			Console.WriteLine("Assembly: " + assemblyName);
			Console.WriteLine("Format: c-style key-value pairs (default)");

			var methods = new MethodSignatureCollection();
			methods.Add(new MethodSignature("System.String MonoTouch.Foundation.NSBundle::LocalizedString(System.String,System.String,System.String)", new List<Parameter>{Parameter.Key, Parameter.Value, Parameter.Table}));
			methods.Add(new MethodSignature("System.String Extensions::LocalizedString(MonoTouch.Foundation.NSBundle,System.String,System.String)", new List<Parameter>{Parameter.NSBundleIdentifier, Parameter.Key, Parameter.Comment}));
			methods.Add(new MethodSignature("System.String Extensions::LocalizedString(MonoTouch.Foundation.NSBundle,System.String,System.String,System.String,System.String)", new List<Parameter>{Parameter.NSBundleIdentifier, Parameter.Key, Parameter.Value, Parameter.Table,Parameter.Comment}));

			Console.WriteLine("Processing these method calls:");
			foreach (MethodSignature item in methods) {
				Console.WriteLine(item);
			}
			Console.WriteLine(" - - - - - - - - - - - - - - -");

			var tables = new Dictionary<string, LocalizedStringTable>();
			tables.Add("", new LocalizedStringTable(DEFAULT_FILE_NAME));
			var sb = new StringBuilder();

			foreach(TypeDefinition type in assembly.MainModule.Types)
			{
				foreach (MethodDefinition methodDefinition in type.Methods)
				{
					for (int i = 0; i < methodDefinition.Body.Instructions.Count; i++)
					{
						Instruction instruction = methodDefinition.Body.Instructions[i];
						if (instruction.Operand != null)
						{
							MethodSignature method = null;
							if (methods.Matches(instruction.Operand.ToString(), out method))
							{
								var locstring = method.Parse (methodDefinition.Body.Instructions, i);
								locstring.InMethods.Add(methodDefinition.DeclaringType + "." + methodDefinition.Name +"()");
								// output to console for information/debugging
								Console.WriteLine(locstring.ToString());

								// collect into tables
								if (!locstring.IsEmpty)
								{
									if (!tables.ContainsKey(locstring.Table))
									{
										tables.Add(locstring.Table, new LocalizedStringTable(locstring.Table));
									}
									tables[locstring.Table].Add(locstring.Key, locstring);
								}

								if (!locstring.IsEmpty)
									sb.Append(locstring.ToString());

							}
						}
					}
				}
			}

			// write out all the files
			var success = true;
			foreach (var table in tables.Values)
			{
				success = success && table.WriteStringsFile(assemblyName);
			}
			Environment.ExitCode = success?0:1;
		}
		
		/*static void WriteStringsFile (string filename, StringBuilder sb, string fromAssemblyName)
		{
			filename = CombineFilenameExtension(filename, DEFAULT_FILE_EXTENSION);
			string s = sb.ToString();
			File.WriteAllText(filename
				,LocalizedString.FileHeaderString (fromAssemblyName) 
				,Encoding.UTF8);
			File.AppendAllText(filename,s, Encoding.UTF8);
			Console.WriteLine(String.Format("File '{0}' written.", filename));

		}
		static string CombineFilenameExtension (string filename, string extension)
		{
			return filename + "." + extension;
		}*/
	}
}

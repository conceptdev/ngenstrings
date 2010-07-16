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
		/// <summary>Localizable</summary>
		const string DEFAULT_FILE_NAME = "Localizable";

		public static void Main (string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: mono ngenstrings.exe assemblyname.dll");
				Console.WriteLine("");
				Environment.ExitCode = 1;
				return; 
			}
			string assemblyName = args[0];
			AssemblyDefinition assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly (assemblyName);
			//AssemblyDefinition assembly = AssemblyFactory.GetAssembly(assemblyName);
			Console.WriteLine("ngenstrings");
			Console.WriteLine("Assembly: " + assemblyName);
			Console.WriteLine("Format: c-style key-value pairs (default)");

			var methods = new MethodSignatureCollection();
			methods.Add(new MethodSignature("System.String MonoTouch.Foundation.NSBundle::LocalizedString(System.String,System.String)", new List<Parameter>{Parameter.Key, Parameter.Comment}));
			methods.Add(new MethodSignature("System.String MonoTouch.Foundation.NSBundle::LocalizedString(System.String,System.String,System.String)", new List<Parameter>{Parameter.Key, Parameter.Value, Parameter.Table}));
			methods.Add(new MethodSignature("System.String MonoTouch.Foundation.NSBundle::LocalizedString(System.String,System.String,System.String,System.String)", new List<Parameter>{Parameter.Key, Parameter.Value, Parameter.Table, Parameter.Comment}));

			//methods.Add(new MethodSignature("System.String Extensions::LocalizedString(MonoTouch.Foundation.NSBundle,System.String,System.String)", new List<Parameter>{Parameter.NSBundleIdentifier, Parameter.Key, Parameter.Comment}));
			//methods.Add(new MethodSignature("System.String Extensions::LocalizedString(MonoTouch.Foundation.NSBundle,System.String,System.String,System.String,System.String)", new List<Parameter>{Parameter.NSBundleIdentifier, Parameter.Key, Parameter.Value, Parameter.Table,Parameter.Comment}));

			// Example 'custom' extraction for Miguel's TweetStation
			methods.Add(new MethodSignature("System.String TweetStation.Locale::GetText(System.String)", new List<Parameter>{Parameter.Key}));
			methods.Add(new MethodSignature("System.String TweetStation.Locale::GetText(System.String, System.Object)", new List<Parameter>{Parameter.Key, Parameter.Ignore}));

			Console.WriteLine("Processing these method calls:");
			foreach (MethodSignature item in methods)
			{
				Console.WriteLine(item);
			}
			Console.WriteLine(" - - - - - - - - - - - - - - -");

			var tables = new Dictionary<string, LocalizedStringTable>();
			tables.Add("", new LocalizedStringTable(DEFAULT_FILE_NAME));

			// iterate method calls with localizable strings
			foreach(TypeDefinition type in assembly.MainModule.Types)
			{
				foreach (MethodDefinition methodDefinition in type.Methods)
				{

					if (methodDefinition.HasBody)
					for (int i = 0; i < methodDefinition.Body.Instructions.Count; i++)
					{
						Instruction instruction = methodDefinition.Body.Instructions[i];

						if (instruction.Operand != null)
						{
							MethodSignature method = null;
							if (methods.Matches(instruction.Operand.ToString(), out method))
							{
								var locstring = method.Parse (methodDefinition.Body.Instructions, i);
								var methstring = methodDefinition.DeclaringType + "." + methodDefinition.Name +"()";
								locstring.InMethods.Add(methstring);
								// collect into tables
								if (!locstring.IsEmpty)
								{
									if (!tables.ContainsKey(locstring.Table))
									{ // ensure table exists
										tables.Add(locstring.Table, new LocalizedStringTable(locstring.Table));
									}
									if (!tables[locstring.Table].ContainsKey(locstring.Key))
									{ // add string if it isn't already there
										tables[locstring.Table].Add(locstring.Key, locstring);
									}
									else
									{ // error if already there, duplicate key (only if value different? or comment too?)
										var duplocstring = tables[locstring.Table][locstring.Key];
										Console.WriteLine (String.Format (
											"Duplicate key \"{0}\" found in {1}; was already in {2} other place/s"
											, locstring.Key
											, methstring
											, duplocstring.InMethods.Count));
										duplocstring.InMethods.Add(methstring);
									}
								}
								// output to console for information/debugging
								Console.Write(locstring.ToString());
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
	}
}

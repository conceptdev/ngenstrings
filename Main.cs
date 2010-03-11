using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace mgenstrings
{
	/// <summary>
	/// mono ngenstrings.exe Localization02.exe
	/// </summary>
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: mgenstrings assemblyname");
				Environment.ExitCode = 1;
				return; 
			}
			string assemblyName = args[0];
			AssemblyDefinition assembly = AssemblyFactory.GetAssembly(assemblyName);

			foreach(TypeDefinition type in assembly.MainModule.Types)
			{
				Console.WriteLine(type.FullName);

				var sb = new StringBuilder();

				foreach (MethodDefinition methodDefinition in type.Methods)
				{
					for (int i = 0; i < methodDefinition.Body.Instructions.Count; i++)
					{
						Instruction instruction = methodDefinition.Body.Instructions[i];
						if (instruction.Operand != null)
						{
							
							if (instruction.Operand.ToString() == "System.String MonoTouch.Foundation.NSBundle::LocalizedString(System.String,System.String,System.String)")
							{

								string text = methodDefinition.Body.Instructions[i-3].Operand as string;
								string note = methodDefinition.Body.Instructions[i-2].Operand as string;
								string table = methodDefinition.Body.Instructions[i-1].Operand as string;
								string output = String.Format("/* {0} */\n\"{1}\" = \"{2}\";\n\n",note, text,text);

								Console.WriteLine(text);
								sb.Append(output);

							}
						}
					}
	
				}
				string s = sb.ToString();
				File.AppendAllText("Localized.strings",s, Encoding.UTF8);
			}
			Console.WriteLine("File Localized.strings written.");
		}
	}
}

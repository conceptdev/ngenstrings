using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ngenstrings
{
	public class MethodSignature
	{
		private List<Parameter> _parameters, _parametersReversed;
		public string FullName { get; set; }
		public string ShortName { get; private set;}
		public int ParameterCount {get; private set;}
		public List<Parameter> Parameters
		{
			get { return _parameters; }
			set
			{
				_parameters = value;
				_parametersReversed = _parameters; //TODO: Clone() the list so this works (and fix ToString:ForEach as well)
				_parametersReversed.Reverse(); // HACK: this reverses the source list (obviously)
			}
		}

		/// <summary>
		/// Configure the method signature
		/// </summary>
		public MethodSignature (string fullName, List<Parameter> parameters)
		{
			FullName = fullName;
			Parameters = parameters;

			var a = fullName.IndexOf("::") + 2;
			var b = fullName.IndexOf("(");
			var c = fullName.IndexOf(")",b);
			ShortName = fullName.Substring(a, b - a);

			var p = fullName.Substring(b, c-b).Trim('(',')');
			var q = p.Split(',');
			
			ParameterCount = q.Length;

			if (parameters.Count != ParameterCount)
			{
				throw new ArgumentException(
					string.Format("The number of parameters specified ({0}) doesn't match the method signature ({1})"
					, parameters.Count, ParameterCount)
					,"parameters");
			}

		}

		/// <summary>
		/// Parse the "string" value of the parameter
		/// </summary>
		public LocalizedString Parse (Mono.Collections.Generic.Collection<Instruction> instructions, int positionOfMethod)
		{
			var ls = new LocalizedString();
			string source = "";
			for (int i = 0; i < _parametersReversed.Count; i++)
			{
				Instruction instruction = instructions[positionOfMethod - (i+1)];
				if ("ldstr" == instruction.OpCode.Name)
				{
					source = instruction.Operand as string;
					switch (_parametersReversed[i])
					{
						case Parameter.NSBundleIdentifier: 
							break;
						case Parameter.Key: 
							ls.Key = source;
							break;
						case Parameter.Value: 
							ls.Value = source;
							break;
						case Parameter.Comment: 
							ls.Comment = source;
							break;
						case Parameter.Table:
							ls.Table = source;
							break;
						case Parameter.Bundle:
							// not supported
							break;
						case Parameter.Ignore:
							// ignore
							break;
						default:
							throw new NotImplementedException(string.Format("Parameter type {0} not implemented in Parse()",Parameters[i].ToString()));
					}
				}
				else if ("stelem.ref" == instruction.OpCode.Name)
				{	// HACK: walk back through IL, looking for the "string" parameter
					// If I knew more about IL and Cecil then I would do this properly, but for now 
					// this works with Miguel's TweetStation Locale.Format(string, string[])
					for (int j = 0; j < 40; j++)
					{
						int k = positionOfMethod - (i+1) - j;
						if (k < 0 || k >= instructions.Count) break;
						var i1 = instructions[k];
						if ("InlineString" == i1.OpCode.OperandType.ToString())
						{
							source = i1.Operand.ToString();
							break; // there might be other strings if we keep walking back, use the first one we find
						}
					}
					// HACK: this is hardcoded to use the string as the 'key' (tsk tsk)
					ls.Key = source;	// TODO: Consider future method signatures with comment or other parameters
					break;
				}
			}
			return ls;
		}

		/// <summary>
		/// Console output (informational/debugging) display of method signature
		/// </summary>
		public override string ToString ()
		{
			var s = "";
			Parameters.ForEach( (p) => s = p +", " + s); // format and sort correctly
			return string.Format("   {0} ({1})", ShortName, s.Trim(',',' '));
		}
	}
}

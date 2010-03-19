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

		public LocalizedString Parse (Mono.Cecil.Cil.InstructionCollection instructions, int positionOfMethod)
		{
			var ls = new LocalizedString();
			var y = "";

			for (int i = 0; i < _parametersReversed.Count; i++) {
				y = instructions[positionOfMethod - (i+1)].Operand as string;

				switch (_parametersReversed[i])
				{
					case Parameter.NSBundleIdentifier: 
						break;
					case Parameter.Key: 
						ls.Key = y;
						break;
					case Parameter.Value: 
						ls.Value = y;
						break;
					case Parameter.Comment: 
						ls.Comment = y;
						break;
					case Parameter.Table:
						ls.Table = y;
						break;
					case Parameter.Bundle:
						// not supported
						break;
					default:
						throw new NotImplementedException(string.Format("Parameter type {0} not implemented in Parse()",Parameters[i].ToString()));
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
			Parameters.ForEach( (p) => s = p +", " + s);
			return string.Format("   {0} ({1})", ShortName, s.Trim(',',' '));
		}
	}
}

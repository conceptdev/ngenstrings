using System;
using System.Collections.Generic;

namespace ngenstrings
{
	public class MethodSignatureCollection : List<MethodSignature>
	{
		public MethodSignatureCollection ()
		{
		}

		public bool Matches (string candidate, out MethodSignature match)
		{
			foreach (MethodSignature item in this) {
				if (item.FullName == candidate) {
					match = item;
					return true;
				}
			}
			match = null;
			return false;
		}

	}
}

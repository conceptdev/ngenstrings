using System;
namespace ngenstrings
{
	public enum OutputFormat
	{
		/// <summary>[DEFAULT] c-style key-value pairs</summary>
		Strings
		, 
		/// <summary>PList key-value XML</summary>
		PList
		, 
		/// <summary>Serialized XML (custom format)</summary>
		Xml
	}
}


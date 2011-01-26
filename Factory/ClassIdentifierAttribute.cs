

using System;

namespace Deveck.Utils.Factory
{

	/// <summary>
	/// Provides a general attribute for attaching an identifier
	/// to a class
	/// </summary>
	public class ClassIdentifierAttribute : Attribute
	{
		private string _identifier;
		
		public string Identifier
		{
			get{ return _identifier; }
		}
		
		
		public ClassIdentifierAttribute (string identifier)
		{
			_identifier = identifier;
		}
	}
}

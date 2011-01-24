using System;
using System.Collections.Generic;
using System.Text;
using Deveck.Utils.StringUtils;

namespace Deveck.Utils.Samples
{
    class Program
    {
   		static Random r = new Random();
		
		public static void Main(string[] args)
        {
			
            Console.WriteLine(@"These samples show you how to use the SimpleFormatter from deveck.net");
            Console.WriteLine();

            SimpleFormatter f = new SimpleFormatter();
			Console.WriteLine("\tUse simple formatter like string.format: {0}", f.Format("{0} {1}", "Hello", "World"));
			Console.WriteLine("\tWith padding: {0}",
			                  f.Format("{0:LF:10:-}{1:R:10:*}", "Hello", "World"));
			Console.WriteLine("\tCentered to 40 chars: {0}", f.Format("{0:M:40:-}", "Hello World"));
			
			Console.WriteLine();
			Console.WriteLine("Registering macro \"hello\"");
			f.DefineTextMacro("hello", "HELLO");
			Console.WriteLine("Registering macro \"world\"");
			f.DefineTextMacro("world", "WoRlD");
			Console.WriteLine();
			
			Console.WriteLine("\tPadded output using macros: {0}", f.Format("{[hello]:LF:10:-}{[world]:R:10:*}"));
			Console.WriteLine("\tTruncating output:");
			Console.WriteLine(f.Format("\t\t{[hello]:L:2}"));
			Console.WriteLine(f.Format("\t\t{[hello]:L:3}"));
			Console.WriteLine(f.Format("\t\t{[hello]:L:4}"));
			Console.WriteLine();
			
			Console.WriteLine("\tPadded output using direct text input: {0}", f.Format("{\"hello\":LF:10:-}{\"world\":R:10:*}"));
			Console.WriteLine();
			Console.WriteLine();
			
			Console.WriteLine("\tNow we will reference a macro called \"universe\" several times\n\tthat has never been defined");
			f.OnGetParameter += OnGetParameter;
			Console.Write("\t");
			for(int i = 0; i< 10; i++)
			{
				if(i > 0)
					Console.Write(", ");
				Console.Write(f.Format("{[universe]}"));
			}
            Console.ReadLine();
        }

        private static string OnGetParameter (string parameterName)
        {
			StringBuilder universe = new StringBuilder();
			
			foreach(char c in "universe")
			{
				bool lowerCase = r.NextDouble() > 0.5;
				
				universe.Append(lowerCase?c.ToString(): c.ToString().ToUpper());
			}
			
			return universe.ToString();
        }

		
    }
}

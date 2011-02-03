using System;
using System.Collections.Generic;
using System.Text;
using Deveck.Utils.StringUtils;
using System.Reflection;
using Deveck.Utils.Factory;
using Deveck.Utils.SimpleComm;
using System.Threading;

namespace Deveck.Utils.Samples
{
    public class Program
    {
   		static Random r = new Random();
		
		public static void Main(string[] args)
        {
			Example_SimpleComm_HID();
			
            //Examples_SimpleFormatter();
			//Examples_ClassIdentifierFactory();
        }

		#region SimpleFormatter examples
		private static void Examples_SimpleFormatter()
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
		#endregion
		
		#region ClassIdentifier factory examples
		private static void Examples_ClassIdentifierFactory()
		{
		
			Console.WriteLine(@"These samples show you how to use the ClassIdentifierFactory from deveck.net");
            Console.WriteLine();
			
			Console.WriteLine("We are now creating 4 objects:");
			Console.WriteLine("\t1. Object of CIClass1 using its identifier c1");
			Console.WriteLine("\t2. Object of CIClass2 using its identifier c2");
			Console.WriteLine("\t3. Object of CIClass3 using its class name");
			Console.WriteLine("\t4. Object of identifier i1.....oh this identifier does not exists, so no instance");
			Console.WriteLine();
			CIBase o1 = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<CIBase>("c1", "class number 1");
			CIBase o2 = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<CIBase>("c2", "class number 2");
			CIBase o3 = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<CIBase>("Deveck.Utils.Samples.Program+CIClass3, Samples", "class number 3");
			CIBase o4 = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<CIBase>("i1", "invalid class");
			
			Console.WriteLine("Who is o1?: Hello from o1: '{0}'", o1);
			Console.WriteLine("Who is o2?: Hello from o2: '{0}'", o2);
			Console.WriteLine("Who is o3?: Hello from o3: '{0}'", o3);
			Console.WriteLine("Is there o4? {0}", o4==null?"NO":"Yes, there it is: '" + o4.ToString() + "', have you created a class with identifier i1?");
			
			
		}
		
		public abstract class CIBase
		{
			private string _name;
			
			public CIBase(string name)
			{
				_name = name;
			}
			
			public override string ToString ()
			{
				return _name;
			}
		}
		
		[ClassIdentifier("c1")]
		public class CIClass1 : CIBase
		{
			public CIClass1(string name)
				:base(name)
			{
			}
		}
		
		[ClassIdentifier("c2")]
		public class CIClass2 : CIBase
		{
			public CIClass2(string name)
				:base(name)
			{
			}
		}
		
		public class CIClass3 : CIBase
		{
			public CIClass3(string name)
				:base(name)
			{
			}
		}
		#endregion

		
		#region SimpleComm examples
		/// <summary>
		/// Shows how to use the HIDComm class to capture keyboard input 
		/// </summary>
		private static void Example_SimpleComm_HID()
		{
			ICommunication hidComm = 
				GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ICommunication>("simplecomm/win/hid");
			
			hidComm.SetupCommunication(null);
			
			while(true)
				Thread.Sleep(50);
		}
		
		#endregion
		
    }
}

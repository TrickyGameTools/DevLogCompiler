// Lic:
// 	DevLog Compiler
// 	
// 	
// 	
// 	
// 	(c) Jeroen P. Broks, 2018, All rights reserved
// 	
// 		This program is free software: you can redistribute it and/or modify
// 		it under the terms of the GNU General Public License as published by
// 		the Free Software Foundation, either version 3 of the License, or
// 		(at your option) any later version.
// 		
// 		This program is distributed in the hope that it will be useful,
// 		but WITHOUT ANY WARRANTY; without even the implied warranty of
// 		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// 		GNU General Public License for more details.
// 		You should have received a copy of the GNU General Public License
// 		along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 		
// 	Exceptions to the standard GNU license are available with Jeroen's written permission given prior 
// 	to the project the exceptions are needed for.
// Version: 18.11.16
// EndLic

using System;
using TrickyUnits;

namespace DevLogCompiler
{
    class MainClass
    {
        static string _workspace;

        static public string WorkSpace{ get{
                if (_workspace != "") return _workspace;
                var c = Dirry.C("$AppSupport$/.DevlogConfig.GINI");
                if (!System.IO.File.Exists(c)) { Console.WriteLine("No config!"); throw new Exception("No Config!"); }
                var g = GINI.ReadFromFile(c);
                _workspace = g.C("WORKSPACE");
                if (_workspace == "") { throw new Exception("Workspace not defined!"); }
                return _workspace;
            }}

        public static bool Yes(string Question){
            Console.Write($"{Question} ? <Y/N> ");
            var k = Console.ReadKey();
            return k.KeyChar == 'Y' || k.KeyChar == 'y';
        }

        public static void Main(string[] args)
        {
            Dirry.C("$AppSupport"); // Just forces MKL to be properly set :P
            QOpen.Hello();
            qstr.Chr(1);
            Compile.Hi();
            MKL.Lic    ("DevLog Compiler - DevLogCompiler.cs","GNU General Public License 3");
            MKL.Version("DevLog Compiler - DevLogCompiler.cs","18.11.16");
            Console.WriteLine($"DevLog - version {MKL.Newest}");
            Console.WriteLine($"(c) Jeroen P. Broks 2018-20{qstr.Left(MKL.Newest,2)}");
#if DEBUG
            Console.WriteLine("\n\nWARNING! This is the debug version!\nAs the name suggests this build should be used for debugging purposes ONLY!\n");
#endif
            if (args.Length==0){
                Console.WriteLine("\n\n");
                Console.WriteLine("DevLogCompiler c <project> - Compiles Project Into Binary form");
                Console.WriteLine("DevLogCompiler d <project> - Decompiles Project From Binary form back to text form");
                Console.WriteLine("DevlogCompiler v           - Full version information");
                return;
            } 
            switch (args[0].ToUpper()){
                case "C": if (args.Length != 2) { Console.WriteLine("Invalid command line input!"); return; }
                    Compile.Go($"{WorkSpace}/Projects/{args[1]}");
                    break;
                case "V": Console.WriteLine($"\n{MKL.All()}"); break;
                default: Console.WriteLine("Unknown command switch"); break;
            }
        }
    }
}

// Lic:
// 	Devlog Compiler
// 	Compile
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
// Version: 18.11.17
// EndLic

#define MaxLine    

using System;
using System.Text;
using System.Collections.Generic;
using TrickyUnits;
namespace DevLogCompiler
{
    class TIndex{
        public int id;
        public long size { get => data.Length; }
        public long offset;
        public string data;
    }

    public class Compile
    {
#if MaxLine
        const int MaxLine = 30000;
#endif
        //static readonly SortedDictionary<int, TIndex> Indexes = new SortedDictionary<int, TIndex>();
        static TIndex Index;
        static public void Hi(){
            MKL.Version("DevLog Compiler - Compile.cs","18.11.17");
            MKL.Lic    ("DevLog Compiler - Compile.cs","GNU General Public License 3");
        }
        static bool Yes(string Question) => MainClass.Yes(Question);

        static string GetLine(QuickStream bin){
            var ret = "";
            var wt = 0;
            var allow = true;
            while (true)
            {
                var x = bin.ReadByte();
                if (x == 10 && qstr.Left(ret, 1) == "#") ret = "";
                if (x == 10 && ret.Trim() != "") break;
                if (x == 10) allow = true;
                if (allow) ret += qstr.Chr((byte)x);
                if (bin.EOF) break;
                wt++;
                if (wt > 10000)
                {
                    //Console.WriteLine($"Is a string loading cycle this long normal?\n{ret.Length}>{ret}"); 
                    Process(bin);
                    wt = 0;
                }
#if MaxLine
                if (ret.Length>MaxLine){
                    Console.WriteLine($"WARNING! There's a line exceeding the maximum allowed size of {MaxLine} characters! Gonna ignore it!");
                    allow = false;
                    ret = "";
                }
                    #endif
            }
            Process(bin);
            return ret.Trim();
        }

        static void Process(QuickStream bt){
            Console.Write($"\t{Math.Round(((double)bt.Position / (double)bt.Size) * 100)}%\r");
        }


        static public void Go(string file){
            var inpfile = $"{file}.Entries";
            var idxfile = $"{file}.Index";
            var cntfile = $"{file}.Content";
            if (System.IO.File.Exists(idxfile) || System.IO.File.Exists(cntfile)) if (!Yes("Target already exists. Continue")) return;
            var bin = QOpen.ReadFile(inpfile);
            var bix = QOpen.WriteFile(idxfile);
            var bcn = QOpen.WriteFile(cntfile);
            var rec = 0;
            bcn.WriteString("DEVLOGPROJECTCONTENT\033", true);
            Console.WriteLine($"Compiling entries for project: {file}");
            while(!bin.EOF){
                var sline = GetLine(bin);
#if DEBUG
                Console.WriteLine($"Compiling line: {sline}");
#endif
                var p = sline.IndexOf(':');
                var key = sline.Trim().ToUpper();
                var value = "";
                if (p>-1){
                    key = sline.Substring(0, p).Trim().ToUpper();
                    value = sline.Substring(p + 1).Trim();
                }
                if (key=="NEW"){
                    if (Index != null) Console.WriteLine($"Warning! New Record ({value}) started while the old one ({Index.id}) wasn't properly pushed! This will lead to unreachable data!");
                    Index = new TIndex();
                    Index.id = qstr.ToInt(value);
                    Index.offset = bcn.Position;
                }
                if (key == "PUSH")
                {
                    if (Index == null) { Console.WriteLine("WARNING! Pushing without a created index! Expect an exception in three-two-one..."); }
                    bix.WriteByte(0);
                    bix.WriteInt(Index.id);
                    bix.WriteLong(Index.size);
                    bix.WriteLong(Index.offset);
                    bcn.WriteString(Index.data, true);
                    Index = null;
                } else if (sline=="") {
                    // Nothing happens, but this doesn't spook up the rest :P
                } else if (value=="") {
                    Console.WriteLine($"Invalid>{sline}");
                } else {
                    if (Index == null)
                    {
                        Console.WriteLine($"WARNING! Definition without a new index creation >{sline}<\nExpect an exception soon!");
#if DEBUG
                        Console.ReadKey();
#endif
                    }
                    rec++;
                    byte[] lkey = BitConverter.GetBytes(key.Length);
                    byte[] lval = BitConverter.GetBytes(value.Length);
                    byte[] bkey = Encoding.UTF8.GetBytes(key);
                    byte[] bval = Encoding.UTF8.GetBytes(value);
                    byte[][] bufs = { lkey, bkey, lval, bval };
                    bcn.WriteByte(0);
                    foreach (byte[] buf in bufs) foreach (byte b in buf) Index.data+=qstr.Chr(b);
                }                 
            }
            bin.Close();
            bix.Close();
            bcn.Close();
            if (rec == 1) {
                Console.WriteLine("\t1 record processed!");
            } else {
                Console.WriteLine($"{rec} records processed!");
            }
        }
    }
}

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
        //QuickStream IkWeetHetNietMeer;
        public int id=-10;
        public long size { get  { 
				long q=1;
				foreach(string k in D.Keys) q+=4+k.Length+D[k].Length;
				return q;
			}
		}
        public long offset=0;
        /*
        public string data //="";
        {
			get{
				if (IkWeetHetNietMeer==null) return "";
				IkWeetHetNietMeer.Close();
				IkWeetHetNietMeer=QOpen.ReadFile(Dirry.C("$AppSupport$/DVCTEMPFILE.BIN"));
				IkWeetHetNietMeer.Position=0;
				var ret= IkWeetHetNietMeer.ReadString((int)IkWeetHetNietMeer.Size);
				IkWeetHetNietMeer.Close();
				IkWeetHetNietMeer=null;
				return ret;
			}
		}
        
        public void AddNull(){
			if (IkWeetHetNietMeer==null) IkWeetHetNietMeer=QOpen.WriteFile(Dirry.C("$AppSupport$/DVCTEMPFILE.BIN"));
			IkWeetHetNietMeer.WriteByte(0);
		}
		
        public void AddData(string mystring){
			Console.WriteLine($"Writing {mystring}");
			if (IkWeetHetNietMeer==null) IkWeetHetNietMeer=QOpen.WriteFile(Dirry.C("$AppSupport$/DVCTEMPFILE.BIN"));
			IkWeetHetNietMeer.Position=IkWeetHetNietMeer.Size;
			IkWeetHetNietMeer.WriteString(mystring);
		}
		
		~TIndex(){
			if (IkWeetHetNietMeer!=null){
				IkWeetHetNietMeer.Close();
				System.IO.File.Delete(Dirry.C("$AppSupport$/DVCTEMPFILE.BIN"));
			}
		}
		*/
		SortedDictionary<string,string> D = new SortedDictionary<string,string>();
		public void Def(string key,string value){ D[key]=value; }
		public void DataWrite(QuickStream f){
			foreach(string key in D.Keys){
				f.WriteByte(0);
				f.WriteString(key);
				f.WriteString(D[key]);
				//Console.WriteLine($"Writing string {key} => {D[key]}");
			}
		}
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
                    Index.Def("ZZZZ","END");
                    Index.Def("ZZZA","END");
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
                    //bcn.WriteString(Index.data, true);
                    Index.DataWrite(bcn);
                    Index = null;
                } else if (sline=="") {
                    // Nothing happens, but this doesn't spook up the rest :P
                } else if (value=="" && qstr.Right(sline,1)!=":") {
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
                    /*
                    byte[] bkey = Encoding.UTF8.GetBytes(key);
                    byte[] bval = Encoding.UTF8.GetBytes(value);
                    byte[] lkey = BitConverter.GetBytes(bkey.Length);
                    byte[] lval = BitConverter.GetBytes(bval.Length);
                    byte[][] bufs = { lkey, bkey, lval, bval };
                    int[] bsize = {4,bkey.Length,4,bval.Length};
                    if (lkey.Length!=4) Console.WriteLine($"Invalid key length {lkey.Length}");
                    if (lval.Length!=4) Console.WriteLine($"Invalid value length {lval.Length}");
                    int old=0;
                    if (Index.data != "" && Index.data!=null)
                    {
                        old = 
                            Index.data.Length;
                    }
                    //Console.WriteLine($"{key.Length} {key} => {value.Length} {value}");
                    Index.data+=qstr.Chr(0); //bcn.WriteByte(0);
                    var ci=-1;
                    foreach (byte[] buf in bufs) {
						ci++;
						var chk=0;
						foreach (byte b in buf) {
							if (buf.Length!=4 && (b<32 || b>127)) {
								Console.WriteLine($"{Index.id}: WARNING! I do not trust character #{b}({qstr.Chr(b)}) that is now in the output! {key}='{value}' (replaced with '?')");
								Index.data+="?";
								chk++;
							} else { 
								Index.data+=qstr.Chr(b);
								chk++;
							}
						}
						if (chk!=bsize[ci]) Console.WriteLine($"{Index.id}: WARNING! Buffer oddity {chk}!={bsize[ci]} ({ci})");						
					}
                    var mustbe = lkey.Length + lval.Length + bkey.Length + bval.Length + old +1;
                    if (Index.data.Length!=mustbe) Console.WriteLine($"{Index.id}: WARNING! Buffer output sizes are NOT correct!   {Index.data.Length}!={mustbe}");
					//Index.size=Index.data.Length;
					// */
					//Index.AddData(key);
					//Index.AddData(value);
					Index.Def(key,value);
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

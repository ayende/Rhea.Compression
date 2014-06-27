using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhea.Compression;
using Rhea.Compression.Debug;
using Rhea.Compression.Dictionary;

namespace Rhea.Tryouts
{
    class Program
    {
        static void Main(string[] args)
        {
            //var dic = new DictionaryOptimizer();

            //dic.Add("{'id':1,'name':'Ryan Peterson','country':'Northern Mariana Islands','email':'rpeterson@youspan.mil'");
            //dic.Add("{'id':2,'name':'Judith Mason','country':'Puerto Rico','email':'jmason@quatz.com'");
            //dic.Add("{'id':3,'name':'Kenneth Berry','country':'Pakistan','email':'kberry@wordtune.mil'");
            //dic.Add("{'id':4,'name':'Judith Ortiz','country':'Cuba','email':'jortiz@snaptags.edu'");
            //dic.Add("{'id':5,'name':'Adam Lewis','country':'Poland','email':'alewis@muxo.mil'");
            //dic.Add("{'id':6,'name':'Angela Spencer','country':'Poland','email':'aspencer@jabbersphere.info'");
            //dic.Add("{'id':7,'name':'Jason Snyder','country':'Cambodia','email':'jsnyder@voomm.net'");
            //dic.Add("{'id':8,'name':'Pamela Palmer','country':'Guinea-Bissau','email':'ppalmer@rooxo.name'");
            //dic.Add("{'id':9,'name':'Mary Graham','country':'Niger','email':'mgraham@fivespan.mil'");
            //dic.Add("{'id':10,'name':'Christopher Brooks','country':'Trinidad and Tobago','email':'cbrooks@blogtag.name'");
            //dic.Add("{'id':11,'name':'Anna West','country':'Nepal','email':'awest@twinte.gov'");
            //dic.Add("{'id':12,'name':'Angela Watkins','country':'Iceland','email':'awatkins@izio.com'");
            //dic.Add("{'id':13,'name':'Gregory Coleman','country':'Oman','email':'gcoleman@browsebug.net'");
            //dic.Add("{'id':14,'name':'Andrew Hamilton','country':'Ukraine','email':'ahamilton@rhyzio.info'");
            //dic.Add("{'id':15,'name':'James Patterson','country':'Poland','email':'jpatterson@skippad.net'");
            //dic.Add("{'id':16,'name':'Patricia Kelley','country':'Papua New Guinea','email':'pkelley@meetz.biz'");
            //dic.Add("{'id':17,'name':'Annie Burton','country':'Germany','email':'aburton@linktype.com'");
            //dic.Add("{'id':18,'name':'Margaret Wilson','country':'Saudia Arabia','email':'mwilson@brainverse.mil'");
            //dic.Add("{'id':19,'name':'Louise Harper','country':'Poland','email':'lharper@skinder.info'");
            //dic.Add("{'id':20,'name':'Henry Hunt','country':'Martinique','email':'hhunt@thoughtstorm.org'");

            //var optimize = dic.Optimize(512);

            //Console.WriteLine("Suffixes");
            //dic.DumpSuffixes(Console.Out);

            //Console.WriteLine("Substrings");
            //dic.DumpSubstrings(Console.Out);

            //Console.WriteLine("Dictionary");
            //Console.WriteLine(Encoding.UTF8.GetString(optimize));

            var dic = Encoding.UTF8.GetBytes("asonerryson@eterson','.mil'ame':'{'id':','country':'P','email':','country':'");

            var text = Encoding.UTF8.GetBytes("{'id':11,'name':'Anna Nepal','country':'Nepal','email':'awest@twinte.gov'}");

            var substringPacker = new SubstringPacker(dic);
            var writer = new StringWriter();
            substringPacker.Pack(text, new DebugPackerOutput(), writer);

            var s = writer.GetStringBuilder().ToString();

            Console.WriteLine(s);

            var substringUnpacker = new SubstringUnpacker(dic);
            var debugUnpackerOoutput = new DebugUnpackerOoutput(
                new StringReader(s),
                substringUnpacker);
            debugUnpackerOoutput.Unpack();

            var uncompressedData = substringUnpacker.UncompressedData();

            Console.WriteLine(Encoding.UTF8.GetString(uncompressedData.Array,uncompressedData.Offset, uncompressedData.Count));
            Console.WriteLine(Encoding.UTF8.GetString(text));
        }
    }
}

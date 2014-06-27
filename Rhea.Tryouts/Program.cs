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
			var trainCompression = new CompressionTrainer();

			trainCompression.TrainOn("{'id':1,'name':'Ryan Peterson','country':'Northern Mariana Islands','email':'rpeterson@youspan.mil'");
			trainCompression.TrainOn("{'id':2,'name':'Judith Mason','country':'Puerto Rico','email':'jmason@quatz.com'");
			trainCompression.TrainOn("{'id':3,'name':'Kenneth Berry','country':'Pakistan','email':'kberry@wordtune.mil'");
			trainCompression.TrainOn("{'id':4,'name':'Judith Ortiz','country':'Cuba','email':'jortiz@snaptags.edu'");
			trainCompression.TrainOn("{'id':5,'name':'Adam Lewis','country':'Poland','email':'alewis@muxo.mil'");
			trainCompression.TrainOn("{'id':6,'name':'Angela Spencer','country':'Poland','email':'aspencer@jabbersphere.info'");
			trainCompression.TrainOn("{'id':7,'name':'Jason Snyder','country':'Cambodia','email':'jsnyder@voomm.net'");
			trainCompression.TrainOn("{'id':8,'name':'Pamela Palmer','country':'Guinea-Bissau','email':'ppalmer@rooxo.name'");
			trainCompression.TrainOn("{'id':9,'name':'Mary Graham','country':'Niger','email':'mgraham@fivespan.mil'");
			trainCompression.TrainOn("{'id':10,'name':'Christopher Brooks','country':'Trinidad and Tobago','email':'cbrooks@blogtag.name'");
			trainCompression.TrainOn("{'id':11,'name':'Anna West','country':'Nepal','email':'awest@twinte.gov'");
			trainCompression.TrainOn("{'id':12,'name':'Angela Watkins','country':'Iceland','email':'awatkins@izio.com'");
			trainCompression.TrainOn("{'id':13,'name':'Gregory Coleman','country':'Oman','email':'gcoleman@browsebug.net'");
			trainCompression.TrainOn("{'id':14,'name':'Andrew Hamilton','country':'Ukraine','email':'ahamilton@rhyzio.info'");
			trainCompression.TrainOn("{'id':15,'name':'James Patterson','country':'Poland','email':'jpatterson@skippad.net'");
			trainCompression.TrainOn("{'id':16,'name':'Patricia Kelley','country':'Papua New Guinea','email':'pkelley@meetz.biz'");
			trainCompression.TrainOn("{'id':17,'name':'Annie Burton','country':'Germany','email':'aburton@linktype.com'");
			trainCompression.TrainOn("{'id':18,'name':'Margaret Wilson','country':'Saudia Arabia','email':'mwilson@brainverse.mil'");
			trainCompression.TrainOn("{'id':19,'name':'Louise Harper','country':'Poland','email':'lharper@skinder.info'");
			trainCompression.TrainOn("{'id':20,'name':'Henry Hunt','country':'Martinique','email':'hhunt@thoughtstorm.org'");


	        var compressionHandler = trainCompression.CreateHandler(1024);

	        var memoryStream = new MemoryStream();
	        var text = "{'id':52,'name':'Christina Murray','country':'Hong Kong','email':'cmurray@brainsphere.gov'}";
	        var result = compressionHandler.Compress(text, memoryStream);

	        Console.WriteLine(text.Length);
	        Console.WriteLine(result);
	        Console.WriteLine(memoryStream.Length);
        }
    }
}

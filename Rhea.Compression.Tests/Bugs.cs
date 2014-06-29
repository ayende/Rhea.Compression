using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rhea.Compression.Tests
{
    public class Bugs
    {
        [Fact]
        public void CanCompressRepeatedChars()
        {
            var docs = new[]
            {
                "{'id':1,'name':'Ryan Peterson','country':'Northern Mariana Islands','email':'rpeterson@youspan.mil'}",
                "{'id':2,'name':'Judith Mason','country':'Puerto Rico','email':'jmason@quatz.com'}",
                "{'id':3,'name':'Kenneth Berry','country':'Pakistan','email':'kberry@wordtune.mil'}",
                "{'id':4,'name':'Judith Ortiz','country':'Cuba','email':'jortiz@snaptags.edu'}",
                "{'id':5,'name':'Adam Lewis','country':'Poland','email':'alewis@muxo.mil'}",
                "{'id':6,'name':'Angela Spencer','country':'Poland','email':'aspencer@jabbersphere.info'}",
                "{'id':7,'name':'Jason Snyder','country':'Cambodia','email':'jsnyder@voomm.net'}",
                "{'id':8,'name':'Pamela Palmer','country':'Guinea-Bissau','email':'ppalmer@rooxo.name'}",
                "{'id':9,'name':'Mary Graham','country':'Niger','email':'mgraham@fivespan.mil'}",
                "{'id':10,'name':'Christopher Brooks','country':'Trinidad and Tobago','email':'cbrooks@blogtag.name'}",
                "{'id':11,'name':'Anna West','country':'Nepal','email':'awest@twinte.gov'}",
                "{'id':12,'name':'Angela Watkins','country':'Iceland','email':'awatkins@izio.com'}",
                "{'id':13,'name':'Gregory Coleman','country':'Oman','email':'gcoleman@browsebug.net'}",
                "{'id':14,'name':'Andrew Hamilton','country':'Ukraine','email':'ahamilton@rhyzio.info'}",
                "{'id':15,'name':'James Patterson','country':'Poland','email':'jpatterson@skippad.net'}",
                "{'id':16,'name':'Patricia Kelley','country':'Papua New Guinea','email':'pkelley@meetz.biz'}",
                "{'id':17,'name':'Annie Burton','country':'Germany','email':'aburton@linktype.com'}",
                "{'id':18,'name':'Margaret Wilson','country':'Saudia Arabia','email':'mwilson@brainverse.mil'}",
                "{'id':19,'name':'Louise Harper','country':'Poland','email':'lharper@skinder.info'}",
                "{'id':20,'name':'Henry Hunt','country':'Martinique','email':'hhunt@thoughtstorm.org'}"

            };

            var trainer = new CompressionTrainer();

            foreach (var doc in docs)
            {
                trainer.TrainOn(doc);
            }

            var compressionHandler = trainer.CreateHandler();

            var memoryStream = new MemoryStream();
            var text = "{'id':11111,'name':'Albert Perry','country':'Congo, Republic of','email':'aperry@demimbu.info'}";
            compressionHandler.Compress(text,memoryStream);
            var compressDebug = compressionHandler.CompressDebug(text);
            var d = compressionHandler.DecompressDebug(compressDebug);

            Assert.Equal(text, d);

            memoryStream.Position = 0;
            var result = compressionHandler.Decompress(memoryStream);

            var s = Encoding.UTF8.GetString(result);

            Assert.Equal(text, s);
        }

    }
}

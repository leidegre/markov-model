using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace MarkovModel
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Random r3;

            var tempFile = Path.GetTempFileName();
            try
            {
                var mc = new MarkovChain(3);

                mc.Add("a");
                mc.Add("as");
                mc.Add("asd");
                mc.Add("asd1");
                mc.Add("asd12");
                mc.Add("asd123");

                mc.Prepare();

                r3 = new Random(3);

                var list = new List<string>();

                list.Add(mc.Generate(r3));
                list.Add(mc.Generate(r3));
                list.Add(mc.Generate(r3));

                //

                mc.WriteToFile(tempFile);

                var mc2 = new MarkovChain();

                mc2.ReadFromFile(tempFile);

                r3 = new Random(3);

                var list2 = new List<string>();

                list2.Add(mc2.Generate(r3));
                list2.Add(mc2.Generate(r3));
                list2.Add(mc2.Generate(r3));

                CollectionAssert.AreEqual(list, list2);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

using System.Text;
using System.Xml.Linq;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;
using NUnit.Framework;

namespace Moor.XmlConversionLibrary.XmlToCsvTest
{
    [TestFixture]
    public class XmlToCsvUsingLinqTest
    {
        private XmlToCsvContext _xmlToCsvContext;

        [Test]
        public void TestInstantiation()
        {
            _xmlToCsvContext = new XmlToCsvContext(new XmlToCsvUsingLinq(@"C:\Temp\IDPoor.xml"));

            foreach (string s in _xmlToCsvContext.Strategy.TableNameCollection)
            {
                _xmlToCsvContext.Execute(s, @"C:\Documents and Settings\Tim\Desktop\ALLTABLES\" + s + ".csv", Encoding.Unicode);
            }
        }
    }
}

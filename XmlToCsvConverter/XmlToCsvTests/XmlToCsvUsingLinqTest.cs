using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;

namespace XmlToCsvTests
{
    [TestClass]
    public class XmlToCsvUsingLinqTest
    {
        private XmlToCsvContext _xmlToCsvContext;

        [TestMethod]
        public void TestInstantiation()
        {
            _xmlToCsvContext = new XmlToCsvContext(new XmlToCsvUsingLinq(@"C:\Payslip.xml"));

            foreach (string s in _xmlToCsvContext.Strategy.TableNameCollection)
            {
                _xmlToCsvContext.Execute(s, @"C:\" + s + ".csv", Encoding.Unicode);
            }
        }
    }
}

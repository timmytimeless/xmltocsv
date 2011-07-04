using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;

namespace XmlToCsvTests
{
    [TestClass]
    public class XmlToCsvUsingLinqTest
    {

        [TestMethod]
        public void TestDataSetImplementation()
        {
            XmlToCsvContext context = new XmlToCsvContext(new XmlToCsvUsingDataSet(@"C:\Payslip.xml"));

            foreach (string s in context.Strategy.TableNameCollection)
            {
                context.Execute(s, @"C:\" + s + ".csv", Encoding.Unicode);
            }
        }

        [TestMethod]
        public void ConvertUsingLinq()
        {
            var converter = new XmlToCsvUsingLinq(@"C:\Payslip.xml");
            var context = new XmlToCsvContext(converter);

            foreach (string xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"C:\" + xmlTableName + ".csv", Encoding.Unicode);
            }
        }
    }
}

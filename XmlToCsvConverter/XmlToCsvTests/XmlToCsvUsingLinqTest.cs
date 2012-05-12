using System.Data;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;

namespace XmlToCsvTests
{
    [TestClass]
    public class XmlToCsvUsingLinqTest
    {

        [TestMethod]
        [DeploymentItem(@"data.xml")]
        public void DataSetImplementationTest()
        {
            const string path = @"data.xml";
            var converter = new XmlToCsvUsingDataSet(path);
            var context = new XmlToCsvContext(converter);

            foreach (string xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"" + xmlTableName + ".csv", Encoding.Unicode);
            }
        }

        [TestMethod]
        [DeploymentItem(@"data.xml")]
        public void LinqImplementationTest()
        {
            const string path = @"data.xml";
            var converter = new XmlToCsvUsingLinq(path);
            var context = new XmlToCsvContext(converter);

            foreach (string xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"" + xmlTableName + ".csv", Encoding.Unicode);
            }
        }

        [TestMethod]
        [DeploymentItem(@"data.xml")]
        [ExpectedException(typeof(DuplicateNameException))]
        public void DuplicateNameErrorTest()
        {
            const string path = @"ErrorDuplicateName.xml";
            new XmlToCsvUsingDataSet(path);
        }
    }
}

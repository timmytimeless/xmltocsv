using System.Data;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;

namespace XmlToCsvTests
{
    [TestClass]
    public class XmlToCsvTests
    {
        [TestMethod]
        public void DataSetImplementationTest()
        {
            const string path = @"data.xml";
            var converter = new XmlToCsvUsingDataSet(path);
            var context = new XmlToCsvContext(converter);
            Assert.AreEqual(1, context.Strategy.TableNameCollection.Count);
            foreach (string xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"dataDataSet_" + xmlTableName + ".csv", Encoding.Default);
            }
        }

        [TestMethod]
        public void LinqImplementationTest()
        {
            const string path = @"data.xml";
            var converter = new XmlToCsvUsingLinq(path);
            var context = new XmlToCsvContext(converter);
            Assert.AreEqual(1, context.Strategy.TableNameCollection.Count);
            foreach (string xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"dataLinq_" + xmlTableName + ".csv", Encoding.Default);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateNameException))]
        public void DuplicateNameErrorTest()
        {
            const string path = @"ErrorDuplicateName.xml";
            var converter = new XmlToCsvUsingDataSet(path);
            var context = new XmlToCsvContext(converter);

            Assert.AreEqual(1, context.Strategy.TableNameCollection.Count);
            foreach (string xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"ErrorDuplicateName_" + xmlTableName + ".csv", Encoding.Default);
            }
        }

        [TestMethod]
        public void DuplicateNameRenamedTest()
        {
            const string path = @"ErrorDuplicateName.xml";
            var converter = new XmlToCsvUsingDataSet(path, true);
            var context = new XmlToCsvContext(converter);
            Assert.AreEqual(2, context.Strategy.TableNameCollection.Count);

            foreach (string xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"ErrorDuplicateName_" + xmlTableName + ".csv", Encoding.Default);
            }
        }
    }
}

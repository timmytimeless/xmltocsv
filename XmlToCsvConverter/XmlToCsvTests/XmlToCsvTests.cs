using System;
using System.Data;
using System.Text;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;
using NUnit.Framework;

namespace XmlToCsvTests
{

    public class XmlToCsvTests
    {
        [Test]
        public void DataSetImplementationTest()
        {
            const string path = @"TestData/data.xml";
            var context = new XmlToCsvContext(new XmlToCsvUsingDataSet(path));
            Assert.That(context.Strategy.TableNameCollection.Count, Is.EqualTo(1));
            foreach (var xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"dataDataSet_" + xmlTableName + ".csv", Encoding.Default);
            }
        }

        [Test]
        public void CanReadAttributeTest()
        {
            const string path = @"TestData/DataWithAttributes.xml";
            var context = new XmlToCsvContext(new XmlToCsvUsingDataSet(path));
            Assert.That(context.Strategy.TableNameCollection.Count, Is.EqualTo(2));

            foreach (var xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"TestData/dataDataSet_" + xmlTableName + ".csv", Encoding.Default);
            }
        }

        [Test]
        public void DoubleQuoteEscapeTest()
        {
            const string path = @"TestData/DoubleQuoteEscape.xml";
            var context = new XmlToCsvContext(new XmlToCsvUsingDataSet(path));
            Assert.That(context.Strategy.TableNameCollection.Count, Is.EqualTo(1));

            foreach (var xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, string.Format(@"TestData/dataDataSet_{0}.csv", xmlTableName), Encoding.Default);
            }
        }

        [Test]
        public void InvalidOperationNestedTableExceptionTest()
        {
            const string path = @"TestData/NestedDataError.xml";
            TestHelper.Throws<InvalidOperationException>(() => new XmlToCsvUsingDataSet(path),
                "Nested table 'Contact-Address' which inherits its namespace cannot have multiple parent tables in different namespaces.");
        }

        [Test]
        public void LinqImplementationTest()
        {
            const string path = @"TestData/data.xml";
            var converter = new XmlToCsvUsingLinq(path);
            var context = new XmlToCsvContext(converter);
            Assert.That(context.Strategy.TableNameCollection.Count, Is.EqualTo(1));
            foreach (var xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"TestData/dataLinq_" + xmlTableName + ".csv", Encoding.Default);
            }
        }

        [Test]
        public void DuplicateNameErrorTest()
        {
            const string path = @"TestData/ErrorDuplicateName.xml";
            TestHelper.Throws<DuplicateNameException>(() => new XmlToCsvUsingDataSet(path), "A column named 'Employees' already belongs to this DataTable: cannot set a nested table name to the same name.");
        }

        [Test]
        public void DuplicateNameRenamedTest()
        {
            const string path = @"TestData/ErrorDuplicateName.xml";
            var context = new XmlToCsvContext(new XmlToCsvUsingDataSet(path, true));
            Assert.AreEqual(2, context.Strategy.TableNameCollection.Count);

            foreach (var xmlTableName in context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, @"ErrorDuplicateName_" + xmlTableName + ".csv", Encoding.Default);
            }
        }
    }
}

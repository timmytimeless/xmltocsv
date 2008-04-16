using System;

namespace Moor.XmlConversionLibrary.XmlToCsvStrategy
{
    public class XmlToCsvUsingLinq : XmlToCsvStrategyBase
    {
        public override void ExportToCsv(string xmlTableName, string csvDestinationFilePath)
        {
            throw new NotImplementedException("ExportToCsv to CSV using linq is not implemented");

            ///// <summary>
            ///// Export all data using Linq to XML
            ///// </summary>
            //public void ExportToCsvWithLinq()
            //{
            //    XDocument doc = XDocument.Load(@XmlFilePath);

            //    var nodes = from node in doc.Descendants("Household")
            //                select new
            //                {
            //                    DataCollectionRound = node.Element("DataCollectionRound").Value,
            //                    RegionCode = node.Element("RegionCode").Value,
            //                    HouseholdCode = node.Element("HouseholdCode").Value,
            //                    YearOfDataCollection = node.Element("YearOfDataCollection").Value,
            //                    DateOfDataEntry = node.Element("DateOfDataEntry").Value,
            //                    NameDataEntryPerson = node.Element("NameDataEntryPerson").Value,
            //                    NameDataEntryPersonUnicode = node.Element("NameDataEntryPersonU").Value,
            //                    PoorLevel = node.Element("PoorLevel").Value,
            //                };


            //    StreamWriter file = File.CreateText(@CsvFilePath);

            //    foreach (var node in nodes)
            //    {
            //        file.WriteLine(
            //            node.DataCollectionRound + "," +
            //            node.RegionCode + "," +
            //            node.HouseholdCode + "," +
            //            node.YearOfDataCollection + "," +
            //            node.DateOfDataEntry + ",\"" +
            //            node.NameDataEntryPerson + "\",\"" +
            //            node.NameDataEntryPersonUnicode + "\"," +
            //            node.PoorLevel + ","
            //            );
            //    }
            //}
        }


    }
}
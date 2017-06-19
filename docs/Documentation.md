<div class="wikidoc">

This project contains a library that you can use to convert data stored in XML to Comma Seperated Values. There is also a client application included. The project is built with C#4.0\. Currently there is one algorithm for the conversion implemented (using strategy pattern), namely the XmlToCsvUsingDataSet implementation. This conversion algorithm will work with any XML that is the result of an ADO.Net DataSet.SaveAsXml action. However, I believe the converter is should work with any tabular shaped XML data. Other implementations of the same conversion can easily be added by implementing for example an XmlToCsvUsingLinq strategy. In fact, the code skeleton for a XmlToCsvUsingLinq stretegy is included for future implementation, but be aware that at the moment it will throw a NotImplementedException.  

For large datasets, the current XmlToCsvUsingDataSet works pretty fast (it parses and saves 28 enities/tables to a CSV file from a 133 mb xml source file in just over 10 seconds).

Code usage example:

<div style="color:black; background-color:white">

<pre>       <span style="color:blue">public</span> <span style="color:blue">void</span> ConvertUsingDataSet()
        {
            <span style="color:blue">var</span> converter = <span style="color:blue">new</span> XmlToCsvUsingDataSet(<span style="color:#a31515">@"C:\Payslip.xml"</span>);
            <span style="color:blue">var</span> context = <span style="color:blue">new</span> XmlToCsvContext(converter);

            <span style="color:blue">foreach</span> (<span style="color:blue">string</span> xmlTableName <span style="color:blue">in</span> context.Strategy.TableNameCollection)
            {
                context.Execute(xmlTableName, <span style="color:#a31515">@"C:\"</span> + xmlTableName + <span style="color:#a31515">".csv"</span>, Encoding.Unicode);
            }
        }
</pre>

<pre>**Example execution of the command line tool from the command prompt:**

<div style="color:black; background-color:white">

<pre>XmlToCsv.Console.exe <span style="color:gray">-</span>xml c:\payslip.xml <span style="color:gray">-</span>dir c:\convertedcsvfiles
</pre>

</div>

 **To get the command line tool help screen:**   

<div style="color:black; background-color:white">

<pre>XmlToCsv.Console.exe <span style="color:gray">-</span>help 
</pre>

</div>

</pre>

</div>

</div>

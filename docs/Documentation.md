<div class="wikidoc">

This project contains a library that you can use to convert data stored in XML to Comma Seperated Values. There is also a client application included. The project is built with C#4.0\. The converter uses the XmlToCsvConverter implementation. This conversion algorithm will work with any XML that is the result of an ADO.Net DataSet.SaveAsXml action. However, I believe the converter is should work with any tabular shaped XML data.

For large datasets, the current XmlToCsvConverter works pretty fast (it parses and saves 28 enities/tables to a CSV file from a 133 mb xml source file in just over 10 seconds).

Code usage example:

<div style="color:black; background-color:white">

<pre>       <span style="color:blue">public</span> <span style="color:blue">void</span> ConvertUsingDataSet()
        {
            <span style="color:blue">var</span> converter = <span style="color:blue">new</span> XmlToCsvConverter(<span style="color:#a31515">@"C:\Payslip.xml"</span>);

            <span style="color:blue">foreach</span> (<span style="color:blue">string</span> tableName <span style="color:blue">in</span> converter.TableNames)
            {
                converter.Export(tableName, <span style="color:#a31515">@"C:\"</span> + tableName + <span style="color:#a31515">".csv"</span>, Encoding.Unicode);
            }
        }
</pre>

<pre>**Example execution of the command line tool from the command prompt:**

<div style="color:black; background-color:white">

<pre>XmlToCsv.Console.exe <span style="color:gray">-</span>xml c:\payslip.xml <span style="color:gray">-</span>dir c:\convertedcsvfiles <span style="color:gray">-</span>encoding utf-8
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

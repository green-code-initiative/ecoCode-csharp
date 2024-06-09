namespace EcoCode.ToolNetFramework.Reports;

internal sealed class HtmlReport : BaseReport
{
    private const string HtmlHeader = """
        <html>
        <head>
            <style>
                table {width: 100%; border-collapse: collapse;}
                th, td {border: 1px solid black; padding: 8px; text-align: left;}
                th {background-color: #f2f2f2;}
            </style>
        </head>
        <body>
            <h1>EcoCode analyzer report</h1>
            <table>
                <tr>
                    <th>Directory</th>
                    <th>File</th>
                    <th>Location</th>
                    <th>Severity</th>
                    <th>Code</th>
                    <th>Message</th>
                </tr>
        """;

    private const string HtmlFooter = """
            </table>
        </body>
        </html>
        """;

    protected override void WriteToStream(StreamWriter writer)
    {
        writer.WriteLine(HtmlHeader);
        foreach (var diagnostic in Diagnostics)
        {
            writer.WriteLine("      <tr>");
            writer.WriteLine($"         <td>{diagnostic.Directory}</td>");
            writer.WriteLine($"         <td>{diagnostic.File}</td>");
            writer.WriteLine($"         <td>{diagnostic.Location}</td>");
            writer.WriteLine($"         <td>{diagnostic.Severity}</td>");
            writer.WriteLine($"         <td>{diagnostic.Code}</td>");
            writer.WriteLine($"         <td>{diagnostic.Message}</td>");
            writer.WriteLine("      </tr>");
        }
        writer.WriteLine(HtmlFooter);
    }
}

namespace EcoCode.Tool.Reports;

internal sealed class HtmlAnalysisReport : AnalysisReport
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
        foreach (var diag in Diagnostics)
        {
            writer.WriteLine("      <tr>");
            writer.WriteLine($"         <td>{diag.Directory}</td>");
            writer.WriteLine($"         <td>{diag.File}</td>");
            writer.WriteLine($"         <td>{diag.Location}</td>");
            writer.WriteLine($"         <td>{diag.Severity}</td>");
            writer.WriteLine($"         <td>{diag.Code}</td>");
            writer.WriteLine($"         <td>{diag.Message}</td>");
            writer.WriteLine("      </tr>");
        }
        writer.WriteLine(HtmlFooter);
    }
}

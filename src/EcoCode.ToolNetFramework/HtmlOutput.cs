using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EcoCode.ToolNetFramework;

internal sealed class HtmlReport : IAnalyzerReport
{
    private readonly List<DiagnosticInfo> _diagnosticInfos = [];

    public void Add(DiagnosticInfo diagnosticInfo) => _diagnosticInfos.Add(diagnosticInfo);

    public void Generate(string outputPath)
    {
        var sb = new StringBuilder("<html><head><style>")
            .AppendLine("<html><head><style>")
            .AppendLine("table {width: 100%; border-collapse: collapse;}")
            .AppendLine("th, td {border: 1px solid black; padding: 8px; text-align: left;}")
            .AppendLine("th {background-color: #f2f2f2;}")
            .AppendLine("</style></head><body>")
            .AppendLine("<h1>EcoCode analyzer report</h1>")
            .AppendLine("<table>")
            .AppendLine("<tr><th>Path</th><th>File</th><th>Location</th><th>Severity</th><th>Code</th><th>Message</th></tr>");

        foreach (var diagnosticInfo in _diagnosticInfos)
        {
            _ = sb.Append("<tr>")
                .Append($"<td>{diagnosticInfo.Path}</td>")
                .Append($"<td>{diagnosticInfo.File}</td>")
                .Append($"<td>{diagnosticInfo.Location}</td>")
                .Append($"<td>{diagnosticInfo.Severity}</td>")
                .Append($"<td>{diagnosticInfo.Code}</td>")
                .Append($"<td>{diagnosticInfo.Message}</td>")
                .AppendLine("</tr>");
        }

        File.WriteAllText(outputPath, sb.AppendLine("</table></body></html>").ToString());
    }
}

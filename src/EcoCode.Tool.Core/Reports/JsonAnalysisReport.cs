using System.Text.Json;

namespace EcoCode.Tool.Core.Reports;

internal sealed class JsonAnalysisReport : AnalysisReport
{
    protected override void WriteToStream(StreamWriter writer)
    {
        writer.WriteLine("[");
        for (int i = 0; i < Diagnostics.Count - 1; i++)
            writer.WriteLine(JsonSerializer.Serialize(Diagnostics[i]) + ',');
        writer.WriteLine(JsonSerializer.Serialize(Diagnostics[Diagnostics.Count - 1]));
        writer.WriteLine("]");
    }
}

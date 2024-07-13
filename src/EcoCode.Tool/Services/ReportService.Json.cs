using System.Text.Json;

namespace EcoCode.Tool.Services;

static partial class ReportService
{
    private static class Json
    {
        public static async Task WriteToStreamAsync(StreamWriter writer, List<DiagnosticInfo> diagnostics)
        {
            await writer.WriteLineAsync("[");

            for (int i = 0; i < diagnostics.Count - 1; i++)
                await writer.WriteLineAsync(JsonSerializer.Serialize(diagnostics[i]) + ',');

            await writer.WriteLineAsync(JsonSerializer.Serialize(diagnostics[^1]));

            await writer.WriteLineAsync("]");
        }
    }
}

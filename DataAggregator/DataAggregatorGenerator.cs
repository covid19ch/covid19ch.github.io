using CovidStatsCH.Components;
using ExcelDataReader;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace DataAggregator
{
    [Generator]
    public partial class DataAggregatorGenerator : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor FileProcessed = new DiagnosticDescriptor(
            id: "DA0001",
            title: "File processed",
            messageFormat: "File {0} processed.",
            category: "Build",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: ".");
        private static readonly DiagnosticDescriptor Compressed = new DiagnosticDescriptor(
            id: "DA0002",
            title: "Data compressed",
            messageFormat: "{0} bytes compressed to {1} bytes ({2}%)",
            category: "Build",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: ".");
        private static readonly DiagnosticDescriptor Error = new DiagnosticDescriptor(
            id: "DA9999",
            title: "Exception in SG",
            messageFormat: "Exception: {0}, StackTrace: {1}",
            category: "Build",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: ".");
        public void Execute(GeneratorExecutionContext context)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (context.AdditionalFiles == null)
            {
                return;
            }
            try
            {
                var files = context.AdditionalFiles.Where(i => i.Path.EndsWith("xlsx")).OrderBy(f => f.Path).ToList();
                context.ReportDiagnostic(Diagnostic.Create(FileProcessed, null, files.Count));
                var dates = new HashSet<DateTime>();
                var All = new Dictionary<DateTime, List<DataPoint>>();
                foreach (var input_file in files)
                {
                    using var stream = File.OpenRead(input_file.Path);
                    using var reader = ExcelReaderFactory.CreateReader(stream);
                    AdvanceToData(reader);
                    var rows = GetRows(reader).ToList();
                    var string_builder = new StringBuilder();
                    var data_points = rows.Select(row =>
                    {
                        return new DataPoint
                        {
                            Date = row.Date,
                            Cases = (row.NewCases ?? 0, row.CummulativeCases ?? 0),
                            Hospitalisations = (row.NewHospitalisations ?? 0, row.CummulativeHospitalisations ?? 0),
                            Deaths = (row.NewDeaths ?? 0, row.CummulativeDeaths ?? 0),
                        };
                    }).ToList();
                    var identifier = $"stats_{rows.Last().Date.Year}_{rows.Last().Date.Month}_{rows.Last().Date.Day}";
                    if (dates.Add(rows.Last().Date))
                    {
                        All.Add(rows.Last().Date, data_points);
                        context.ReportDiagnostic(Diagnostic.Create(FileProcessed, null, input_file.Path));
                    }
                }
                var last_day = dates.OrderBy(v => v).Last();
                var MostRecent = last_day.ToLongDateString();
                var Current = All[last_day];
                var ExtendedCurrent = Current.SevenDayAverages().OrderByDescending(v => v.Date).Take(62).ToList();
                var ExtendedAll = All.ToDictionary(e => e.Key, e => e.Value.SevenDayAverages().OrderByDescending(v => v.Date).Take(62).PrependUpTo(Current.Max(d => d.Date)).ToList());
                var ComparedExtendedCurrent = ExtendedCurrent.Compare(ExtendedAll);
                var ComparedExtendedAll = ExtendedAll.ToDictionary(e => e.Key, e => e.Value.Compare(ExtendedAll));
                var extended_current_serialized_uncompressed = JsonSerializer.Serialize(ExtendedCurrent);
                var extended_current_serialized_compressed = Compress(extended_current_serialized_uncompressed);
                if (Decompress(extended_current_serialized_compressed) != extended_current_serialized_uncompressed)
                {
                    throw new Exception($"1 Bad compression {extended_current_serialized_compressed.Length} -> {Decompress(extended_current_serialized_compressed).Length} vs {extended_current_serialized_uncompressed.Length}");
                }
                context.ReportDiagnostic(Diagnostic.Create(Compressed, null, extended_current_serialized_uncompressed.Length, extended_current_serialized_compressed.Length,
                    100 * extended_current_serialized_compressed.Length / (double)extended_current_serialized_uncompressed.Length));
                var extended_all_serialized_uncompressed = JsonSerializer.Serialize(ExtendedAll);
                var extended_all_serialized_compressed = Compress(extended_all_serialized_uncompressed);
                if (Decompress(extended_all_serialized_compressed) != extended_all_serialized_uncompressed)
                {
                    throw new Exception($"2 Bad compression {Decompress(extended_all_serialized_compressed).Length} vs {extended_all_serialized_uncompressed.Length}");
                }
                context.ReportDiagnostic(Diagnostic.Create(Compressed, null, extended_all_serialized_uncompressed.Length, extended_all_serialized_compressed.Length,
                    100 * extended_all_serialized_compressed.Length / (double)extended_all_serialized_uncompressed.Length));
                var compared_extended_current_serialized_uncompressed = JsonSerializer.Serialize(ComparedExtendedCurrent);
                var compared_extended_current_serialized_compressed = Compress(compared_extended_current_serialized_uncompressed);
                if (Decompress(compared_extended_current_serialized_compressed) != compared_extended_current_serialized_uncompressed)
                {
                    throw new Exception($"3 Bad compression {Decompress(compared_extended_current_serialized_compressed).Length} vs {compared_extended_current_serialized_uncompressed.Length}");
                }
                context.ReportDiagnostic(Diagnostic.Create(Compressed, null, compared_extended_current_serialized_uncompressed.Length, compared_extended_current_serialized_compressed.Length,
                    100 * compared_extended_current_serialized_compressed.Length / (double)compared_extended_current_serialized_uncompressed.Length));
                var compared_extended_all_serialized_uncompressed = JsonSerializer.Serialize(ComparedExtendedAll);
                var compared_extended_all_serialized_compressed = Compress(compared_extended_all_serialized_uncompressed);
                if (Decompress(compared_extended_all_serialized_compressed) != compared_extended_all_serialized_uncompressed)
                {
                    throw new Exception($"4 Bad compression {Decompress(compared_extended_all_serialized_compressed).Length} vs {compared_extended_all_serialized_uncompressed.Length}");
                }
                context.ReportDiagnostic(Diagnostic.Create(Compressed, null, compared_extended_all_serialized_uncompressed.Length, compared_extended_all_serialized_compressed.Length,
                    100 * compared_extended_all_serialized_compressed.Length / (double)compared_extended_all_serialized_uncompressed.Length));
                var final_code = $@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
namespace CovidStatsCH.Components
{{
    public partial class DataPointProvider
    {{
        private static string Decompress(string input)
        {{
            using var input_stream = new MemoryStream(Convert.FromBase64String(input));
            using var compression_stream = new GZipStream(input_stream, CompressionMode.Decompress);
            using var output_stream = new MemoryStream();
            compression_stream.CopyTo(output_stream);
            var output_buffer = output_stream.ToArray();
            return Encoding.ASCII.GetString(output_buffer);
        }}
        public static readonly DateTime MostRecent = DateTime.Parse(""{last_day.ToLongDateString()}"");
        public static readonly List<ExtendedDataPoint> ExtendedCurrent = JsonSerializer.Deserialize<List<ExtendedDataPoint>>(Decompress(""{extended_current_serialized_compressed}""));
        public static readonly Dictionary<DateTime, List<ExtendedDataPoint>> ExtendedAll = JsonSerializer.Deserialize<Dictionary<DateTime, List<ExtendedDataPoint>>>(Decompress(""{extended_all_serialized_compressed}""));
        public static readonly List<ComparedExtendedDataPoint> ComparedExtendedCurrent = JsonSerializer.Deserialize<List<ComparedExtendedDataPoint>>(Decompress(""{compared_extended_current_serialized_compressed}""));
        public static readonly Dictionary<DateTime, List<ComparedExtendedDataPoint>> ComparedExtendedAll = JsonSerializer.Deserialize<Dictionary<DateTime, List<ComparedExtendedDataPoint>>>(Decompress(""{compared_extended_all_serialized_compressed}""));
        
    }}
}}
";
                context.AddSource("final", final_code);
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(Error, null, e.Message, e.StackTrace));
            }
        }
        private static string Compress(string input)
        {
            var data = Encoding.ASCII.GetBytes(input);
            using var output_stream = new MemoryStream();
            using var compression_stream = new GZipStream(output_stream, CompressionMode.Compress);
            compression_stream.Write(data, 0, data.Length);
            compression_stream.Close();
            var output_buffer = output_stream.ToArray();
            return Convert.ToBase64String(output_buffer);
        }
        private static string Decompress(string input)
        {
            using var input_stream = new MemoryStream(Convert.FromBase64String(input));
            using var compression_stream = new GZipStream(input_stream, CompressionMode.Decompress);
            using var output_stream = new MemoryStream();
            compression_stream.CopyTo(output_stream);
            var output_buffer = output_stream.ToArray();
            return Encoding.ASCII.GetString(output_buffer);
        }
        private class Row
        {
            public DateTime Date { get; set; }
            public int? NewCases { get; internal set; }
            public int? CummulativeCases { get; internal set; }
            public int? NewHospitalisations { get; internal set; }
            public int? CummulativeHospitalisations { get; internal set; }
            public int? NewDeaths { get; internal set; }
            public int? CummulativeDeaths { get; internal set; }
        }
        private void AdvanceToData(IExcelDataReader reader)
        {
            while (reader.Read() && reader.GetString(0) != "Datum")
            {
            }
        }
        private IEnumerable<Row> GetRows(IExcelDataReader reader)
        {
            while (reader.Read())
            {
                yield return new Row
                {
                    Date = reader.GetDateTime(0),
                    NewCases = TryGetInt(reader, 1),
                    CummulativeCases = TryGetInt(reader, 2),
                    NewHospitalisations = TryGetInt(reader, 3),
                    CummulativeHospitalisations = TryGetInt(reader, 4),
                    NewDeaths = TryGetInt(reader, 5),
                    CummulativeDeaths = TryGetInt(reader, 6),
                };
            }

        }

        private static int? TryGetInt(IExcelDataReader reader, int column)
        {
            var value = reader.GetValue(column);
            if (value is double d)
            {
                return (int)d;
            }
            if (value is int i)
            {
                return i;
            }
            if (value is long l)
            {
                return (int)l;
            }
            if (value is not string s)
            {
                return null;
            }
            return int.Parse(s);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}

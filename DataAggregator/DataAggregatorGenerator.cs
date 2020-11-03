using CovidStatsCH.Components;
using ExcelDataReader;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
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
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            if (context.AdditionalFiles == null)
            {
                return;
            }
            try
            {
                var files = context.AdditionalFiles.Where(i => i.Path.EndsWith("xlsx")).OrderBy(f => f.Path).ToList();
                context.ReportDiagnostic(Diagnostic.Create(FileProcessed, null, files.Count));
                var dates = new HashSet<DateTime>();
                var All = new Dictionary<string, List<DataPoint>>();
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
                        All.Add(identifier, data_points);
                        context.ReportDiagnostic(Diagnostic.Create(FileProcessed, null, input_file.Path));
                    }
                }
                var last_day = dates.OrderBy(v => v).Last();
                var MostRecent = last_day.ToLongDateString();
                var last_day_identifier = $"stats_{last_day.Year}_{last_day.Month}_{last_day.Day}";
                var Current = All[last_day_identifier];
                var ExtendedCurrent = Current.SevenDayAverages().OrderByDescending(v => v.Date).Take(62).ToList();
                var ExtendedAll = All.ToDictionary(e => e.Key, e => e.Value.SevenDayAverages().OrderByDescending(v => v.Date).Take(62).PrependUpTo(Current.Max(d => d.Date)).ToList());
                var ComparedExtendedCurrent = ExtendedCurrent.Compare(ExtendedAll);
                var ComparedExtendedAll = ExtendedAll.ToDictionary(e => e.Key, e => e.Value.Compare(ExtendedAll));
                var final_code = $@"
using System;
using System.Linq;
using System.Collections.Generic;
namespace CovidStatsCH.Components
{{
    public partial class DataPointProvider
    {{
        public static readonly string MostRecent = ""{last_day.ToLongDateString()}"";
        public static readonly List<ComparedExtendedDataPoint> ComparedExtendedCurrent = JsonSerializer.Deserialize<List<ComparedExtendedDataPoint>>({JsonSerializer.Serialize(ComparedExtendedCurrent)});
        public static readonly Dictionary<string, List<ComparedExtendedDataPoint>> ComparedExtendedAll = JsonSerializer.Deserialize<Dictionary<string, List<ComparedExtendedDataPoint>>>({JsonSerializer.Serialize(ComparedExtendedAll)});
        
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

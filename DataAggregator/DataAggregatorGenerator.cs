using ExcelDataReader;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            if (context.AdditionalFiles == null)
            {
                return;
            }
            try
            {
                var files = context.AdditionalFiles.Where(i => i.Path.EndsWith("xlsx")).OrderBy(f => f.Path).ToList();
                context.ReportDiagnostic(Diagnostic.Create(FileProcessed, null, files.Count));
                var dates = new List<DateTime>();
                foreach (var input_file in files)
                {
                    using var stream = File.OpenRead(input_file.Path);
                    using var reader = ExcelReaderFactory.CreateReader(stream);
                    AdvanceToData(reader);
                    var rows = GetRows(reader).ToList();
                    var string_builder = new StringBuilder();
                    foreach (var row in rows)
                    {
                        string_builder.AppendLine($@"
new DataPoint {{
Date =  new DateTime({row.Date.Ticks}L),
Cases = ({row.NewCases ?? 0}, {row.CummulativeCases ?? 0}),
Hospitalisations = ({row.NewHospitalisations ?? 0}, {row.CummulativeHospitalisations ?? 0}),
Deaths = ({row.NewDeaths ?? 0}, {row.CummulativeDeaths ?? 0})  }},
");
                    }
                    var identifier = $"stats_{rows.Last().Date.Year}_{rows.Last().Date.Month}_{rows.Last().Date.Day}";
                    dates.Add(rows.Last().Date);
                    var code = $@"
using System;
using System.Collections.Generic;
namespace CovidStatsCH.Components
{{
    public partial class DataPointProvider
    {{
        public static readonly List<DataPoint> {identifier} = new List<DataPoint>
        {{
            {string_builder}
        }};
    }}
}}
";
                    context.AddSource(identifier, code);
                    context.ReportDiagnostic(Diagnostic.Create(FileProcessed, null, input_file.Path));
                }
                var last_day = dates.OrderBy(v => v).Last();
                var last_day_identifier = $"stats_{last_day.Year}_{last_day.Month}_{last_day.Day}";
                var final_code = $@"
using System;
using System.Collections.Generic;
namespace CovidStatsCH.Components
{{
    public partial class DataPointProvider
    {{
        public static readonly string MostRecent = ""{last_day.ToLongDateString()}"";
        public static readonly List<DataPoint> Current = {last_day_identifier};
        public static readonly Dictionary<string, List<DataPoint>> All = new Dictionary<string, List<DataPoint>>
        {{
            {string.Join(",\n", dates.Select(d => 
                {
                    var human_readable = d.ToLongDateString();
                    var identifier = $"stats_{d.Year}_{d.Month}_{d.Day}";
                    return $"[\"{human_readable}\"] = DataPointProvider.{identifier}";
                }))}
        }};
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

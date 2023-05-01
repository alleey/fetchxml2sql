using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FetchXml.Model;
using FetchXml.Converter;
using FetchXml.Formatter;

namespace FetchXml.CLI.Commands
{
    [Command(Name = "sqlserver",
       Description = "Convert fetchxml to sqlserver",
       UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue,
       OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    class ConvertSqlServerCommand : CommandBase
    {
        private readonly IFetchXmlToSQLConverter _converter;
        private readonly ISqlFormatter _formatter;
        private readonly IConsole _console;

        public ConvertSqlServerCommand(
           IFetchXmlToSQLConverter converter,
           ISqlFormatter formatter,
           ILogger<ConvertSqlServerCommand> logger,
           IConsole console) : base(logger)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        [Argument(0)]
        public string[] Files { get; }

        protected override async Task<int> DoWorkAsync(CommandLineApplication app)
        {
            if (Files != null)
            {
                foreach (var file in Files)
                {
                    await Task.Run(() => 
                    {
                        WriteDebug($"Converting {file} ...");
                        using (var fileStream = File.Open(file, FileMode.Open))
                        {
                            var serializer = new XmlSerializer(typeof(FetchType));
                            var fetchXml = (FetchType)serializer.Deserialize(fileStream);
                            var selectExprs = _converter.Convert(fetchXml);

                            foreach (var sqlSelect in selectExprs)
                                _console.WriteLine(_formatter.Format(sqlSelect));
                        }
                    });
                }
            } 
            else
            {
                app.ShowHelp();
            }
            return 0;
        }
    }
}

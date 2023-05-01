using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace FetchXml.CLI.Commands
{
   [Command(Name = "convert",
       UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue,
       OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
   [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
   [Subcommand(typeof(ConvertSqlServerCommand))]
   class ConvertSQLCommand : CommandBase
   {
      public ConvertSQLCommand(ILogger<ConvertSQLCommand> logger) 
         : base(logger)
      {
      }

      protected override Task<int> DoWorkAsync(CommandLineApplication app)
      {
         // this shows help even if the --help option isn't specified
         app.ShowHelp();
         return Task.FromResult(0);
      }

      private static string GetVersion()
          => typeof(ConvertSQLCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

   }
}

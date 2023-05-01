using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FetchXml.CLI.Commands
{
   [HelpOption("--help")]
   abstract class CommandBase
   {
      private readonly ILogger _logger;

      public CommandBase(ILogger<CommandBase> logger)
      {
         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      }

      protected void WriteDebug(string message, params object[] args) => _logger.LogDebug(message, args);
      protected void WriteHost(string message, params object[] args) => _logger.LogInformation(message, args);
      protected void WriteWarning(string message, params object[] args) => _logger.LogWarning(message, args);
      protected void WriteException(Exception ex) => _logger.LogError(ex.Message);

      protected abstract Task<int> DoWorkAsync(CommandLineApplication app);

      protected Task<int> OnExecuteAsync(CommandLineApplication app)
      {
         return DoWorkAsync(app);
      }
   }
}

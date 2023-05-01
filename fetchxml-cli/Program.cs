using System;
using System.Threading.Tasks;
using FetchXml.Dialect;
using FetchXml.Converter;
using FetchXml.Formatter;
using FetchXml.CLI.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FetchXml.CLI
{
    class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var builder = new HostBuilder()
              .ConfigureHostConfiguration((config) =>
              {
                  config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
#if DEBUG
                     .AddJsonFile("appsettings.debug.json", optional: true, reloadOnChange: false)
#endif
                     .AddEnvironmentVariables(prefix: "FETCHXML_CLI_")
                     .AddCommandLine(args);
              })
              .ConfigureLogging((hostingContext, logging) =>
              {
                  logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                  logging.AddConsole();
              })
              .ConfigureServices(ConfigureApplicationServices);

            try
            {
                return await builder.RunCommandLineApplicationAsync<ConvertSQLCommand>(args).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        static void ConfigureApplicationServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.Configure<SqlServerSqlFormatter.Options>(hostContext.Configuration.GetSection("formatter"));
            services.AddTransient<SqlServerSqlFormatter.Options>(x => x.GetRequiredService<IOptions<SqlServerSqlFormatter.Options>>().Value);
            services.AddTransient<ISqlFormatter, SqlServerSqlFormatter>();

            services.AddTransient<ISqlDialect, SqlServerDialect>();

            services.Configure<FetchXmlToSQLConverter.Options>(hostContext.Configuration.GetSection("convertor"));
            services.AddTransient<FetchXmlToSQLConverter.Options>(x => x.GetRequiredService<IOptions<FetchXmlToSQLConverter.Options>>().Value);
            services.AddTransient<IFetchXmlToSQLConverter, FetchXmlToSQLConverter>();
        }
    }
}

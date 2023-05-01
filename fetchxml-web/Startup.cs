using FetchXml.Dialect;
using FetchXml.Converter;
using FetchXml.Formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FetchXml.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SqlServerSqlFormatter.Options>(Configuration.GetSection("formatter"));
            services.AddTransient<SqlServerSqlFormatter.Options>(x => x.GetRequiredService<IOptions<SqlServerSqlFormatter.Options>>().Value);
            
            services.AddTransient<ISqlFormatter, SqlServerSqlFormatter>();
            services.AddTransient<ISqlDialect, SqlServerDialect>();

            services.Configure<FetchXmlToSQLConverter.Options>(Configuration.GetSection("convertor"));
            services.AddTransient<FetchXmlToSQLConverter.Options>(x => x.GetRequiredService<IOptions<FetchXmlToSQLConverter.Options>>().Value);
            services.AddTransient<IFetchXmlToSQLConverter, FetchXmlToSQLConverter>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

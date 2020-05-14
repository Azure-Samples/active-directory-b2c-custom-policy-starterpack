using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using System;

namespace AADB2C.UserMigration.API
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
            services.AddMvc();

            services
                .AddSingleton(provider =>
                    {
                        var connectionString = Configuration["BlobStorageConnectionString"];

                        if (string.IsNullOrEmpty(connectionString))
                        {
                            throw new InvalidOperationException("BlobStorageConnectionString should be set in appsettings.json");
                        }

                        return CloudStorageAccount.Parse(connectionString);
                    })
                .AddSingleton<UserMigrationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}

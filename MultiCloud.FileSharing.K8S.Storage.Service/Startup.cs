using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiCloud.FileSharing.K8S.Storage.Interfaces;
using MultiCloud.FileStorage.K8S.Storage.AWS.Providers;
using MultiCloud.FileStorage.K8S.Storage.Azure.Providers;
using MultiCloud.FileStorage.K8S.Storage.GCP.Providers;

namespace MultiCloud.FileSharing.K8S.Storage.Service
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
        }

        public void ConfigureAWSServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTransient<IStorageStrategyProvider, AWSS3StorageStrategyProvider>();
            services.Configure<AWSS3StorageStrategyProvider.Options>(Configuration);
        }

        public void ConfigureAzureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTransient<IStorageStrategyProvider, AzureStorageStrategyProvider>();
            services.Configure<AzureStorageStrategyProvider.Options>(Configuration);
        }

        public void ConfigureGCPServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTransient<IStorageStrategyProvider, GCPStorageStrategyProvider>();
            services.Configure<GCPStorageStrategyProvider.Options>(Configuration);
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

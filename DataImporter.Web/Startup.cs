using DataImporter.Entities;
using DataImporter.FileHandler;
using DataImporter.FileHandler.Impl;
using DataImporter.Repositories;
using DataImporter.Services;
using DataImporter.Services.Impl;
using DataImporter.Services.Impl.Parsers;
using DataImporter.SqlStorage;
using DataImporter.SqlStorage.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataImporter.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    this.Configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsAssembly("DataImporter.SqlStorage")));

            services.AddRazorPages();

            this.RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<ITransactionService, TransactionService>();

            services.AddTransient<IFileParserFactory, FileParserFactory>();
            services.AddTransient<IFileParser<Transaction>, CsvTransactionParser>();
            services.AddTransient<IFileParser<Transaction>, XmlTransactionParser>();

            services.AddTransient<IFileExtensionParser, FileExtensionParser>();
            services.AddTransient<ICsvFileReader, CsvFileReader>();
            services.AddTransient<IXmlFileReader, XmlFileReader>();
        }
    }
}

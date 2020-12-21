using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MZPO.Services;
using MZPO.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace MZPO
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
            services.AddControllers();

            services.AddDbContext<MySQLContext>(opt => opt.UseMySql(
                Configuration.GetConnectionString("MySQLConnection"), 
                new MySqlServerVersion(new Version(5, 7, 31)),
                builder =>
                {
                    builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                }
                ));

            services.AddScoped<IAccountRepo, AccountRepo>();
            services.AddScoped<ICityRepo, CityRepo>();
            services.AddScoped<ITagRepo, TagRepo>();
            services.AddScoped<ICFRepo, CFRepo>();

            services.AddSingleton<Amo>();
            services.AddSingleton<TaskList>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

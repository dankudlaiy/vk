using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConvertToPrimaryConstructor

namespace Eq;

public class Startup
{
    public IConfiguration Configuration { get; }
        
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
        
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
    }
        
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        app.UseMvc();
    }
}
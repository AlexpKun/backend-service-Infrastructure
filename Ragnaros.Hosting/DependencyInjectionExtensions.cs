using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using System.Reflection;

namespace Ragnaros.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IMvcBuilder AddRagnarosMvc(this IServiceCollection serviceDescriptors)
        {
            return serviceDescriptors.AddSwaggerDocument(conf => conf.Title = Assembly.GetEntryAssembly().GetName().Name)
                                     .AddMvc()
                                     .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                                     .AddJsonOptions(opts => opts.SerializerSettings.Converters.Add(new StringEnumConverter()));
        }

        public static IApplicationBuilder UseRagnarosMvc(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUi3();

            app.UseMvc();

            return app;
        }
    }
}

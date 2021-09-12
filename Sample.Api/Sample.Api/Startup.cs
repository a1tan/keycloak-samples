using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sample.Api
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
            services.AddApiVersioning(
            options =>
            {
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.Conventions.Add(new VersionByNamespaceConvention());
            }
        );
            services.AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                }
            );
            services.AddHttpClient();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenerationOptions>();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                Configuration.Bind("Security:Jwt", options);
            });
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app
                .UseSwagger(o =>
                {
                    o.RouteTemplate = "swagger/{documentName}/" + ("swagger") + ".json";
                })
                .UseSwaggerUI(setup =>
                {
                    // Her API versiyonu icin bir Swagger endpoint
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        setup.SwaggerEndpoint($"{description.GroupName}/{"swagger"}.json", description.GroupName.ToUpperInvariant());
                    }
                    //setup.RoutePrefix = "swagger";
                    setup.DocumentTitle = "swagger";
                    if (Configuration["Api:AllowAnonymous"] == null || !Configuration.GetValue<bool>("Api:AllowAnonymous"))
                    {
                        setup.OAuthClientId(Configuration["Security:Jwt:ClientId"]);
                        setup.OAuthClientSecret(Configuration["Security:Jwt:ClientSecret"]);
                        setup.OAuthAppName("Sample API");
                        setup.OAuthScopeSeparator(" ");
                        setup.OAuthUsePkce();
                    }
                });
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

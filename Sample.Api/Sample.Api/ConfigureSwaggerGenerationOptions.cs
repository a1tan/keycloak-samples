using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Net.Http;

namespace Sample.Api
{
    public class ConfigureSwaggerGenerationOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Bağımlılıkları başlatır.
        /// </summary>
        /// <param name="configuration">Konfigürasyondan gelecek identity provider bilgileri için kullanlır</param>
        /// <param name="httpClientFactory">Identity provider keşfi için kullanılır</param>
        public ConfigureSwaggerGenerationOptions(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Swagger içi authorization grant flow konfigürasyonunu yapar
        /// </summary>
        /// <param name="options"></param>
        public void Configure(SwaggerGenOptions options)
        {
            var discoveryDocument = GetDiscoveryDocument();

            options.OperationFilter<AuthorizeOperationFilter>();
            options.DescribeAllParametersInCamelCase();
            options.CustomSchemaIds(x => x.GenericsSupportedId());
            options.SwaggerDoc("v1", CreateOpenApiInfo());
            options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,

                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        //AuthorizationUrl = new Uri(discoveryDocument.AuthorizeEndpoint),
                        //TokenUrl = new Uri(discoveryDocument.TokenEndpoint),
                        AuthorizationUrl = new Uri($"{_configuration["Security:Jwt:Authority"]}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{_configuration["Security:Jwt:Authority"]}/protocol/openid-connect/token"),
                        //Scopes = new Dictionary<string, string>
                        //{
                        //    { _settings.Security.Jwt.Audience , "Balea Server HTTP Api" }
                        //},
                    }
                },
                Description = "Balea Server OpenId Security Scheme"
            });
        }

        /// <summary>
        /// Identity provider keşfini yapar. JWKS adresi vs.
        /// </summary>
        /// <returns></returns>
        private DiscoveryDocumentResponse GetDiscoveryDocument()
        {
            return _httpClientFactory
                .CreateClient()
                .GetDiscoveryDocumentAsync(_configuration["Security:Jwt:Authority"])
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Swagger dokümantasyonu versiyon bilgisi gibi tanımları getirir
        /// </summary>
        /// <returns></returns>
        private OpenApiInfo CreateOpenApiInfo()
        {
            //TODO: Bu kısımlar konfigürasyon dosyasından gelmeli
            return new OpenApiInfo()
            {
                Title = _configuration["OpenApi:Name"] ?? "My Awesome API",
                Version = "v1",
                Description = _configuration["OpenApi:Name"] ?? "My Awesome API",
                Contact = new OpenApiContact() { Name = "API" },
                License = new OpenApiLicense()
            };
        }
    }
}
using Asp.Versioning;
using Microsoft.OpenApi.Models;

namespace Shomei.Testing.AuthAPI.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddSwaggerExtension(this IServiceCollection service)
        {
            string SECURITY_DEFINITION = "Bearer";
            var URL = new Uri("https://abreuhd.com");

            service.AddSwaggerGen(options =>
            {
                List<string> xmlFiles = [.. Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)];
                xmlFiles.ForEach(xmlFile => options.IncludeXmlComments(xmlFile));

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Testing",
                    Description = "TestingApi",
                    Contact = new OpenApiContact
                    {
                        Email = "jefferson@abreuhd.com",
                        Name = "Jefferson Abreu",
                        Url = URL
                    }
                });

                options.EnableAnnotations();

                options.DescribeAllParametersInCamelCase();
                options.AddSecurityDefinition(SECURITY_DEFINITION, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = SECURITY_DEFINITION,
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {Token}"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = SECURITY_DEFINITION
                            },
                            Scheme = SECURITY_DEFINITION,
                            Name = SECURITY_DEFINITION,
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });
            });
        }

        public static void AddApiVersioningExtension(this IServiceCollection service)
        {
            service.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

        }
    }
}

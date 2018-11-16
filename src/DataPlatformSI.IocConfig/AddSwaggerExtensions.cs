using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataPlatformSI.IocConfig
{
    public static class AddSwaggerExtensions
    {
        public static IServiceCollection AddSwaggerGen(this IServiceCollection services,string xmlPath)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                   new Info()
                   {
                       Title = $"DataPlatform API V1",
                       Version = "V1",
                       Description = "A webapi application with Swagger, Swashbuckle, and API versioning.",
                       Contact = new Contact() { Name = "Fusion Shen", Email = "fusionshen@hotmail.com", Url="www.eabax.com" },
                       TermsOfService = "Shareware",
                       License = new License() { Name = "MIT", Url = "https://opensource.org/licenses/MIT" }
                   }
                );
                c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, xmlPath + ".xml"),true);
                //Authorize
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "Bearer", new string[] { } }, });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 参数结构: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",//jwt默认的参数名称
                    In = "header",//jwt默认存放Authorization信息的位置(请求头中)
                    Type = "apiKey"
                });
            });

            return services;
        }
    }
}

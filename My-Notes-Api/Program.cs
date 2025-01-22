using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using My_Notes_Api.Services;
using NLog;
using My_Notes_Api.Context;
using System.Text.Json.Serialization;

namespace My_Notes_Api
{
    public class Program
    {
        public static Logger logger;
        public static IConfiguration configuration;
        public static void Main(string[] args)
        {
            logger = LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config")).GetCurrentClassLogger();
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

            logger.Debug("--START--");
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            var env = builder.Environment;

            configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // Set the base path for the appsettings.json file
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

            // Logger and Database ORM.
            services.AddSingleton<ILoggerManager, LoggerManager>();
            services.AddSingleton<DapperContext>();

            // Configure Cookie
            services.ConfigureApplicationCookie(x =>
            {
                x.Cookie.HttpOnly = true;
            });

            Dapper.SqlMapper.Settings.CommandTimeout = 0;

            services.AddControllers()
                .AddMvcOptions(x =>
                {
                    x.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                })
                .AddJsonOptions(x =>
                {
                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    x.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            // Enable cors
            services.AddCors(o => o.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            }));

            services.Configure<FormOptions>(options =>
            {
                // Set the limit to 128 MB
                options.MultipartBodyLengthLimit = 2294967295;
            });
            services.AddMvc(m => m.EnableEndpointRouting = false);

            // Add services to the container
            services.AddScoped<IAuthService, AuthService>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var app = builder.Build();
            {
                app.UseCors(x => x
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

                //app.UseMiddleware<ErrorHandlerMiddleware>();
                app.MapControllers();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                //c.InjectJavascript("swagger-ui-bundle.js", "text/javascript");
                //c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "v1");
                c.DocumentTitle = "MyNotesApi";
                //c.DefaultModelsExpandDepth(-1);
                //c.DocExpansion(DocExpansion.None);
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
            });

            app.UseCors(); 
            app.UseMvc();

            //app.UseHttpsRedirection();
            app.Run();
        }
    }
}

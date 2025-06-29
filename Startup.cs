using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Text.Json.Serialization;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;
using WebApi.Services.WebApi.Services;
using StackExchange.Redis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Add services to the DI container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>();
            services.AddCors();
            services.AddControllers().AddJsonOptions(x =>
            {
                // Serialize enums as strings in API responses (e.g., Role)
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                // Ignore omitted parameters on models to enable optional params (e.g., User update)
                x.JsonSerializerOptions.IgnoreNullValues = true;

                // Handle reference cycles
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; // Xử lý vòng tham chiếu
            });

            services.AddAutoMapper(typeof(AutoMapperProfile));

            // Configure DI for application services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITourService, TourService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IDiscountCodeService, DiscountCodeService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ITransactionService, TransactionService>();

            services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var redisUrl = configuration["REDIS_URL"] ?? "localhost:6379";

                    var options = ConfigurationOptions.Parse(redisUrl);
                    options.AbortOnConnectFail = false; // Không crash khi chưa kết nối được

                    try
                    {
                        return ConnectionMultiplexer.Connect(options);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("⚠️ Redis connection failed: " + ex.Message);
                        return null; // Trả null, cho phép app vẫn chạy
                    }
                });

            // Configure JWT
            var jwtSettings = Configuration.GetSection("Jwt").Get<JwtSettings>();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Key); // Đảm bảo key có ít nhất 16 ký tự

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Configure authorization
            services.AddAuthorization();
            services.AddHttpContextAccessor();
           
        }

        // Configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();
            app.UseAuthentication(); // Xác thực 
            app.UseAuthorization(); // Phân quyền
            app.UseStaticFiles();   // Up file tĩnh

            // Global CORS policy
            app.UseCors(x => x
                .WithOrigins("tour-fe-dr6e.vercel.app")
                .AllowAnyMethod()
                .AllowAnyHeader());

            // Global error handler
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseMiddleware<JwtMiddleware>();
            app.UseEndpoints(x => x.MapControllers());
            
        }
    }
}

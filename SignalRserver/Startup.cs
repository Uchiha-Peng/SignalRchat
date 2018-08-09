using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SignalRAuthenticationSample.Data;
using SignalRAuthenticationSample.Hubs;

namespace SignalRAuthenticationSample
{
    public class Startup
    {
        //我们使用在启动期间在此服务器上生成的密钥来保护我们的令牌。
        //这意味着如果应用重新启动，现有令牌将变为无效。 它也行不通
        //使用多台服务器时
        public static readonly SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // 此方法由运行时调用。 使用此方法将服务添加到容器。
        #region snippet
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ChatDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>                {
                    //Identity使Cookie认证成为默认值。
                    //但是，我们希望JWT Bearer Auth成为默认值。
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    //配置JWT Bearer Auth以获取我们的安全密钥
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            LifetimeValidator = (before, expires, token, param) =>
                            {
                                return expires > DateTime.UtcNow;
                            },
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateActor = false,
                            ValidateLifetime = true,
                            IssuerSigningKey = SecurityKey
                        };

                    //我们必须挂钩OnMessageReceived事件才能
                    //允许JWT身份验证处理程序读取访问权限
                    // WebSocket或者来自查询字符串的标记
                    //服务器发送事件请求进来。
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/hubs/chat")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSignalR();

            //更改为使用Name作为SignalR的用户标识符
            //警告：这需要您的JWT令牌的来源
            //确保名称声明是唯一的！
            //如果Name声明不是唯一的，则用户可以接收消息
            //面向不同的用户！
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        }
        #endregion

        // This method gets called by the runtime. Use this method to configure the
        // HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseMvc();

            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/hubs/chat");
            });
        }
    }
}

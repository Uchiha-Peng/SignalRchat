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
        //����ʹ���������ڼ��ڴ˷����������ɵ���Կ���������ǵ����ơ�
        //����ζ�����Ӧ�������������������ƽ���Ϊ��Ч�� ��Ҳ�в�ͨ
        //ʹ�ö�̨������ʱ
        public static readonly SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // �˷���������ʱ���á� ʹ�ô˷�����������ӵ�������
        #region snippet
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ChatDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>                {
                    //IdentityʹCookie��֤��ΪĬ��ֵ��
                    //���ǣ�����ϣ��JWT Bearer Auth��ΪĬ��ֵ��
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    //����JWT Bearer Auth�Ի�ȡ���ǵİ�ȫ��Կ
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

                    //���Ǳ���ҹ�OnMessageReceived�¼�����
                    //����JWT�����֤��������ȡ����Ȩ��
                    // WebSocket�������Բ�ѯ�ַ����ı��
                    //�����������¼����������
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

            //����Ϊʹ��Name��ΪSignalR���û���ʶ��
            //���棺����Ҫ����JWT���Ƶ���Դ
            //ȷ������������Ψһ�ģ�
            //���Name��������Ψһ�ģ����û����Խ�����Ϣ
            //����ͬ���û���
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

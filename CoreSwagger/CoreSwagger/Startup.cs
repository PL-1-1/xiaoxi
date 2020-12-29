using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace CoreSwagger
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
            services.AddControllers().AddNewtonsoftJson();
            services.AddControllers();
            #region 跨域配置
            services.AddCors(ky =>
            {
                ky.AddPolicy("cors", yx =>
                {
                    yx.WithOrigins("http://*.*.*.*").AllowAnyHeader()//允许任何标题
                        .AllowAnyMethod()//允许任何方法
                        .AllowAnyOrigin();//允许任何来源主机访问
                });
            });
            #endregion
            #region 注册swagger生成器,定义一个或者多个swagger文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "学习的api接口", Version = "v1", Description = "基于.NET Core 3.1的API" });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                var xmlPath = Path.Combine(basePath, "MyApi.xml");
                // 加载xml文件（为Swagger JSON and UI 设置xml文档注释路径）
                c.IncludeXmlComments(xmlPath, true);
            });
            #endregion
            #region 注册jwt相关服务
            // 设置添加身份验证
            services.AddAuthentication(options =>
            {
                //默认身份验证方案
                options.DefaultAuthenticateScheme =JwtBearerDefaults.AuthenticationScheme;// 承载身份验证使用的默认值
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                // 获取或设置用于验证标识标记的参数
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // 验证颁发者
                    ValidateIssuer = true,
                    // 验证访问者
                    ValidateAudience = true,
                    // 验证生存期
                    ValidateLifetime = true,
                    // 验证使用者的签名秘钥
                    ValidateIssuerSigningKey = true,
                    // 设置颁发者
                    ValidIssuer = "MyIssue",
                    // 设置访问者
                    ValidAudience = "MyAudience",
                    //设置秘钥------返回令牌对称安全密钥的新实例
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("873D82D99180B30807F4DA6893369D88"))
                };

                // 身份验证处理过程的控制相关事件
                options.Events = new JwtBearerEvents()
                {
                    // 第一次收到协议消息时调用
                    OnMessageReceived = context =>
                    {
                        string token = context.Request.Headers["token"];
                        //这里可以对拿到的token进行一些处理，或者添加验证token之前的一些操作
                        //需要拿到token赋值给上下文，不然此后在Http上下文的标识中拿不到参数
                        context.Token = token;
                        return Task.CompletedTask;
                    },
                    // 在将资询发送给回调者之前调用（权限验证失败后触发）
                    OnChallenge = context =>
                    {
                        // 终止.NET Core默认的返回类型和结果
                        context.HandleResponse();
                        // 自定义返回结果
                        var myResult = JsonConvert.SerializeObject(new { code = 401, msg = "抱歉，您无权访问！" });
                        // 自定义返回数据类型
                        context.Response.ContentType = "applic/json";
                        // 自定义返回状态码（权限验证失败默认更改为401）(可以自己改成其他的)
                        // 这里写了自定义返回结果后，状态码自动就变成了 200
                        // context.Response.StatusCode = StatusCodes.Status200OK;
                        //输出json数据结果
                        context.Response.WriteAsync(myResult);
                        return Task.FromResult(0);
                    },
                };
            });

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //启动身份验证
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();
            //启用中间件服务生成的swagger作为JSON的终结点
            app.UseCors("cors");
            app.UseSwagger();
           

            // 启用swaggerUI
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "");
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

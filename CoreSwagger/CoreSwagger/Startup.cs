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
            #region ��������
            services.AddCors(ky =>
            {
                ky.AddPolicy("cors", yx =>
                {
                    yx.WithOrigins("http://*.*.*.*").AllowAnyHeader()//�����κα���
                        .AllowAnyMethod()//�����κη���
                        .AllowAnyOrigin();//�����κ���Դ��������
                });
            });
            #endregion
            #region ע��swagger������,����һ�����߶��swagger�ĵ�
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ѧϰ��api�ӿ�", Version = "v1", Description = "����.NET Core 3.1��API" });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                var xmlPath = Path.Combine(basePath, "MyApi.xml");
                // ����xml�ļ���ΪSwagger JSON and UI ����xml�ĵ�ע��·����
                c.IncludeXmlComments(xmlPath, true);
            });
            #endregion
            #region ע��jwt��ط���
            // ������������֤
            services.AddAuthentication(options =>
            {
                //Ĭ�������֤����
                options.DefaultAuthenticateScheme =JwtBearerDefaults.AuthenticationScheme;// ���������֤ʹ�õ�Ĭ��ֵ
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                // ��ȡ������������֤��ʶ��ǵĲ���
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // ��֤�䷢��
                    ValidateIssuer = true,
                    // ��֤������
                    ValidateAudience = true,
                    // ��֤������
                    ValidateLifetime = true,
                    // ��֤ʹ���ߵ�ǩ����Կ
                    ValidateIssuerSigningKey = true,
                    // ���ð䷢��
                    ValidIssuer = "MyIssue",
                    // ���÷�����
                    ValidAudience = "MyAudience",
                    //������Կ------�������ƶԳư�ȫ��Կ����ʵ��
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("873D82D99180B30807F4DA6893369D88"))
                };

                // �����֤������̵Ŀ�������¼�
                options.Events = new JwtBearerEvents()
                {
                    // ��һ���յ�Э����Ϣʱ����
                    OnMessageReceived = context =>
                    {
                        string token = context.Request.Headers["token"];
                        //������Զ��õ���token����һЩ�������������֤token֮ǰ��һЩ����
                        //��Ҫ�õ�token��ֵ�������ģ���Ȼ�˺���Http�����ĵı�ʶ���ò�������
                        context.Token = token;
                        return Task.CompletedTask;
                    },
                    // �ڽ���ѯ���͸��ص���֮ǰ���ã�Ȩ����֤ʧ�ܺ󴥷���
                    OnChallenge = context =>
                    {
                        // ��ֹ.NET CoreĬ�ϵķ������ͺͽ��
                        context.HandleResponse();
                        // �Զ��巵�ؽ��
                        var myResult = JsonConvert.SerializeObject(new { code = 401, msg = "��Ǹ������Ȩ���ʣ�" });
                        // �Զ��巵����������
                        context.Response.ContentType = "applic/json";
                        // �Զ��巵��״̬�루Ȩ����֤ʧ��Ĭ�ϸ���Ϊ401��(�����Լ��ĳ�������)
                        // ����д���Զ��巵�ؽ����״̬���Զ��ͱ���� 200
                        // context.Response.StatusCode = StatusCodes.Status200OK;
                        //���json���ݽ��
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
            //���������֤
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();
            //�����м���������ɵ�swagger��ΪJSON���ս��
            app.UseCors("cors");
            app.UseSwagger();
           

            // ����swaggerUI
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

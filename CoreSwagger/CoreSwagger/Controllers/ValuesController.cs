
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace CoreSwagger.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="dy">通过解析操作对象，动态获取参数</param>
        /// <returns></returns>
        [HttpPost]
        public object loginAccount(dynamic dy)
        {
            //获取账户名名称
            string accountName = dy.accountName.ToString();
            //获取账户密码
            string accountPwd = dy.accountPwd.ToString();
            try
            {
                var account = new { AccountName = "admin", AccountPwd = "123456" };
                if (accountName.Equals(account.AccountName) && accountPwd.Equals(account.AccountPwd))
                {
                    //声明 用于创建临牌的参数（自己想加些什么参数就什么）
                    Claim[] claims = new Claim[] { new Claim("accountName", accountName.ToString()), new Claim("accountPwd", accountPwd.ToString()) };
                    //调用  Jwthelper 类来创建令牌
                    string token = JwtHelper.GenerateToken(claims);
                    //返回令牌
                    return new { code = 200, message = "登录成功！", data = new { token, accountName,accountPwd } };
                }
                else
                {
                    return new { code = 200, message = "账号或密码错误！" };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 测试一下
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]//添加验证标识
        public object TextToken() {
            try
            {
                var token = HttpContext.Request.Headers["account_token"];
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                string accountName = identity.Claims.Where(name => name.Type == "accountName").FirstOrDefault().Value;
                string accountPwd = identity.Claims.Where(pwd => pwd.Type == "accountPwd").FirstOrDefault().Value;
                return new { code = "200", message = "成功！", data = new { accountName, accountPwd } };
            }
            catch (Exception ex) {
                throw ex;
            }
        }
    
    
    }
}

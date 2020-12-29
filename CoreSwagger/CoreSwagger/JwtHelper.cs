using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CoreSwagger
{
    public class JwtHelper
    {
        public static string GenerateToken(Claim[] claims) {
            var secret = "873D82D99180B30807F4DA6893369D88";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            //使用安全键实例创建一个关键的信息，用什么加密算法
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                "MyIssue",
                "MyAudience",
                 claims,
                expires: DateTime.Now.AddDays(1),//令牌过期时间
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WaqfSystem.Application.DTOs.Mobile;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public AuthController(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<MobileAuthResponseDto>>> Login(MobileLoginDto dto)
        {
            // Stub for real password check
            if (dto.Email == "inspector@waqf.gov.iq" && dto.Password == "Inspect123")
            {
                var token = GenerateJwtToken("1", "FIELD_INSPECTOR");
                
                var response = new MobileAuthResponseDto
                {
                    AccessToken = token,
                    ExpiresIn = 3600 * 24 * 7, // 7 days in seconds
                    User = new MobileUserDto
                    {
                        Id = 1,
                        FullNameAr = "مفتش ميداني",
                        Role = "FIELD_INSPECTOR",
                        GovernorateId = 1,
                        GovernorateName = "بغداد"
                    }
                };

                return Ok(ApiResponse<MobileAuthResponseDto>.Ok(response));
            }

            return Unauthorized(ApiResponse<MobileAuthResponseDto>.Fail("خطأ في بيانات الدخول"));
        }

        private string GenerateJwtToken(string userId, string role)
        {
            var keyStr = _config["Jwt:Key"] ?? "VERY_SECRET_KEY_FOR_WAQF_SYSTEM_2024_MUST_BE_LONG_ENOUGH";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"] ?? "WaqfSystem",
                _config["Jwt:Audience"] ?? "MobileApp",
                claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

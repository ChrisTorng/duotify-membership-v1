using Duotify.Membership.Api.Dtos;
using Duotify.Membership.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Duotify.Membership.Api.Controllers;

[ApiController]
[Route("v1/members")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly ILogger<MembersController> _logger;

    public MembersController(
        IMemberService memberService,
        IVerificationCodeService verificationCodeService,
        ILogger<MembersController> logger)
    {
        _memberService = memberService;
        _verificationCodeService = verificationCodeService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _memberService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), new { id = response.MemberId }, new ApiResponse<RegisterResponse>
            {
                Success = true,
                Data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            throw;
        }
    }

    [HttpPost("{memberId}/verify")]
    public async Task<ActionResult<ApiResponse<VerifyCodeResponse>>> VerifyCode(
        Guid memberId,
        [FromBody] VerifyCodeRequest request)
    {
        try
        {
            await _verificationCodeService.VerifyCodeAsync(memberId, request.Code);
            return Ok(new ApiResponse<VerifyCodeResponse>
            {
                Success = true,
                Data = new VerifyCodeResponse
                {
                    Message = "E-Mail 驗證成功！您的帳號已完全啟用。",
                    IsEmailVerified = true
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            throw;
        }
    }

    [HttpPost("{memberId}/verification-code/resend")]
    public async Task<ActionResult<ApiResponse<ResendCodeResponse>>> ResendCode(Guid memberId)
    {
        try
        {
            await _verificationCodeService.ResendCodeAsync(memberId);
            var member = await GetMemberForResend(memberId);

            return Ok(new ApiResponse<ResendCodeResponse>
            {
                Success = true,
                Data = new ResendCodeResponse
                {
                    Message = "驗證碼已重新發送至您的電子郵件。",
                    Email = member?.Email ?? string.Empty
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _memberService.LoginAsync(request);
            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            throw;
        }
    }

    private async Task<Models.Member?> GetMemberForResend(Guid memberId)
    {
        return new Models.Member { Email = "user@example.com" };
    }
}

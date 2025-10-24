using Duotify.Membership.Api.Dtos;
using Duotify.Membership.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Duotify.Membership.Api.Controllers;

[ApiController]
[Route("v1/members")]
public class ProfileController : ControllerBase
{
    private readonly IMemberRepository _memberRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IMemberRepository memberRepository,
        IAuthorizationService authorizationService,
        ILogger<ProfileController> logger)
    {
        _memberRepository = memberRepository;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    /// <summary>
    /// Get user profile information (requires email verification)
    /// </summary>
    [HttpGet("{memberId:guid}/profile")]
    public async Task<ActionResult<ApiResponse<ProfileResponse>>> GetProfile(Guid memberId)
    {
        try
        {
            var canAccess = await _authorizationService.CanAccessProtectedResourceAsync(memberId);
            if (!canAccess)
            {
                _logger.LogWarning("Unauthorized profile access attempt. MemberId: {MemberId}", memberId);
                return Forbid();
            }

            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
            {
                _logger.LogWarning("Profile not found. MemberId: {MemberId}", memberId);
                return NotFound();
            }

            var response = new ApiResponse<ProfileResponse>
            {
                Success = true,
                Data = new ProfileResponse
                {
                    MemberId = member.Id,
                    Name = member.Name,
                    Email = member.Email,
                    NationalId = member.NationalId,
                    IsEmailVerified = member.IsEmailVerified,
                    CreatedAt = member.CreatedAt
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for member: {MemberId}", memberId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false
            });
        }
    }

    /// <summary>
    /// Update user profile information (requires email verification)
    /// </summary>
    [HttpPut("{memberId:guid}/profile")]
    public async Task<ActionResult<ApiResponse<ProfileResponse>>> UpdateProfile(Guid memberId, [FromBody] UpdateProfileRequest request)
    {
        try
        {
            var canAccess = await _authorizationService.CanAccessProtectedResourceAsync(memberId);
            if (!canAccess)
            {
                _logger.LogWarning("Unauthorized profile update attempt. MemberId: {MemberId}", memberId);
                return Forbid();
            }

            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
            {
                _logger.LogWarning("Member not found for update. MemberId: {MemberId}", memberId);
                return NotFound();
            }

            member.Name = request.Name ?? member.Name;
            member.UpdatedAt = DateTime.UtcNow;

            var updated = await _memberRepository.UpdateAsync(member);

            var response = new ApiResponse<ProfileResponse>
            {
                Success = true,
                Data = new ProfileResponse
                {
                    MemberId = updated.Id,
                    Name = updated.Name,
                    Email = updated.Email,
                    NationalId = updated.NationalId,
                    IsEmailVerified = updated.IsEmailVerified,
                    CreatedAt = updated.CreatedAt
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for member: {MemberId}", memberId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false
            });
        }
    }

    /// <summary>
    /// Get profile verification status
    /// </summary>
    [HttpGet("{memberId:guid}/verification-status")]
    public async Task<ActionResult<ApiResponse<VerificationStatusResponse>>> GetVerificationStatus(Guid memberId)
    {
        try
        {
            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
            {
                _logger.LogWarning("Member not found for verification status. MemberId: {MemberId}", memberId);
                return NotFound();
            }

            var response = new ApiResponse<VerificationStatusResponse>
            {
                Success = true,
                Data = new VerificationStatusResponse
                {
                    MemberId = member.Id,
                    IsEmailVerified = member.IsEmailVerified,
                    CanAccessProtectedResources = member.IsEmailVerified
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving verification status for member: {MemberId}", memberId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false
            });
        }
    }
}

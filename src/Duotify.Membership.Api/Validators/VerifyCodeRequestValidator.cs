using Duotify.Membership.Api.Dtos;
using FluentValidation;

namespace Duotify.Membership.Api.Validators;

public class VerifyCodeRequestValidator : AbstractValidator<VerifyCodeRequest>
{
    public VerifyCodeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("驗證碼不能為空")
            .Length(6).WithMessage("驗證碼必須為 6 碼")
            .Matches("[0-9]{6}").WithMessage("驗證碼必須為 6 位數字");
    }
}

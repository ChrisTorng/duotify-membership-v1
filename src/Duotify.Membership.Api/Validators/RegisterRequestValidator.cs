using Duotify.Membership.Api.Dtos;
using FluentValidation;

namespace Duotify.Membership.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private readonly TaiwaneseNationalIdValidator _nationalIdValidator;

    public RegisterRequestValidator(TaiwaneseNationalIdValidator nationalIdValidator)
    {
        _nationalIdValidator = nationalIdValidator;

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("身分證字號不能為空")
            .Length(10).WithMessage("身分證字號必須為 10 碼")
            .Custom((nationalId, context) =>
            {
                if (!_nationalIdValidator.IsValid(nationalId))
                {
                    context.AddFailure("NationalId", "身分證字號格式不正確或檢查碼驗證失敗");
                }
            });

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("姓名不能為空")
            .MaximumLength(100).WithMessage("姓名不能超過 100 個字元");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("電子郵件不能為空")
            .EmailAddress().WithMessage("電子郵件格式不正確")
            .MaximumLength(255).WithMessage("電子郵件不能超過 255 個字元");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼不能為空")
            .Length(8, 20).WithMessage("密碼長度必須為 8 到 20 碼")
            .Matches("[A-Z]").WithMessage("密碼必須包含至少一個大寫英文字母")
            .Matches("[a-z]").WithMessage("密碼必須包含至少一個小寫英文字母")
            .Matches("[0-9]").WithMessage("密碼必須包含至少一個數字");
    }
}

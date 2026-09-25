using FluentValidation;

namespace RbcTestTask.Api.Validators;

public class RequestModelValidator : AbstractValidator<RequestModel>
{
    public RequestModelValidator()
    {
        RuleFor(x => x.Selector).NotEmpty().WithMessage("Selector is required.");
        RuleFor(x => x.Attribute).NotEmpty().WithMessage("Attribute is required.");
        RuleFor(x => x.UrlB64).NotEmpty().WithMessage("UrlB64 is required.");
        RuleFor(x => x.EncryptedTextBytesB64).NotEmpty().WithMessage("EncryptedTextBytesB64 is required.");
        RuleFor(x => x.KeyBytesB64).NotEmpty().WithMessage("KeyBytesB64 is required.");
        RuleFor(x => x.PageB64).NotEmpty().WithMessage("PageB64 is required.");
    }
}
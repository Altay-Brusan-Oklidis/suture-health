using FluentValidation;

namespace SutureHealth.Notifications.v0001.Validators
{
    class NotificationValidator : AbstractValidator<AspNetCore.v0001.Models.Notification>
    {
        public NotificationValidator()
        {
            RuleFor(m => m.SourceId)
                .NotEmpty()
                .WithMessage("A Notification must contain a SourceId")
                .MaximumLength(256)
                .WithMessage("A SourceId must not have length greater than 256");
            RuleFor(m => m.CallbackUrl)
                .NotEmpty()
                .WithMessage("A Notification must contain a CallbackUrl")
                .MaximumLength(2048)
                .WithMessage("A CallbackUrl must not have length greater than 2048");
            RuleFor(m => m.DestinationUri)
                .NotEmpty()
                .WithMessage("A Notification must contain a DestinationUri")
                .MaximumLength(2048)
                .WithMessage("A DestinationUri must not have length greater than 2048");
            RuleFor(m => m.SourceUrl)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.SourceText))
                .WithMessage("A Notification must contain a SourceUrl when SourceText is empty.")
                .MaximumLength(256)
                .WithMessage("A SourceUrl must not have length greater than 256");
            RuleFor(m => m.SourceText)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.SourceUrl))
                .WithMessage("SourceText is required when SourceUrl is empty")
                .MaximumLength(1000)
                .WithMessage("SourceText cannot have more than 1000 characters.");
            RuleFor(m => m.Subject)
                .NotEmpty()
                .WithMessage("A Notification must contain a Subject")
                .MaximumLength(256)
                .WithMessage("A Subject must not have length greater than 256");
        }
    }
}

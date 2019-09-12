using FluentValidation;

using BES;

namespace Validation
{
    /// <summary>
    /// This class executes abstract validation against the user
    /// </summary>
    public class UserValidator : AbstractValidator<UserBES>
    {
        public UserValidator(bool IsUpdate = false)
        {
            RuleFor(o => o.Name).NotEmpty().WithMessage("The label cannot be null or empty");
            RuleFor(o => o.Nickname).NotEmpty().WithMessage("The nickname cannot be null or empty");

            if (IsUpdate)
            {
                RuleFor(o => o.ID).GreaterThan(0).WithMessage("The ID cannot be null or empty, and must be greater than zero");
            }
        }
    }
}

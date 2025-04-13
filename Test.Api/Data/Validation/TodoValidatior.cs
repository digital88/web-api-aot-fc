using FluentValidation;

namespace Test.Api.Data.Validation;

public sealed class CreateTodoValidatior : AbstractValidator<CreateTodoDto>
{
    public CreateTodoValidatior()
    {
        RuleFor(e => e.Title).NotNull().NotEmpty().MaximumLength(1000);
    }
}
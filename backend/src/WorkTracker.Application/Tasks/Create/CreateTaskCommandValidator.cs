using FluentValidation;
using TaskPriorityEnum = WorkTracker.Domain.Entities.TaskPriority;
using TaskStatusEnum = WorkTracker.Domain.Entities.TaskItemStatus;

namespace WorkTracker.Application.Tasks.Create;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<TaskStatusEnum>())}.");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage($"Priority must be one of: {string.Join(", ", Enum.GetNames<TaskPriorityEnum>())}.");
    }
}

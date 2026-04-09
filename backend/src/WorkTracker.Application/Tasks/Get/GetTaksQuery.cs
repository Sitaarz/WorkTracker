using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Application.Tasks.Get;

public sealed record class GetTaskQuery(
    Guid OwnerId,
    TaskItemStatus? Status = null,
    TaskPriority? Priority = null,
    SortBy? SortedBy = SortBy.CreatedAt,
    SortDirection? SortDirection = SortDirection.Asc,
    int Page = 1,
    int PageSize = 20
);

public enum SortBy
{
    CreatedAt = 0,
    DueDate = 1
}

public enum SortDirection
{
    Asc = 0,
    Desc = 1
}

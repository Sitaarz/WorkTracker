using WorkTracker.Application.Tasks.Get;
using WorkTracker.Domain.Entities;

namespace WorkTracker.API.Contracts.Tasks;

public sealed record class GetTasksRequest
(
    TaskItemStatus? Status = null,
    TaskPriority? Priority = null,
    SortBy? SortedBy = SortBy.CreatedAt,
    SortDirection? SortDirection = SortDirection.Asc,
    int Page = 1,
    int PageSize = 20
);

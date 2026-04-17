import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiEndpoints } from '../../../core/api/api.endpoints';
import { environment } from '../../../../environments/environment';

export type TaskStatus = 'ToDo' | 'InProgress' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High';
export type TaskSortBy = 'CreatedAt' | 'DueDate';
export type TaskSortDirection = 'Asc' | 'Desc';

export interface TaskItem {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
  ownerId: string;
  createdAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
}

export interface UpdateTaskRequest extends TaskItem {}

export interface TaskFilterRequest {
  status?: TaskStatus;
  priority?: TaskPriority;
  sortedBy?: TaskSortBy;
  sortDirection?: TaskSortDirection;
  page?: number;
  pageSize?: number;
}

export interface PagedTasksResponse {
  items: TaskItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class TasksService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  public getTasks() {
    return this.http.get<TaskItem[]>(this.baseUrl + ApiEndpoints.tasks.tasks);
  }

  public createTask(request: CreateTaskRequest) {
    return this.http.post<TaskItem>(this.baseUrl + ApiEndpoints.tasks.tasks, request);
  }

  public updateTask(request: UpdateTaskRequest) {
    return this.http.put<void>(this.baseUrl + ApiEndpoints.tasks.task(request.id), request);
  }

  public deleteTask(taskId: string) {
    return this.http.delete<void>(this.baseUrl + ApiEndpoints.tasks.task(taskId));
  }

  public filterTasks(request: TaskFilterRequest) {
    let params = new HttpParams();

    if (request.status) {
      params = params.set('status', request.status);
    }
    if (request.priority) {
      params = params.set('priority', request.priority);
    }
    if (request.sortedBy) {
      params = params.set('sortedBy', request.sortedBy);
    }
    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }
    if (request.page) {
      params = params.set('page', request.page);
    }
    if (request.pageSize) {
      params = params.set('pageSize', request.pageSize);
    }

    return this.http.get<PagedTasksResponse>(this.baseUrl + ApiEndpoints.tasks.filter, { params });
  }
}

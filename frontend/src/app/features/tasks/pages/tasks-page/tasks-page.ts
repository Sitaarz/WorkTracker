import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { ProblemDetails } from '../../../../shared/models/errors';
import { ToastService } from '../../../../shared/ui/toast.service';
import {
  CreateTaskRequest,
  TaskItem,
  TaskPriority,
  TaskSortBy,
  TaskSortDirection,
  TasksService,
  TaskStatus,
  UpdateTaskRequest,
} from '../../services/tasks-service';

@Component({
  selector: 'app-tasks-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tasks-page.html',
  styleUrl: './tasks-page.scss',
})
export class TasksPage {
  private readonly fb = inject(FormBuilder);
  private readonly tasksService = inject(TasksService);
  private readonly toastService = inject(ToastService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  public readonly statuses: readonly TaskStatus[] = ['ToDo', 'InProgress', 'Done'];
  public readonly priorities: readonly TaskPriority[] = ['Low', 'Medium', 'High'];
  public readonly sortByOptions: readonly TaskSortBy[] = ['CreatedAt', 'DueDate'];
  public readonly sortDirectionOptions: readonly TaskSortDirection[] = ['Asc', 'Desc'];

  public readonly tasks = signal<TaskItem[]>([]);
  public readonly isLoading = signal(false);
  public readonly isSaving = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly activeTaskId = signal<string | null>(null);

  public readonly createForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(120)]],
    description: ['', [Validators.required, Validators.maxLength(1000)]],
    status: this.fb.nonNullable.control<TaskStatus>('ToDo'),
    priority: this.fb.nonNullable.control<TaskPriority>('Medium'),
    dueDate: [''],
  });

  public readonly filterForm = this.fb.nonNullable.group({
    status: this.fb.nonNullable.control<string>(''),
    priority: this.fb.nonNullable.control<string>(''),
    sortedBy: this.fb.nonNullable.control<TaskSortBy>('CreatedAt'),
    sortDirection: this.fb.nonNullable.control<TaskSortDirection>('Asc'),
    page: this.fb.nonNullable.control<number>(1),
    pageSize: this.fb.nonNullable.control<number>(20),
  });

  public readonly editForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(120)]],
    description: ['', [Validators.required, Validators.maxLength(1000)]],
    status: this.fb.nonNullable.control<TaskStatus>('ToDo'),
    priority: this.fb.nonNullable.control<TaskPriority>('Medium'),
    dueDate: [''],
  });

  public readonly hasTasks = computed(() => this.tasks().length > 0);

  constructor() {
    this.loadTasks();
  }

  public loadTasks(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.tasksService
      .getTasks()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (tasks) => this.tasks.set(tasks),
        error: (error: HttpErrorResponse) => this.setError(error, 'Failed to load tasks.'),
      });
  }

  public applyFilters(): void {
    if (this.filterForm.invalid) {
      this.filterForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const value = this.filterForm.getRawValue();
    this.tasksService
      .filterTasks({
        status: value.status ? (value.status as TaskStatus) : undefined,
        priority: value.priority ? (value.priority as TaskPriority) : undefined,
        sortedBy: value.sortedBy,
        sortDirection: value.sortDirection,
        page: value.page,
        pageSize: value.pageSize,
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => this.tasks.set(response.items),
        error: (error: HttpErrorResponse) => this.setError(error, 'Failed to filter tasks.'),
      });
  }

  public clearFilters(): void {
    this.filterForm.reset({
      status: '',
      priority: '',
      sortedBy: 'CreatedAt',
      sortDirection: 'Asc',
      page: 1,
      pageSize: 20,
    });
    this.loadTasks();
  }

  public createTask(): void {
    this.createForm.markAllAsTouched();
    if (this.createForm.invalid || this.isSaving()) {
      return;
    }

    const value = this.createForm.getRawValue();
    const request: CreateTaskRequest = {
      title: value.title.trim(),
      description: value.description.trim(),
      status: value.status,
      priority: value.priority,
      dueDate: value.dueDate ? new Date(value.dueDate).toISOString() : null,
    };

    this.isSaving.set(true);
    this.tasksService
      .createTask(request)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (task) => {
          this.tasks.update((tasks) => [task, ...tasks]);
          this.createForm.reset({
            title: '',
            description: '',
            status: 'ToDo',
            priority: 'Medium',
            dueDate: '',
          });
          this.toastService.showSuccess('Task created.');
        },
        error: (error: HttpErrorResponse) => this.setError(error, 'Failed to create task.'),
      });
  }

  public startEdit(task: TaskItem): void {
    this.activeTaskId.set(task.id);
    this.editForm.reset({
      title: task.title,
      description: task.description,
      status: task.status,
      priority: task.priority,
      dueDate: task.dueDate ? task.dueDate.slice(0, 10) : '',
    });
  }

  public cancelEdit(): void {
    this.activeTaskId.set(null);
    this.editForm.reset({
      title: '',
      description: '',
      status: 'ToDo',
      priority: 'Medium',
      dueDate: '',
    });
  }

  public saveEdit(task: TaskItem): void {
    this.editForm.markAllAsTouched();
    if (this.editForm.invalid || this.isSaving()) {
      return;
    }

    const value = this.editForm.getRawValue();
    const request: UpdateTaskRequest = {
      id: task.id,
      ownerId: task.ownerId,
      createdAt: task.createdAt,
      title: value.title.trim(),
      description: value.description.trim(),
      status: value.status,
      priority: value.priority,
      dueDate: value.dueDate ? new Date(value.dueDate).toISOString() : null,
    };

    this.isSaving.set(true);
    this.tasksService
      .updateTask(request)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.tasks.update((tasks) =>
            tasks.map((current) =>
              current.id === task.id
                ? { ...request }
                : current
            )
          );
          this.cancelEdit();
          this.toastService.showSuccess('Task updated.');
        },
        error: (error: HttpErrorResponse) => this.setError(error, 'Failed to update task.'),
      });
  }

  public deleteTask(taskId: string): void {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.tasksService
      .deleteTask(taskId)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.tasks.update((tasks) => tasks.filter((task) => task.id !== taskId));
          this.toastService.showSuccess('Task removed.');
        },
        error: (error: HttpErrorResponse) => this.setError(error, 'Failed to remove task.'),
      });
  }

  public logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  private setError(error: HttpErrorResponse, fallbackMessage: string): void {
    const problem = error.error as ProblemDetails | undefined;
    this.errorMessage.set(problem?.detail ?? fallbackMessage);
  }
}

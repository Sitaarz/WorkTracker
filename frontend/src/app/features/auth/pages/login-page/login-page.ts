import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { AuthStore } from '../../../../core/auth/auth.store';
import { ProblemDetails } from '../../../../shared/models/errors';
import { ToastService } from '../../../../shared/ui/toast.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toastService = inject(ToastService);

  public readonly isSubmitting = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly fieldErrors = signal<Record<string, string[]>>({});

  public readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  public submit(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.fieldErrors.set({});

    this.authService
      .login({
        email: this.form.controls.email.getRawValue(),
        password: this.form.controls.password.getRawValue(),
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.authStore.setAuthenticated(response.user);
          this.toastService.showSuccess(`Welcome back, ${response.user.name}!`);
          const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
          this.router.navigateByUrl(returnUrl || '/');
        },
        error: (error: HttpErrorResponse) => {
          const problem = error.error as ProblemDetails | undefined;
          this.setFieldErrors(problem?.errors);
          this.errorMessage.set(problem?.detail ?? 'Login failed. Please try again.');
        },
      });
  }

  public getFieldError(field: string): string | null {
    return this.fieldErrors()[field]?.[0] ?? null;
  }

  private setFieldErrors(errors: Record<string, string[]> | undefined): void {
    if (!errors) {
      this.fieldErrors.set({});
      return;
    }

    const mappedErrors: Record<string, string[]> = {};

    for (const [rawField, messages] of Object.entries(errors)) {
      const normalizedField = rawField.trim().toLowerCase();

      if (normalizedField === 'email') {
        mappedErrors['email'] = messages;
      } else if (normalizedField === 'password') {
        mappedErrors['password'] = messages;
      }
    }

    this.fieldErrors.set(mappedErrors);
  }
}

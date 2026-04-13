import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { AuthStore } from '../../../../core/auth/auth.store';
import { ProblemDetails } from '../../../../shared/models/errors';

const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password')?.value as string | undefined;
  const confirmPassword = control.get('confirmPassword')?.value as string | undefined;

  if (!password || !confirmPassword) {
    return null;
  }

  return password === confirmPassword ? null : { passwordMismatch: true };
};

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register-page.html',
  styleUrl: './register-page.scss',
})
export class RegisterPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  public readonly isSubmitting = signal(false);
  public readonly errorMessage = signal<string | null>(null);
  public readonly fieldErrors = signal<Record<string, string[]>>({});

  public readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]],
  }, { validators: passwordMatchValidator });

  public submit(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.fieldErrors.set({});

    this.authService
      .register({
        name: this.form.controls.name.getRawValue(),
        email: this.form.controls.email.getRawValue(),
        password: this.form.controls.password.getRawValue(),
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.authStore.setAuthenticated(response.user);
          this.router.navigateByUrl('/');
        },
        error: (error: HttpErrorResponse) => {
          const problem = error.error as ProblemDetails | undefined;
          this.setFieldErrors(problem?.errors);
          this.errorMessage.set(problem?.detail ?? 'Registration failed. Please try again.');
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

      if (normalizedField === 'name') {
        mappedErrors['name'] = messages;
      } else if (normalizedField === 'email') {
        mappedErrors['email'] = messages;
      } else if (normalizedField === 'password') {
        mappedErrors['password'] = messages;
      } else if (normalizedField === 'confirmpassword' || normalizedField === 'confirm_password') {
        mappedErrors['confirmPassword'] = messages;
      }
    }

    this.fieldErrors.set(mappedErrors);
  }
}

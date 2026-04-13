import { inject, Injectable } from '@angular/core';
import { AuthStore } from './auth.store';
import { HttpClient } from '@angular/common/http';
import { ApiEndpoints } from '../api/api.endpoints';
import { environment } from '../../../environments/environment';
import { CurretUser } from './auth.store';
import { Router } from '@angular/router';
import { ProblemDetails } from '../../shared/models/errors';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  password: string;
  email: string;
}

interface AuthResponse {
  user: CurretUser;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly _authStore: AuthStore = inject(AuthStore);
  private readonly http: HttpClient = inject(HttpClient);
  private readonly router: Router = inject(Router);

  private navigateAfterAuthSuccess() {
    const returnUrl = this.router.parseUrl(this.router.url).queryParams['returnUrl'] as string | undefined;
    this.router.navigateByUrl(returnUrl || '/');
  }

  public login(request: LoginRequest) {
    return this.http.post<AuthResponse>(environment.apiBaseUrl + ApiEndpoints.auth.login, request)
  }

  public register(request: RegisterRequest) {
    return this.http.post<AuthResponse>(environment.apiBaseUrl + ApiEndpoints.auth.register, request)
  }

  public me() {
    return this.http.get<AuthResponse>(environment.apiBaseUrl + ApiEndpoints.auth.me).subscribe({
      next: (authResponse) => this._authStore.setAuthenticated(authResponse.user),
      error: (error: ProblemDetails) => {
        console.warn('Fetching current user failed', error);
        this._authStore.clear();
      }
    });
  }

  public logout() {
    this._authStore.clear();
  }
}

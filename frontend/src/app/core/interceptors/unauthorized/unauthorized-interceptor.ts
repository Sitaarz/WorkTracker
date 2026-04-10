import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { TokenStorageService } from '../../../features/auth/services/token-storage-service/token-storage-service';
import { catchError, throwError } from 'rxjs';

export const unauthorizedInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const tokenStorage = inject(TokenStorageService);
  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        tokenStorage.clearToken();

        const isOnLoginPage = router.url.startsWith('/login');
        if (!isOnLoginPage) {
          router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
        }
      }
      return throwError(() => error);
    })
  );
};

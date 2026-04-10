import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenStorageService } from '../../../features/auth/services/token-storage-service/token-storage-service';

export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenStorageService);
  const token = tokenService.getToken();

  const isApiCall = req.url.startsWith('/api/');

  if (isApiCall && token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req);
};

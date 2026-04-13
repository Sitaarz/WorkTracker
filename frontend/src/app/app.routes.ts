import { Routes } from '@angular/router';
import { authGuard, guestOnlyGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: "login",
    canActivate: [guestOnlyGuard],
    loadComponent: () => import("./features/auth/pages/login-page/login-page").then(m => m.LoginPage)
  },
  {
    path: "register",
    canActivate: [guestOnlyGuard],
    loadComponent: () => import("./features/auth/pages/register-page/register-page").then(m => m.RegisterPage)
  },
  {
    path: "",
    canActivate: [authGuard],
    loadComponent: () => import("./features/tasks/pages/tasks-page/tasks-page").then(m => m.TasksPage)
  },
  {
    path: "**",
    redirectTo: ""
  }
];

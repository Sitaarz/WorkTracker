import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: "login",
    canActivate: [() => import("./core/auth/auth.guard").then(m => m.guestOnlyGuard)],
    loadComponent: () => import("./features/auth/pages/login-page/login-page").then(m => m.LoginPage)
  },
  {
    path: "register",
    canActivate: [() => import("./core/auth/auth.guard").then(m => m.guestOnlyGuard)],
    loadComponent: () => import("./features/auth/pages/register-page/register-page").then(m => m.RegisterPage)
  },
  {
    path: "",
    canActivate: [() => import("./core/auth/auth.guard").then(m => m.authGuard)],
    loadComponent: () => import("./features/tasks/pages/tasks-page/tasks-page").then(m => m.TasksPage)
  },
  {
    path: "**",
    redirectTo: ""
  }
];

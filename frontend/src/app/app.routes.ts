import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: "login",
    loadComponent: () => import("./features/auth/pages/login-page/login-page").then(m => m.LoginPage)
  },
  {
    path: "register",
    loadComponent: () => import("./features/auth/pages/register-page/register-page").then(m => m.RegisterPage)
  },
  {
    canActivate: [() => import("./core/auth/auth.guard").then(m => m.authGuard)],
    path: "tasks",
    loadComponent: () => import("./features/tasks/pages/tasks-page/tasks-page").then(m => m.TasksPage)

  },
  {
    path: "",
    redirectTo: "tasks",
    pathMatch: "full"
  }
];

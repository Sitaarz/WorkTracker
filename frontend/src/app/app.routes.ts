import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: "login",
    loadComponent: () => import("./features/auth/pages/login-page/login-page").then(m => m.LoginPage)
  },
  {
    path: "tasks",
    loadComponent: () => import("./features/tasks/pages/tasks-page/tasks-page").then(m => m.TasksPage)

  },
  {
    path: "",
    redirectTo: "tasks",
    pathMatch: "full"
  }
];

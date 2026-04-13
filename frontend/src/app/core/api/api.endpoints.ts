import { filter } from "rxjs";

export const ApiEndpoints = {
  auth: {
    login: 'v1/auth/login',
    register: 'v1/auth/register',
    me: 'v1/auth/me'
  },
  tasks: {
    tasks: 'v1/tasks',
    task: (id: number) => `v1/tasks/${id}`,
    filter: 'v1/tasks/filter'
  }
}

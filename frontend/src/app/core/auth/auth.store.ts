import { computed, Injectable, signal, Signal } from '@angular/core';

export interface CurretUser {
  name: string;
  email: string;
  roles: string[];
}

export type AuthStatus = "authenticated" | "unauthenticated" | "unknown";

@Injectable({
  providedIn: 'root',
})
export class AuthStore {
  public readonly currentUser = signal<CurretUser | null>(null);
  public readonly authStatus= signal<AuthStatus>("unknown");
  public readonly isAuthenticated = computed(() => this.authStatus() === "authenticated");
  public readonly hasRole = (role: string) => computed(() => this.currentUser()?.roles.includes(role) ?? false);

  public setAuthenticated(user: CurretUser): void {
    this.currentUser.set(user);
    this.authStatus.set("authenticated");
  }

  public clear(): void {
    this.currentUser.set(null);
    this.authStatus.set("unauthenticated");
  }
}

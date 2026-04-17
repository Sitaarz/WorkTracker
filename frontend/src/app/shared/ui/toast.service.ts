import { Injectable, signal } from '@angular/core';

type ToastType = 'success' | 'error' | 'info';

export interface ToastMessage {
  message: string;
  type: ToastType;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  public readonly currentToast = signal<ToastMessage | null>(null);
  private hideTimerId: ReturnType<typeof setTimeout> | null = null;

  public showSuccess(message: string, durationMs = 3000): void {
    this.show({ message, type: 'success' }, durationMs);
  }

  public clear(): void {
    this.currentToast.set(null);

    if (this.hideTimerId) {
      clearTimeout(this.hideTimerId);
      this.hideTimerId = null;
    }
  }

  private show(toast: ToastMessage, durationMs: number): void {
    this.clear();
    this.currentToast.set(toast);
    this.hideTimerId = setTimeout(() => this.clear(), durationMs);
  }
}

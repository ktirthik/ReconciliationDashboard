import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

interface LoginResponse { token: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'rd_token';
  private readonly http = inject(HttpClient);

  isLoggedIn = signal(this.hasToken());

  login(username: string, password: string) {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, { username, password }).pipe(
      tap(res => {
        try { localStorage.setItem(this.TOKEN_KEY, res.token); } catch {}
        this.isLoggedIn.set(true);
      })
    );
  }

  logout() {
    try { localStorage.removeItem(this.TOKEN_KEY); } catch {}
    this.isLoggedIn.set(false);
  }

  getToken(): string | null {
    try { return localStorage.getItem(this.TOKEN_KEY); } catch { return null; }
  }

  private hasToken(): boolean {
    try { return !!localStorage.getItem(this.TOKEN_KEY); } catch { return false; }
  }
}

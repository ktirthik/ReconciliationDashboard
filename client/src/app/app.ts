import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet, Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    @if (auth.isLoggedIn()) {
      <nav class="nav">
        <a class="nav-brand" routerLink="/">Reconciliation Dashboard</a>
        <div class="nav-links">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}">Dashboard</a>
          <a routerLink="/accounts" routerLinkActive="active">Accounts</a>
        </div>
        <button class="logout-btn" (click)="logout()">Sign out</button>
      </nav>
    }
    <main [class.main-content]="auth.isLoggedIn()">
      <router-outlet />
    </main>
  `,
  styles: [`
    .nav {
      display: flex; align-items: center; gap: 2rem;
      padding: 0 2rem; height: 56px;
      background: var(--surface); border-bottom: 1px solid var(--border);
      position: sticky; top: 0; z-index: 100;
    }
    .nav-brand { font-weight: 700; font-size: 1rem; color: var(--primary); text-decoration: none; white-space: nowrap; }
    .nav-links { display: flex; gap: 1.5rem; flex: 1; }
    .nav-links a { text-decoration: none; color: var(--text-muted); font-size: 0.9rem; padding: 4px 0; border-bottom: 2px solid transparent; }
    .nav-links a.active { color: var(--primary); border-bottom-color: var(--primary); }
    .logout-btn { background: none; border: 1px solid var(--border); border-radius: 4px; padding: 4px 12px; cursor: pointer; font-size: 0.85rem; color: var(--text-muted); }
    .logout-btn:hover { border-color: var(--primary); color: var(--primary); }
    .main-content { max-width: 1200px; margin: 0 auto; padding: 2rem; }
  `]
})
export class App {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}

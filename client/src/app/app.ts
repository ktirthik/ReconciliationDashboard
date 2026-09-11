import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="nav">
      <a class="nav-brand" routerLink="/">Reconciliation Dashboard</a>
      <div class="nav-links">
        <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <a routerLink="/accounts" routerLinkActive="active">Accounts</a>
      </div>
    </nav>
    <main class="main-content">
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
    .nav-links { display: flex; gap: 1.5rem; }
    .nav-links a { text-decoration: none; color: var(--text-muted); font-size: 0.9rem; padding: 4px 0; border-bottom: 2px solid transparent; }
    .nav-links a.active { color: var(--primary); border-bottom-color: var(--primary); }
    .main-content { max-width: 1200px; margin: 0 auto; padding: 2rem; }
  `]
})
export class App {}

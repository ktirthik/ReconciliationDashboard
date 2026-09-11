import { Component, input } from '@angular/core';
import { AccountStatus } from '../../core/models/account.model';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span class="badge badge-{{ status().toLowerCase() }}">{{ label() }}</span>`,
  styles: [`
    .badge { display: inline-block; padding: 2px 10px; border-radius: 12px; font-size: 0.78rem; font-weight: 600; letter-spacing: 0.02em; }
    .badge-active  { background: #d1e7dd; color: #0f5132; }
    .badge-flagged { background: #fff3cd; color: #856404; }
    .badge-atrisk  { background: #f8d7da; color: #842029; }
    .badge-closed  { background: #e2e3e5; color: #41464b; }
  `]
})
export class StatusBadgeComponent {
  status = input.required<AccountStatus>();
  get label() { return () => this.status() === 'AtRisk' ? 'At Risk' : this.status(); }
}

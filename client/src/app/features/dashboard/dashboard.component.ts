import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Account } from '../../core/models/account.model';
import { AccountService } from '../../core/services/account.service';
import { StatusBadgeComponent } from '../../shared/status-badge/status-badge.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly accountService = inject(AccountService);

  allAccounts = signal<Account[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  get flagged() { return this.allAccounts().filter(a => a.status === 'Flagged'); }
  get atRisk()  { return this.allAccounts().filter(a => a.status === 'AtRisk'); }
  get active()  { return this.allAccounts().filter(a => a.status === 'Active'); }
  get closed()  { return this.allAccounts().filter(a => a.status === 'Closed'); }

  ngOnInit() {
    this.accountService.getAll().subscribe({
      next: accounts => { this.allAccounts.set(accounts); this.loading.set(false); },
      error: () => { this.error.set('Failed to load accounts.'); this.loading.set(false); }
    });
  }
}

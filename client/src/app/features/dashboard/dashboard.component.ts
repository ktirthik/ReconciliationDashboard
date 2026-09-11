import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Account } from '../../core/models/account.model';
import { AccountService } from '../../core/services/account.service';
import { StatusBadgeComponent } from '../../shared/status-badge/status-badge.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, DatePipe, StatusBadgeComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly accountService = inject(AccountService);

  allAccounts = signal<Account[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  flagged  = computed(() => this.allAccounts().filter(a => a.status === 'Flagged'));
  atRisk   = computed(() => this.allAccounts().filter(a => a.status === 'AtRisk'));
  active   = computed(() => this.allAccounts().filter(a => a.status === 'Active'));
  closed   = computed(() => this.allAccounts().filter(a => a.status === 'Closed'));

  recentlyUpdated = computed(() =>
    [...this.allAccounts()]
      .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
      .slice(0, 5)
  );

  ngOnInit() {
    this.accountService.getAll().subscribe({
      next: accounts => { this.allAccounts.set(accounts); this.loading.set(false); },
      error: () => { this.error.set('Failed to load accounts.'); this.loading.set(false); }
    });
  }
}

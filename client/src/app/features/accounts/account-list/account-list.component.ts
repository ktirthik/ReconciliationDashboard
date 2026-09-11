import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Account, AccountStatus } from '../../../core/models/account.model';
import { AccountService } from '../../../core/services/account.service';
import { StatusBadgeComponent } from '../../../shared/status-badge/status-badge.component';

@Component({
  selector: 'app-account-list',
  standalone: true,
  imports: [RouterLink, FormsModule, DatePipe, StatusBadgeComponent],
  templateUrl: './account-list.component.html',
  styleUrl: './account-list.component.scss'
})
export class AccountListComponent implements OnInit {
  private readonly accountService = inject(AccountService);

  accounts = signal<Account[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  statusFilter = signal<AccountStatus | ''>('');
  searchQuery = signal('');

  readonly statuses: Array<AccountStatus | ''> = ['', 'Active', 'Flagged', 'AtRisk', 'Closed'];

  filtered = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (!q) return this.accounts();
    return this.accounts().filter(a =>
      a.accountNumber.toLowerCase().includes(q) ||
      a.customerName.toLowerCase().includes(q)
    );
  });

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    const status = this.statusFilter() || undefined;
    this.accountService.getAll(status as AccountStatus | undefined).subscribe({
      next: accounts => { this.accounts.set(accounts); this.loading.set(false); },
      error: () => { this.error.set('Failed to load accounts.'); this.loading.set(false); }
    });
  }

  onFilterChange() { this.load(); }

  exportCsv() {
    const rows = this.filtered();
    const header = ['Account Number', 'Customer Name', 'Status', 'Flag Reason', 'Created', 'Last Updated'];
    const lines = rows.map(a => [
      a.accountNumber,
      `"${a.customerName.replace(/"/g, '""')}"`,
      a.status,
      a.flagReason ? `"${a.flagReason.replace(/"/g, '""')}"` : '',
      new Date(a.createdAt).toISOString().slice(0, 10),
      new Date(a.updatedAt).toISOString().slice(0, 10)
    ].join(','));
    const csv = [header.join(','), ...lines].join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `accounts-${new Date().toISOString().slice(0, 10)}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }
}

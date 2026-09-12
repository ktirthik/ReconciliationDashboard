import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Account, AccountStatus } from '../../../core/models/account.model';
import { AccountService } from '../../../core/services/account.service';
import { StatusBadgeComponent } from '../../../shared/status-badge/status-badge.component';

type SortField = 'accountNumber' | 'customerName' | 'status' | 'updatedAt';
type SortDir = 'asc' | 'desc';

const STATUS_ORDER: Record<AccountStatus, number> = { Flagged: 0, AtRisk: 1, Active: 2, Closed: 3 };

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
  sortField = signal<SortField>('accountNumber');
  sortDir = signal<SortDir>('asc');

  readonly statuses: Array<AccountStatus | ''> = ['', 'Active', 'Flagged', 'AtRisk', 'Closed'];

  filtered = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    const field = this.sortField();
    const dir = this.sortDir();

    let rows = q
      ? this.accounts().filter(a =>
          a.accountNumber.toLowerCase().includes(q) ||
          a.customerName.toLowerCase().includes(q)
        )
      : [...this.accounts()];

    rows.sort((a, b) => {
      let cmp = 0;
      if (field === 'status') {
        cmp = (STATUS_ORDER[a.status] ?? 99) - (STATUS_ORDER[b.status] ?? 99);
      } else if (field === 'updatedAt') {
        cmp = new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime();
      } else {
        cmp = a[field].localeCompare(b[field]);
      }
      return dir === 'asc' ? cmp : -cmp;
    });

    return rows;
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

  sort(field: SortField) {
    if (this.sortField() === field) {
      this.sortDir.update(d => d === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortField.set(field);
      this.sortDir.set('asc');
    }
  }

  sortIcon(field: SortField): string {
    if (this.sortField() !== field) return '↕';
    return this.sortDir() === 'asc' ? '↑' : '↓';
  }

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

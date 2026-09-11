import { Component, OnInit, inject, signal } from '@angular/core';
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

  readonly statuses: Array<AccountStatus | ''> = ['', 'Active', 'Flagged', 'AtRisk', 'Closed'];

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
}

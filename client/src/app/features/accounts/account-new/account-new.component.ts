import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../../core/services/account.service';

@Component({
  selector: 'app-account-new',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './account-new.component.html',
  styleUrl: './account-new.component.scss'
})
export class AccountNewComponent implements OnInit {
  private readonly accountService = inject(AccountService);
  private readonly router = inject(Router);

  accountNumber = signal('');
  customerName = signal('');
  saving = signal(false);
  error = signal<string | null>(null);
  suggestedNumber = signal('');

  ngOnInit() {
    this.accountService.getAll().subscribe({
      next: accounts => {
        const nums = accounts
          .map(a => parseInt(a.accountNumber.replace(/^ACC-0*/i, ''), 10))
          .filter(n => !isNaN(n));
        const next = nums.length > 0 ? Math.max(...nums) + 1 : 1;
        const suggestion = `ACC-${String(next).padStart(4, '0')}`;
        this.suggestedNumber.set(suggestion);
        this.accountNumber.set(suggestion);
      }
    });
  }

  save() {
    const num = this.accountNumber().trim();
    const name = this.customerName().trim();
    if (!num || !name) { this.error.set('Both fields are required.'); return; }

    this.saving.set(true);
    this.error.set(null);
    this.accountService.create({ accountNumber: num, customerName: name }).subscribe({
      next: created => this.router.navigate(['/accounts', created.id]),
      error: () => { this.error.set('Failed to create account. Account number may already exist.'); this.saving.set(false); }
    });
  }
}

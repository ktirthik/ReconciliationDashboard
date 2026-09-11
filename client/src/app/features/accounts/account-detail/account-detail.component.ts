import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Account, AccountStatus, AiInsight, Correction, UpdateAccountRequest } from '../../../core/models/account.model';
import { AccountService } from '../../../core/services/account.service';
import { StatusBadgeComponent } from '../../../shared/status-badge/status-badge.component';

@Component({
  selector: 'app-account-detail',
  standalone: true,
  imports: [RouterLink, FormsModule, DatePipe, StatusBadgeComponent],
  templateUrl: './account-detail.component.html',
  styleUrl: './account-detail.component.scss'
})
export class AccountDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);

  account = signal<Account | null>(null);
  loading = signal(true);
  saving = signal(false);
  deleting = signal(false);
  error = signal<string | null>(null);
  successMsg = signal<string | null>(null);

  insight = signal<AiInsight | null>(null);
  insightLoading = signal(false);
  insightError = signal<string | null>(null);

  corrections = signal<Correction[]>([]);
  correctionsLoading = signal(false);
  correctionsRunning = signal(false);
  correctionsError = signal<string | null>(null);
  correctionsLoaded = signal(false);

  editMode = signal(false);
  editForm: UpdateAccountRequest = { customerName: '', status: 'Active', flagReason: null };

  readonly statuses: AccountStatus[] = ['Active', 'Flagged', 'AtRisk', 'Closed'];

  get id() { return this.route.snapshot.paramMap.get('id')!; }

  ngOnInit() {
    this.accountService.getById(this.id).subscribe({
      next: a => { this.account.set(a); this.loading.set(false); this.initForm(a); },
      error: () => { this.error.set('Account not found.'); this.loading.set(false); }
    });
  }

  initForm(a: Account) {
    this.editForm = { customerName: a.customerName, status: a.status, flagReason: a.flagReason };
  }

  startEdit() { this.editMode.set(true); this.successMsg.set(null); }
  cancelEdit() { this.editMode.set(false); this.initForm(this.account()!); }

  save() {
    this.saving.set(true);
    this.accountService.update(this.id, this.editForm).subscribe({
      next: updated => {
        this.account.set(updated);
        this.saving.set(false);
        this.editMode.set(false);
        this.successMsg.set('Account updated.');
        setTimeout(() => this.successMsg.set(null), 3000);
      },
      error: () => { this.error.set('Failed to save changes.'); this.saving.set(false); }
    });
  }

  delete() {
    if (!confirm(`Delete account ${this.account()?.accountNumber}? This cannot be undone.`)) return;
    this.deleting.set(true);
    this.accountService.delete(this.id).subscribe({
      next: () => this.router.navigate(['/accounts']),
      error: () => { this.error.set('Failed to delete account.'); this.deleting.set(false); }
    });
  }

  loadInsight() {
    this.insightLoading.set(true);
    this.insightError.set(null);
    this.accountService.getAiInsight(this.id).subscribe({
      next: insight => { this.insight.set(insight); this.insightLoading.set(false); },
      error: () => { this.insightError.set('Failed to load AI insight.'); this.insightLoading.set(false); }
    });
  }

  loadCorrections() {
    this.correctionsLoading.set(true);
    this.correctionsError.set(null);
    this.accountService.getCorrections(this.id).subscribe({
      next: c => { this.corrections.set(c); this.correctionsLoading.set(false); this.correctionsLoaded.set(true); },
      error: () => { this.correctionsError.set('Failed to load corrections.'); this.correctionsLoading.set(false); }
    });
  }

  runEngine() {
    this.correctionsRunning.set(true);
    this.correctionsError.set(null);
    this.accountService.runCorrectionsEngine(this.id).subscribe({
      next: newOnes => {
        this.corrections.update(existing => [...newOnes, ...existing]);
        this.correctionsRunning.set(false);
        this.correctionsLoaded.set(true);
      },
      error: () => { this.correctionsError.set('Failed to run correction engine.'); this.correctionsRunning.set(false); }
    });
  }

  correctionTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      InterestSuppression: 'Interest Suppression',
      MeterRemap: 'Meter Remap',
      ClassificationError: 'Classification Error',
      DebtRestructure: 'Debt Restructure',
      ManualReview: 'Manual Review'
    };
    return labels[type] ?? type;
  }
}

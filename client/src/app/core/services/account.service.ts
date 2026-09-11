import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Account, AccountStatus, AiInsight, CreateAccountRequest, UpdateAccountRequest } from '../models/account.model';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/accounts';

  getAll(status?: AccountStatus): Observable<Account[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<Account[]>(this.base, { params });
  }

  getById(id: string): Observable<Account> {
    return this.http.get<Account>(`${this.base}/${id}`);
  }

  create(request: CreateAccountRequest): Observable<Account> {
    return this.http.post<Account>(this.base, request);
  }

  update(id: string, request: UpdateAccountRequest): Observable<Account> {
    return this.http.put<Account>(`${this.base}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getAiInsight(id: string): Observable<AiInsight> {
    return this.http.get<AiInsight>(`${this.base}/${id}/ai-insight`);
  }
}

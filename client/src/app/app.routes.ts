import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'accounts',
    loadComponent: () => import('./features/accounts/account-list/account-list.component').then(m => m.AccountListComponent)
  },
  {
    path: 'accounts/:id',
    loadComponent: () => import('./features/accounts/account-detail/account-detail.component').then(m => m.AccountDetailComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];

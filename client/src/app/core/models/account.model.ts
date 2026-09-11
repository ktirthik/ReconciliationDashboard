export interface Account {
  id: string;
  accountNumber: string;
  customerName: string;
  status: AccountStatus;
  flagReason: string | null;
  createdAt: string;
  updatedAt: string;
}

export type AccountStatus = 'Active' | 'Flagged' | 'AtRisk' | 'Closed';

export interface AiInsight {
  summary: string;
  similarCases: SimilarCase[];
}

export interface SimilarCase {
  id: string;
  title: string;
  content: string;
  tags: string;
  similarity: number;
}

export interface CreateAccountRequest {
  accountNumber: string;
  customerName: string;
}

export interface UpdateAccountRequest {
  customerName: string;
  status: AccountStatus;
  flagReason: string | null;
}

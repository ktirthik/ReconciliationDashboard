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

export type CorrectionStatus = 'Pending' | 'Applied' | 'Rejected';
export type CorrectionType = 'InterestSuppression' | 'MeterRemap' | 'ClassificationError' | 'DebtRestructure' | 'ManualReview';

export interface Correction {
  id: string;
  accountId: string;
  correctionType: CorrectionType;
  description: string;
  originalValue: string | null;
  correctedValue: string | null;
  status: CorrectionStatus;
  createdAt: string;
  appliedAt: string | null;
}

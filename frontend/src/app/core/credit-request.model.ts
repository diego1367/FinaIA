export type CreditRequestStatus = 'Pending' | 'Approved' | 'Rejected';

export interface CreditRequest {
  id: number;
  documentNumber: string;
  fullName: string;
  requestedAmount: number;
  termMonths: number;
  status: CreditRequestStatus;
  createdAtUtc: string;
}

export interface CreateCreditRequest {
  documentNumber: string;
  fullName: string;
  requestedAmount: number;
  termMonths: number;
}

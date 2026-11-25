import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreditRequest, CreateCreditRequest, CreditRequestStatus } from './credit-request.model';

const API_URL = 'http://localhost:5069/api/credit-requests';

@Injectable({ providedIn: 'root' })
export class CreditRequestService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<CreditRequest[]> {
    return this.http.get<CreditRequest[]>(API_URL);
  }

  create(request: CreateCreditRequest): Observable<CreditRequest> {
    return this.http.post<CreditRequest>(API_URL, request);
  }

  setStatus(id: number, status: Extract<CreditRequestStatus, 'Approved' | 'Rejected'>): Observable<CreditRequest> {
    return this.http.patch<CreditRequest>(`${API_URL}/${id}/status`, { status });
  }
}

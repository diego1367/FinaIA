import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { CreditRequest, CreditRequestStatus } from './core/credit-request.model';
import { CreditRequestService } from './core/credit-request.service';

type Field = 'documentNumber' | 'fullName' | 'requestedAmount' | 'termMonths';

const decimalAmount: ValidatorFn = ({ value }) =>
  value == null || /^\d{1,13}(\.\d{1,2})?$/.test(String(value)) ? null : { precision: true };
const integerMonths: ValidatorFn = ({ value }) =>
  value == null || Number.isInteger(value) ? null : { integer: true };
const trimmedName: ValidatorFn = ({ value }) => {
  const length = String(value ?? '').trim().length;
  return length >= 2 && length <= 100 ? null : { length: true };
};

@Component({
  imports: [ReactiveFormsModule, CurrencyPipe, DatePipe],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  private readonly formBuilder = inject(FormBuilder);
  private readonly creditRequestsApi = inject(CreditRequestService);

  protected readonly requests = signal<CreditRequest[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal('');
  protected readonly saving = signal(false);
  protected readonly pendingAction = signal<number | null>(null);
  protected readonly errorMessage = signal('');
  protected readonly successMessage = signal('');
  protected readonly serverErrors = signal<Partial<Record<Field, string>>>({});
  protected readonly form = this.formBuilder.nonNullable.group({
    documentNumber: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9]{6,20}$/)]],
    fullName: ['', [trimmedName]],
    requestedAmount: this.formBuilder.control<number | null>(null, [Validators.required, Validators.min(0.01), decimalAmount]),
    termMonths: this.formBuilder.control<number | null>(12, [Validators.required, Validators.min(1), Validators.max(120), integerMonths]),
  });

  constructor() {
    this.loadRequests();
  }

  protected loadRequests(): void {
    if (this.loading() || this.saving() || this.pendingAction() !== null) return;
    this.loading.set(true);
    this.loadError.set('');
    this.errorMessage.set('');
    this.creditRequestsApi.getAll().subscribe({
      next: (requests) => {
        this.requests.set(requests);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loadError.set(this.readError(error, 'No fue posible consultar las solicitudes.'));
        this.loading.set(false);
      },
    });
  }

  protected submit(): void {
    if (this.saving() || this.loading()) return;
    this.successMessage.set('');
    this.errorMessage.set('');
    this.serverErrors.set({});
    const values = this.form.getRawValue();
    if (this.form.invalid || values.requestedAmount === null || values.termMonths === null) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.creditRequestsApi.create({
      ...values,
      fullName: values.fullName.trim(),
      requestedAmount: values.requestedAmount,
      termMonths: values.termMonths,
    }).subscribe({
      next: (created) => {
        this.requests.update((requests) => [created, ...requests]);
        this.form.reset({ documentNumber: '', fullName: '', requestedAmount: null, termMonths: 12 });
        this.successMessage.set('Solicitud registrada correctamente.');
        this.saving.set(false);
      },
      error: (error: unknown) => {
        this.showCreateError(error);
        this.saving.set(false);
      },
    });
  }

  protected resolve(request: CreditRequest, status: Extract<CreditRequestStatus, 'Approved' | 'Rejected'>): void {
    if (request.status !== 'Pending' || this.pendingAction() !== null) return;
    this.successMessage.set('');
    this.errorMessage.set('');
    this.pendingAction.set(request.id);
    this.creditRequestsApi.setStatus(request.id, status).subscribe({
      next: (updated) => {
        this.requests.update((requests) => requests.map((item) => item.id === updated.id ? updated : item));
        this.successMessage.set(`Solicitud ${status === 'Approved' ? 'aprobada' : 'rechazada'}.`);
        this.pendingAction.set(null);
      },
      error: (error: unknown) => {
        this.errorMessage.set(this.readError(error, 'No fue posible actualizar el estado.'));
        this.pendingAction.set(null);
      },
    });
  }

  protected clearFieldError(name: Field): void {
    if (this.serverErrors()[name]) this.serverErrors.update((errors) => ({ ...errors, [name]: undefined }));
  }

  protected fieldError(name: Field, message: string): string | undefined {
    const control = this.form.controls[name];
    return this.serverErrors()[name] ?? (control.invalid && (control.dirty || control.touched) ? message : undefined);
  }

  protected statusLabel(status: CreditRequestStatus): string {
    return { Pending: 'Pendiente', Approved: 'Aprobada', Rejected: 'Rechazada' }[status];
  }

  private readError(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse && error.status >= 400 && error.status < 500) {
      const body = error.error;
      if (typeof body === 'object' && body !== null && 'detail' in body && typeof body.detail === 'string') return body.detail;
    }
    return fallback;
  }

  private showCreateError(error: unknown): void {
    const fieldErrors: Partial<Record<Field, string>> = {};
    if (error instanceof HttpErrorResponse && error.status === 400 && error.error?.errors) {
      const fields: Field[] = ['documentNumber', 'fullName', 'requestedAmount', 'termMonths'];
      for (const [key, messages] of Object.entries(error.error.errors)) {
        const field = fields.find((name) => name.toLowerCase() === key.replace(/^\$\./, '').toLowerCase());
        if (field && Array.isArray(messages) && typeof messages[0] === 'string') fieldErrors[field] = messages[0];
      }
    }
    this.serverErrors.set(fieldErrors);
    this.errorMessage.set(Object.keys(fieldErrors).length
      ? 'Revisa los campos señalados.'
      : this.readError(error, 'No fue posible registrar la solicitud.'));
  }
}

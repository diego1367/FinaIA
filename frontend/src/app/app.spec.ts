import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { CreditRequest } from './core/credit-request.model';

const url = 'http://localhost:5069/api/credit-requests';
const request: CreditRequest = {
  id: 1,
  documentNumber: 'DEMO001',
  fullName: 'Persona de prueba',
  requestedAmount: 5000000,
  termMonths: 12,
  status: 'Pending',
  createdAtUtc: '2026-01-01T00:00:00Z',
};

describe('Solicitudes de crédito', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
  });

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('muestra la carga inicial, avisa si falla y permite volver a consultar', () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);
    const initial = http.expectOne({ method: 'GET', url });
    fixture.detectChanges();

    const page = fixture.nativeElement as HTMLElement;
    expect(page.querySelector('.loading-state')?.textContent).toContain('Consultando');
    const retry = page.querySelector<HTMLButtonElement>('button.retry')!;
    expect(retry.disabled).toBe(true);

    initial.flush('Error de prueba', { status: 503, statusText: 'Service Unavailable' });
    fixture.detectChanges();
    expect(page.querySelector('[role="alert"]')?.textContent).toContain('No fue posible consultar');
    expect(page.querySelector('.loading-state')).toBeNull();
    expect(retry.disabled).toBe(false);

    retry.click();
    http.expectOne({ method: 'GET', url }).flush([]);
    fixture.detectChanges();
    expect(page.querySelector('[role="alert"]')).toBeNull();
    expect(page.querySelector('.empty-state')?.textContent).toContain('Aún no hay solicitudes');
  });

  it('muestra los errores de un formulario incompleto sin enviar una solicitud', () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);
    http.expectOne(url).flush([]);
    fixture.detectChanges();

    const page = fixture.nativeElement as HTMLElement;
    page.querySelector('form')?.dispatchEvent(new Event('submit', { cancelable: true }));
    fixture.detectChanges();

    expect(page.querySelector('#document-error')?.textContent).toContain('6 y 20');
    expect(page.querySelector('#amount-error')?.textContent).toContain('monto positivo');
    http.expectNone({ method: 'POST', url });
  });

  it('actualiza la fila y oculta las acciones después de aprobar', () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);
    http.expectOne(url).flush([request]);
    fixture.detectChanges();

    const page = fixture.nativeElement as HTMLElement;
    page.querySelector<HTMLButtonElement>('.decision')?.click();
    const decision = http.expectOne({ method: 'PATCH', url: url + '/1/status' });
    expect(decision.request.body).toEqual({ status: 'Approved' });
    decision.flush({ ...request, status: 'Approved' });
    fixture.detectChanges();

    expect(page.querySelector('.status')?.textContent).toContain('Aprobada');
    expect(page.querySelector('.decision')).toBeNull();
    expect(page.querySelector('.success-message')?.textContent).toContain('Solicitud aprobada');
    http.expectNone(url);
  });

  it('incorpora a la tabla la solicitud confirmada por la API', () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);
    http.expectOne(url).flush([]);
    fixture.detectChanges();

    const page = fixture.nativeElement as HTMLElement;
    const fields = {
      documentNumber: request.documentNumber,
      fullName: request.fullName,
      requestedAmount: String(request.requestedAmount),
      termMonths: String(request.termMonths),
    };
    for (const [id, value] of Object.entries(fields)) {
      const input = page.querySelector<HTMLInputElement>('#' + id)!;
      input.value = value;
      input.dispatchEvent(new Event('input'));
    }
    page.querySelector('form')?.dispatchEvent(new Event('submit', { cancelable: true }));
    const create = http.expectOne({ method: 'POST', url });
    expect(create.request.body).toEqual({ ...fields, requestedAmount: 5000000, termMonths: 12 });
    expect(page.querySelector('tbody')).toBeNull();

    create.flush(request, { status: 201, statusText: 'Created' });
    fixture.detectChanges();

    expect(page.querySelector('tbody')?.textContent).toContain(request.fullName);
    expect(page.querySelector('.status')?.textContent).toContain('Pendiente');
    http.expectNone(url);
  });

  it('muestra un conflicto de la API y conserva el estado confirmado de la fila', () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);
    http.expectOne(url).flush([request]);
    fixture.detectChanges();

    const page = fixture.nativeElement as HTMLElement;
    const reject = page.querySelector<HTMLButtonElement>('.decision.reject')!;
    reject.click();
    const decision = http.expectOne({ method: 'PATCH', url: url + '/1/status' });
    fixture.detectChanges();
    expect(decision.request.body).toEqual({ status: 'Rejected' });
    expect(reject.disabled).toBe(true);

    decision.flush({ detail: 'Solo puedes aprobar o rechazar una solicitud pendiente.' }, {
      status: 409,
      statusText: 'Conflict',
    });
    fixture.detectChanges();

    expect(page.querySelector('[role="alert"]')?.textContent).toContain('Solo puedes aprobar o rechazar');
    expect(page.querySelector('.status')?.textContent).toContain('Pendiente');
    expect(reject.disabled).toBe(false);
  });
});

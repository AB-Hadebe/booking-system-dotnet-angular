import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ServiceOption {
  id: number;
  name: string;
  durationMinutes: number;
  price: number;
}

export interface Provider {
  id: number;
  name: string;
  specialty: string | null;
}

export interface AvailabilitySlot {
  id: number;
  providerId: number;
  startTime: string;
  endTime: string;
}

export interface CreateBookingRequest {
  customerName: string;
  customerEmail: string;
  serviceId: number;
  availabilitySlotId: number;
}

export interface BookingResponse {
  id: number;
  customerName: string;
  providerName: string;
  serviceName: string;
  startTime: string;
  endTime: string;
  status: string;
}

// Thin wrapper around the Booking API's HTTP endpoints. Components should
// depend on this rather than calling HttpClient directly, so the API surface
// (routes, request/response shapes) lives in one place.
@Injectable({ providedIn: 'root' })
export class BookingApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getServices(): Observable<ServiceOption[]> {
    return this.http.get<ServiceOption[]>(`${this.baseUrl}/services`);
  }

  getProviders(): Observable<Provider[]> {
    return this.http.get<Provider[]>(`${this.baseUrl}/providers`);
  }

  getOpenSlots(providerId?: number): Observable<AvailabilitySlot[]> {
    const url = providerId
      ? `${this.baseUrl}/availability?providerId=${providerId}`
      : `${this.baseUrl}/availability`;
    return this.http.get<AvailabilitySlot[]>(url);
  }

  createBooking(request: CreateBookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.baseUrl}/bookings`, request);
  }

  cancelBooking(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/bookings/${id}`);
  }
}

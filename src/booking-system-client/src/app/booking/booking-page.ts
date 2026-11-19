import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AvailabilitySlot,
  BookingApiService,
  BookingResponse,
  Provider,
  ServiceOption
} from '../core/booking-api.service';

interface SlotDay {
  label: string;
  slots: AvailabilitySlot[];
}

@Component({
  selector: 'app-booking-page',
  imports: [ReactiveFormsModule, DatePipe, CurrencyPipe],
  templateUrl: './booking-page.html',
  styleUrl: './booking-page.scss'
})
export class BookingPage implements OnInit {
  private readonly api = inject(BookingApiService);
  private readonly fb = inject(FormBuilder);

  // Data loaded from the API
  protected readonly services = signal<ServiceOption[]>([]);
  protected readonly providers = signal<Provider[]>([]);
  protected readonly slots = signal<AvailabilitySlot[]>([]);

  // What the user has picked so far
  protected readonly selectedService = signal<ServiceOption | null>(null);
  protected readonly selectedProvider = signal<Provider | null>(null);
  protected readonly selectedSlot = signal<AvailabilitySlot | null>(null);

  // Screen state
  protected readonly loading = signal(true);
  protected readonly loadingSlots = signal(false);
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly booking = signal<BookingResponse | null>(null);
  protected readonly cancelled = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    customerName: ['', [Validators.required, Validators.maxLength(100)]],
    customerEmail: ['', [Validators.required, Validators.email]]
  });

  // Groups the flat list of slots by calendar day (in the user's local time),
  // so the template can show "Thursday 19 November" headings.
  protected readonly slotDays = computed<SlotDay[]>(() => {
    const groups = new Map<string, AvailabilitySlot[]>();
    for (const slot of this.slots()) {
      const label = new Date(slot.startTime).toLocaleDateString(undefined, {
        weekday: 'long',
        day: 'numeric',
        month: 'long'
      });
      const existing = groups.get(label) ?? [];
      existing.push(slot);
      groups.set(label, existing);
    }
    return Array.from(groups, ([label, slots]) => ({ label, slots }));
  });

  ngOnInit(): void {
    this.api.getServices().subscribe({
      next: (services) => this.services.set(services),
      error: () => this.showApiDown()
    });

    this.api.getProviders().subscribe({
      next: (providers) => {
        this.providers.set(providers);
        this.loading.set(false);
      },
      error: () => {
        this.showApiDown();
        this.loading.set(false);
      }
    });
  }

  selectService(service: ServiceOption): void {
    this.selectedService.set(service);
    this.errorMessage.set(null);
  }

  selectProvider(provider: Provider): void {
    this.selectedProvider.set(provider);
    this.selectedSlot.set(null);
    this.errorMessage.set(null);
    this.loadSlots(provider.id);
  }

  selectSlot(slot: AvailabilitySlot): void {
    this.selectedSlot.set(slot);
    this.errorMessage.set(null);
  }

  submit(): void {
    const service = this.selectedService();
    const slot = this.selectedSlot();
    if (this.form.invalid || !service || !slot) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { customerName, customerEmail } = this.form.getRawValue();
    this.api
      .createBooking({
        customerName,
        customerEmail,
        serviceId: service.id,
        availabilitySlotId: slot.id
      })
      .subscribe({
        next: (booking) => {
          this.booking.set(booking);
          this.submitting.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          if (err.status === 409) {
            // Someone else grabbed the slot first - refresh the list.
            this.errorMessage.set('Sorry, that time was just taken. Please pick another slot.');
            this.selectedSlot.set(null);
            const provider = this.selectedProvider();
            if (provider) {
              this.loadSlots(provider.id);
            }
          } else if (err.status === 0) {
            this.showApiDown();
          } else {
            this.errorMessage.set('Something went wrong creating your booking. Please try again.');
          }
        }
      });
  }

  cancelBooking(): void {
    const booking = this.booking();
    if (!booking) {
      return;
    }

    this.api.cancelBooking(booking.id).subscribe({
      next: () => this.cancelled.set(true),
      error: () => this.errorMessage.set('Could not cancel the booking. Please try again.')
    });
  }

  startOver(): void {
    this.selectedService.set(null);
    this.selectedProvider.set(null);
    this.selectedSlot.set(null);
    this.slots.set([]);
    this.booking.set(null);
    this.cancelled.set(false);
    this.errorMessage.set(null);
    this.form.reset();
  }

  private loadSlots(providerId: number): void {
    this.loadingSlots.set(true);
    this.api.getOpenSlots(providerId).subscribe({
      next: (slots) => {
        this.slots.set(slots);
        this.loadingSlots.set(false);
      },
      error: () => {
        this.loadingSlots.set(false);
        this.showApiDown();
      }
    });
  }

  private showApiDown(): void {
    this.errorMessage.set(
      'Could not reach the API. Is BookingSystem.Api running on http://localhost:5017?'
    );
  }
}

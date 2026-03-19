import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ThemeSharedModule } from '@abp/ng.theme.shared';

import {
  CustomerDto,
  CustomerService,
  CreateUpdateCustomerDto,
} from '../proxy/customers';

@Component({
  selector: 'app-customer',
  standalone: true,
  templateUrl: './customer.component.html',
  imports: [CommonModule, ReactiveFormsModule, ThemeSharedModule],
  providers: [ListService],
})
export class CustomerComponent implements OnInit {
  customers = { items: [], totalCount: 0 } as PagedResultDto<CustomerDto>;
  selectedCustomer = {} as CustomerDto;

  form!: FormGroup;
  isModalOpen = false;

  public readonly list = inject(ListService);
  private readonly customerService = inject(CustomerService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);

  ngOnInit(): void {
    const customerStreamCreator = query =>
      this.customerService.getList({
        ...query,
        maxResultCount: 100,
      });

    this.list.hookToQuery(customerStreamCreator).subscribe(response => {
      this.customers = response;
    });
  }

  buildForm(): void {
    this.form = this.fb.group({
      name: [this.selectedCustomer.name || '', [Validators.required, Validators.maxLength(128)]],
      phone: [this.selectedCustomer.phone || '', [Validators.maxLength(32)]],
      address: [this.selectedCustomer.address || '', [Validators.maxLength(256)]],
      gender: [this.selectedCustomer.gender || '', [Validators.maxLength(32)]],
      dateOfBirth: [this.toDateInputValue(this.selectedCustomer.dateOfBirth)],
      patientCode: [this.selectedCustomer.patientCode || '', [Validators.maxLength(64)]],
      isActive: [this.selectedCustomer.isActive ?? true],
    });
  }

  createCustomer(): void {
    this.selectedCustomer = {} as CustomerDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  editCustomer(id: string): void {
    this.customerService.get(id).subscribe(customer => {
      this.selectedCustomer = customer;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const rawValue = this.form.value;
    const input: CreateUpdateCustomerDto = {
      ...rawValue,
      dateOfBirth: rawValue.dateOfBirth ? new Date(rawValue.dateOfBirth).toISOString() : undefined,
    };

    const request = this.selectedCustomer.id
      ? this.customerService.update(this.selectedCustomer.id, input)
      : this.customerService.create(input);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.list.get();
    });
  }

  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.customerService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  private toDateInputValue(value?: string): string | null {
    if (!value) return null;
    return value.substring(0, 10);
  }
}
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ThemeSharedModule } from '@abp/ng.theme.shared';

import {
  SupplierDto,
  SupplierService,
  CreateUpdateSupplierDto,
} from '../proxy/suppliers';

@Component({
  selector: 'app-supplier',
  standalone: true,
  templateUrl: './supplier.component.html',
  imports: [CommonModule, ReactiveFormsModule, ThemeSharedModule],
  providers: [ListService],
})
export class SupplierComponent implements OnInit {
  suppliers = { items: [], totalCount: 0 } as PagedResultDto<SupplierDto>;
  selectedSupplier = {} as SupplierDto;

  form!: FormGroup;
  isModalOpen = false;

  public readonly list = inject(ListService);
  private readonly supplierService = inject(SupplierService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);

  ngOnInit(): void {
    const supplierStreamCreator = query =>
      this.supplierService.getList({
        ...query,
        maxResultCount: 100,
      });

    this.list.hookToQuery(supplierStreamCreator).subscribe(response => {
      this.suppliers = response;
    });
  }

  buildForm(): void {
    this.form = this.fb.group({
      name: [this.selectedSupplier.name || '', [Validators.required, Validators.maxLength(128)]],
      contactPerson: [this.selectedSupplier.contactPerson || '', [Validators.maxLength(128)]],
      phone: [this.selectedSupplier.phone || '', [Validators.maxLength(32)]],
      email: [this.selectedSupplier.email || '', [Validators.email, Validators.maxLength(128)]],
      address: [this.selectedSupplier.address || '', [Validators.maxLength(256)]],
      isActive: [this.selectedSupplier.isActive ?? true],
    });
  }

  createSupplier(): void {
    this.selectedSupplier = {} as SupplierDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  editSupplier(id: string): void {
    this.supplierService.get(id).subscribe(supplier => {
      this.selectedSupplier = supplier;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const input = this.form.value as CreateUpdateSupplierDto;

    const request = this.selectedSupplier.id
      ? this.supplierService.update(this.selectedSupplier.id, input)
      : this.supplierService.create(input);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.list.get();
    });
  }

  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.supplierService.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
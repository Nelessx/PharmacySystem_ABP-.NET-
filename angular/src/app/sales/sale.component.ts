import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ThemeSharedModule } from '@abp/ng.theme.shared';

import {
  CreateUpdateSaleDto,
  CreateUpdateSaleItemDto,
  CustomerLookupDto,
  MedicineLookupDto,
  SaleDto,
  SaleService,
} from '../proxy/sales';

@Component({
  selector: 'app-sale',
  standalone: true,
  templateUrl: './sale.component.html',
  imports: [CommonModule, ReactiveFormsModule, ThemeSharedModule],
  providers: [ListService],
})
export class SaleComponent implements OnInit {
  // List page data
  sales = { items: [], totalCount: 0 } as PagedResultDto<SaleDto>;

  // Selected sale for edit
  selectedSale = {} as SaleDto;

  // Main form
  form!: FormGroup;

  // Modal flag
  isModalOpen = false;

  // Lookup data
  customers: CustomerLookupDto[] = [];
  medicines: MedicineLookupDto[] = [];

  // UI-only calculated totals
  uiTotalAmount = 0;
  uiNetAmount = 0;

  public readonly list = inject(ListService);
  private readonly saleService = inject(SaleService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);

  ngOnInit(): void {
    const saleStreamCreator = query =>
      this.saleService.getList({
        ...query,
        maxResultCount: 100,
      });

    this.list.hookToQuery(saleStreamCreator).subscribe(response => {
      this.sales = response;
    });

    this.loadCustomers();
    this.loadMedicines();
  }

  // Shortcut to items form array
  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  // Load customers for dropdown
  loadCustomers(): void {
    this.saleService.getCustomerLookup().subscribe(response => {
      this.customers = response.items;
    });
  }

  // Load medicines for dropdown
  loadMedicines(): void {
    this.saleService.getMedicineLookup().subscribe(response => {
      this.medicines = response.items;
    });
  }

  // Build main form
  buildForm(): void {
    this.form = this.fb.group({
      saleNumber: [
        this.selectedSale.saleNumber || '',
        [Validators.required, Validators.maxLength(64)],
      ],
      customerId: [
        this.selectedSale.customerId || null,
      ],
      saleDate: [
        this.toDateInputValue(this.selectedSale.saleDate) || this.today(),
        [Validators.required],
      ],
      notes: [
        this.selectedSale.notes || '',
        [Validators.maxLength(256)],
      ],
      discountAmount: [
        this.selectedSale.discountAmount ?? 0,
        [Validators.required, Validators.min(0)],
      ],
      items: this.fb.array([]),
    });

    // Existing items for edit
    if (this.selectedSale.items?.length) {
      this.selectedSale.items.forEach(item => {
        this.items.push(
          this.createItemFormGroup({
            medicineId: item.medicineId,
            batchNumber: item.batchNumber,
            quantity: item.quantity,
            unitPrice: item.unitPrice,
          })
        );
      });
    } else {
      // New sale starts with one item row
      this.addItemRow();
    }

    this.form.valueChanges.subscribe(() => {
      this.recalculateUiTotals();
    });

    this.recalculateUiTotals();
  }

  // Create one item row
  createItemFormGroup(item?: {
    medicineId?: string;
    batchNumber?: string | null;
    quantity?: number;
    unitPrice?: number;
  }): FormGroup {
    return this.fb.group({
      medicineId: [item?.medicineId || null, [Validators.required]],
      batchNumber: [item?.batchNumber || ''],
      quantity: [item?.quantity ?? 1, [Validators.required, Validators.min(1)]],
      unitPrice: [item?.unitPrice ?? 0, [Validators.required, Validators.min(0)]],
      lineTotal: [{ value: 0, disabled: true }],
    });
  }

  // Add new item row
  addItemRow(): void {
    this.items.push(this.createItemFormGroup());
    this.recalculateUiTotals();
  }

  // Remove item row
  removeItemRow(index: number): void {
    if (this.items.length === 1) {
      return;
    }

    this.items.removeAt(index);
    this.recalculateUiTotals();
  }

  // Open create modal
  createSale(): void {
    this.selectedSale = {} as SaleDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  // Open edit modal
  editSale(id: string): void {
    this.saleService.get(id).subscribe(sale => {
      this.selectedSale = sale;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  // Save sale
  save(): void {
    if (this.form.invalid) return;

    const rawValue = this.form.getRawValue();

    const input: CreateUpdateSaleDto = {
      saleNumber: rawValue.saleNumber,
      customerId: rawValue.customerId || undefined,
      saleDate: new Date(rawValue.saleDate).toISOString(),
      notes: rawValue.notes,
      discountAmount: rawValue.discountAmount,
      items: rawValue.items.map((x: any): CreateUpdateSaleItemDto => ({
        medicineId: x.medicineId,
        batchNumber: x.batchNumber,
        quantity: x.quantity,
        unitPrice: x.unitPrice,
      })),
    };

    const request = this.selectedSale.id
      ? this.saleService.update(this.selectedSale.id, input)
      : this.saleService.create(input);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.list.get();
    });
  }

  // Delete sale
  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.saleService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  // UI total calculations only
  recalculateUiTotals(): void {
    let totalAmount = 0;

    this.items.controls.forEach(control => {
      const quantity = Number(control.get('quantity')?.value || 0);
      const unitPrice = Number(control.get('unitPrice')?.value || 0);
      const lineTotal = quantity * unitPrice;

      control.get('lineTotal')?.setValue(lineTotal, { emitEvent: false });

      totalAmount += lineTotal;
    });

    const discount = Number(this.form?.get('discountAmount')?.value || 0);
    const netAmount = Math.max(totalAmount - discount, 0);

    this.uiTotalAmount = totalAmount;
    this.uiNetAmount = netAmount;
  }

  // Convert ISO date string to yyyy-MM-dd
  private toDateInputValue(value?: string): string | null {
    if (!value) return null;
    return value.substring(0, 10);
  }

  // Today as yyyy-MM-dd
  private today(): string {
    return new Date().toISOString().substring(0, 10);
  }
}
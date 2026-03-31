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
  CreateUpdatePurchaseDto,
  CreateUpdatePurchaseItemDto,
  MedicineLookupDto,
  PurchaseDto,
  PurchaseService,
  SupplierLookupDto,
} from '../proxy/purchases';

@Component({
  selector: 'app-purchase',
  standalone: true,
  templateUrl: './purchase.component.html',
  imports: [CommonModule, ReactiveFormsModule, ThemeSharedModule],
  providers: [ListService],
})
export class PurchaseComponent implements OnInit {
  // List page data
  purchases = { items: [], totalCount: 0 } as PagedResultDto<PurchaseDto>;

  // Current selected purchase for edit
  selectedPurchase = {} as PurchaseDto;

  // Main reactive form
  form!: FormGroup;

  // Modal visibility
  isModalOpen = false;

  // Dropdown lookup data
  suppliers: SupplierLookupDto[] = [];
  medicines: MedicineLookupDto[] = [];

  public readonly list = inject(ListService);
  private readonly purchaseService = inject(PurchaseService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);

  ngOnInit(): void {
    // Load purchase list with a larger page size for now
    const purchaseStreamCreator = query =>
      this.purchaseService.getList({
        ...query,
        maxResultCount: 100,
      });

    this.list.hookToQuery(purchaseStreamCreator).subscribe(response => {
      this.purchases = response;
    });

    // Load dropdown data
    this.loadSuppliers();
    this.loadMedicines();
  }

  // Convenience getter for item rows
  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  // Load suppliers for dropdown
  loadSuppliers(): void {
    this.purchaseService.getSupplierLookup().subscribe(response => {
      this.suppliers = response.items;
    });
  }

  // Load medicines for dropdown
  loadMedicines(): void {
    this.purchaseService.getMedicineLookup().subscribe(response => {
      this.medicines = response.items;
    });
  }

  // Build the main form
  buildForm(): void {
    this.form = this.fb.group({
      purchaseNumber: [
        this.selectedPurchase.purchaseNumber || '',
        [Validators.required, Validators.maxLength(64)],
      ],
      supplierId: [
        this.selectedPurchase.supplierId || null,
        [Validators.required],
      ],
      purchaseDate: [
        this.toDateInputValue(this.selectedPurchase.purchaseDate) || this.today(),
        [Validators.required],
      ],
      invoiceNumber: [
        this.selectedPurchase.invoiceNumber || '',
        [Validators.maxLength(128)],
      ],
      notes: [
        this.selectedPurchase.notes || '',
        [Validators.maxLength(256)],
      ],
      discountAmount: [
        this.selectedPurchase.discountAmount ?? 0,
        [Validators.required, Validators.min(0)],
      ],
      concurrencyStamp: [this.selectedPurchase.concurrencyStamp || null],
      items: this.fb.array([]),
    });

    // If editing existing purchase, populate rows from existing items
    if (this.selectedPurchase.items?.length) {
      this.selectedPurchase.items.forEach(item => {
        this.items.push(
          this.createItemFormGroup({
            medicineId: item.medicineId,
            batchNumber: item.batchNumber,
            expiryDate: this.toDateInputValue(item.expiryDate),
            quantity: item.quantity,
            unitPrice: item.unitPrice,
          })
        );
      });
    } else {
      // For new purchase, start with one empty row
      this.addItemRow();
    }

    // Recalculate totals whenever form changes
    this.form.valueChanges.subscribe(() => {
      this.recalculateUiTotals();
    });
  }

  // Create one purchase item row
  createItemFormGroup(item?: {
    medicineId?: string;
    batchNumber?: string | null;
    expiryDate?: string | null;
    quantity?: number;
    unitPrice?: number;
  }): FormGroup {
    return this.fb.group({
      medicineId: [item?.medicineId || null, [Validators.required]],
      batchNumber: [item?.batchNumber || ''],
      expiryDate: [item?.expiryDate || null],
      quantity: [item?.quantity ?? 1, [Validators.required, Validators.min(1)]],
      unitPrice: [item?.unitPrice ?? 0, [Validators.required, Validators.min(0)]],
      lineTotal: [{ value: 0, disabled: true }],
    });
  }

  // Add new empty item row
  addItemRow(): void {
    this.items.push(this.createItemFormGroup());
    this.recalculateUiTotals();
  }

  // Remove one item row
  removeItemRow(index: number): void {
    if (this.items.length === 1) {
      return;
    }

    this.items.removeAt(index);
    this.recalculateUiTotals();
  }

  // Open create modal
  createPurchase(): void {
    this.selectedPurchase = {} as PurchaseDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  // Open edit modal
  editPurchase(id: string): void {
    this.purchaseService.get(id).subscribe(purchase => {
      this.selectedPurchase = purchase;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  // Save purchase
  save(): void {
    if (this.form.invalid) return;

    const rawValue = this.form.getRawValue();

    const input: CreateUpdatePurchaseDto = {
      purchaseNumber: rawValue.purchaseNumber,
      supplierId: rawValue.supplierId,
      purchaseDate: new Date(rawValue.purchaseDate).toISOString(),
      invoiceNumber: rawValue.invoiceNumber,
      notes: rawValue.notes,
      discountAmount: rawValue.discountAmount,
      concurrencyStamp: rawValue.concurrencyStamp,
      items: rawValue.items.map((x: any): CreateUpdatePurchaseItemDto => ({
        medicineId: x.medicineId,
        batchNumber: x.batchNumber,
        expiryDate: x.expiryDate ? new Date(x.expiryDate).toISOString() : undefined,
        quantity: x.quantity,
        unitPrice: x.unitPrice,
      })),
    };

    const request = this.selectedPurchase.id
      ? this.purchaseService.update(this.selectedPurchase.id, input)
      : this.purchaseService.create(input);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.list.get();
    });

    console.log('selectedPurchase', this.selectedPurchase);
    console.log('rawValue', rawValue);
    console.log('concurrencyStamp being sent', rawValue.concurrencyStamp);
  }

  // Delete purchase
  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.purchaseService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  // Calculate line totals and overall totals for UI only
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

  // UI-only totals shown in modal
  uiTotalAmount = 0;
  uiNetAmount = 0;

  // Convert ISO date string to input[type=date] format
  private toDateInputValue(value?: string): string | null {
    if (!value) return null;
    return value.substring(0, 10);
  }

  // Returns today's date in yyyy-MM-dd format
  private today(): string {
    return new Date().toISOString().substring(0, 10);
  }
}
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ThemeSharedModule } from '@abp/ng.theme.shared';

import {
  CategoryLookupDto,
  CreateUpdateMedicineDto,
  MedicineDto,
  MedicineService,
} from '../proxy/medicines';

@Component({
  selector: 'app-medicine',
  standalone: true,
  templateUrl: './medicine.component.html',
  imports: [CommonModule, ReactiveFormsModule, ThemeSharedModule],
  providers: [ListService],
})
export class MedicineComponent implements OnInit {
  medicines = { items: [], totalCount: 0 } as PagedResultDto<MedicineDto>;
  categories: CategoryLookupDto[] = [];
  selectedMedicine = {} as MedicineDto;

  form!: FormGroup;
  isModalOpen = false;

  public readonly list = inject(ListService);
  private readonly medicineService = inject(MedicineService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);

  ngOnInit(): void {
    const medicineStreamCreator = query => this.medicineService.getList(query);

    this.list.hookToQuery(medicineStreamCreator).subscribe(response => {
      this.medicines = response;
    });

    this.loadCategories();
  }

  loadCategories(): void {
    this.medicineService.getCategoryLookup().subscribe(response => {
      this.categories = response.items;
    });
  }

  buildForm(): void {
    this.form = this.fb.group({
      name: [this.selectedMedicine.name || '', [Validators.required, Validators.maxLength(128)]],
      genericName: [this.selectedMedicine.genericName || '', [Validators.maxLength(128)]],
      categoryId: [this.selectedMedicine.categoryId || null, [Validators.required]],
      unit: [this.selectedMedicine.unit || '', [Validators.maxLength(64)]],
      barcode: [this.selectedMedicine.barcode || '', [Validators.maxLength(64)]],
      purchasePrice: [this.selectedMedicine.purchasePrice ?? 0, [Validators.required, Validators.min(0)]],
      salePrice: [this.selectedMedicine.salePrice ?? 0, [Validators.required, Validators.min(0)]],
      reorderLevel: [this.selectedMedicine.reorderLevel ?? 0, [Validators.required, Validators.min(0)]],
      isActive: [this.selectedMedicine.isActive ?? true],
    });
  }

  createMedicine(): void {
    this.selectedMedicine = {} as MedicineDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  editMedicine(id: string): void {
    this.medicineService.get(id).subscribe(medicine => {
      this.selectedMedicine = medicine;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const input = this.form.value as CreateUpdateMedicineDto;

    const request = this.selectedMedicine.id
      ? this.medicineService.update(this.selectedMedicine.id, input)
      : this.medicineService.create(input);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.list.get();
    });
  }

  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.medicineService.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
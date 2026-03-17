import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation, ThemeSharedModule } from '@abp/ng.theme.shared';

import {
  CategoryService,
  CategoryDto,
  CreateUpdateCategoryDto
} from '../proxy/categories';

@Component({
  selector: 'app-category',
  standalone: true,
  templateUrl: './category.component.html',
  imports: [CommonModule, ReactiveFormsModule, ThemeSharedModule],
  providers: [ListService],
})
export class CategoryComponent implements OnInit {
  category = { items: [], totalCount: 0 } as PagedResultDto<CategoryDto>;
  selectedCategory = {} as CategoryDto;

  form!: FormGroup;
  isModalOpen = false;

  public readonly list = inject(ListService);
  private readonly categoryService = inject(CategoryService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);

  ngOnInit(): void {
    const categoryStreamCreator = query => this.categoryService.getList(query);

    this.list.hookToQuery(query =>
      this.categoryService.getList({
        ...query,
        maxResultCount: 100 // increase limit
      })
    ).subscribe(response => {
      this.category = response;
    });
  }

  buildForm(): void {
    this.form = this.fb.group({
      name: [this.selectedCategory.name || '', Validators.required],
      description: [this.selectedCategory.description || ''],
      isActive: [this.selectedCategory.isActive ?? true],
    });
  }

  createCategory(): void {
    this.selectedCategory = {} as CategoryDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  editCategory(id: string): void {
    this.categoryService.get(id).subscribe(category => {
      this.selectedCategory = category;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const input = this.form.value as CreateUpdateCategoryDto;

    const request = this.selectedCategory.id
      ? this.categoryService.update(this.selectedCategory.id, input)
      : this.categoryService.create(input);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.list.get();
    });
  }

  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.categoryService.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ThemeSharedModule, ToasterService } from '@abp/ng.theme.shared';
import { PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { jsPDF } from 'jspdf';

import {
    SaleService,
    CreateUpdateSaleDto,
    CreateUpdateSaleItemDto,
    CustomerLookupDto,
} from '../proxy/sales';

import { MedicineService, MedicineDto } from '../proxy/medicines';
import { StockService, StockDto } from '../proxy/stocks';

interface PosCartItem {
    medicineId: string;
    medicineName: string;
    barcode?: string | null;
    batchNumber?: string | null;
    expiryDate?: string | null;
    availableQuantity: number;
    quantity: number;
    unitPrice: number;
    lineTotal: number;
}

interface PosReceiptItem {
    medicineName: string;
    batchNumber?: string | null;
    expiryDate?: string | null;
    quantity: number;
    unitPrice: number;
    lineTotal: number;
}

interface PosReceipt {
    saleId?: string;
    saleNumber: string;
    saleDate: string;
    customerName: string;
    items: PosReceiptItem[];
    subtotal: number;
    discountAmount: number;
    netTotal: number;
    createdAt: string;
}

@Component({
    selector: 'app-pos',
    standalone: true,
    imports: [CommonModule, FormsModule, ThemeSharedModule],
    templateUrl: './pos.component.html',
    styleUrl: './pos.component.scss',
})
export class PosComponent implements OnInit {
    private readonly saleService = inject(SaleService);
    private readonly medicineService = inject(MedicineService);
    private readonly stockService = inject(StockService);
    private readonly toaster = inject(ToasterService);
    private readonly router = inject(Router);

    medicines: MedicineDto[] = [];
    stocks: StockDto[] = [];
    customers: CustomerLookupDto[] = [];

    searchText = '';
    selectedCustomerId: string | null = null;
    saleDate = this.today();
    notes = '';
    discountAmount = 0;

    cart: PosCartItem[] = [];

    lastReceipt: PosReceipt | null = null;
    showReceiptModal = false;
    saleNumber = this.generateSaleNumber();

    isSaving = false;
    isLoading = false;

    ngOnInit(): void {
        this.loadData();
    }

    get filteredMedicines(): MedicineDto[] {
        const search = this.searchText.trim().toLowerCase();

        if (!search) {
            return this.medicines.slice(0, 20);
        }

        return this.medicines
            .filter(medicine => {
                const name = medicine.name?.toLowerCase() || '';
                const genericName = medicine.genericName?.toLowerCase() || '';
                const barcode = medicine.barcode?.toLowerCase() || '';

                return (
                    name.includes(search) ||
                    genericName.includes(search) ||
                    barcode.includes(search)
                );
            })
            .slice(0, 30);
    }

    get subtotal(): number {
        return this.cart.reduce((sum, item) => sum + item.lineTotal, 0);
    }

    get netTotal(): number {
        return Math.max(this.subtotal - Number(this.discountAmount || 0), 0);
    }

    get totalItems(): number {
        return this.cart.reduce((sum, item) => sum + Number(item.quantity || 0), 0);
    }

    loadData(): void {
        this.isLoading = true;

        const input: PagedAndSortedResultRequestDto = {
            skipCount: 0,
            maxResultCount: 1000,
            sorting: 'name',
        };

        this.medicineService.getList(input).subscribe({
            next: response => {
                this.medicines = response.items || [];
            },
            error: () => {
                this.toaster.error('Failed to load medicines');
            },
        });

        this.stockService
            .getList({
                skipCount: 0,
                maxResultCount: 1000,
                sorting: 'medicineName',
            })
            .subscribe({
                next: response => {
                    this.stocks = response.items || [];
                    this.isLoading = false;
                },
                error: () => {
                    this.isLoading = false;
                    this.toaster.error('Failed to load stock');
                },
            });

        this.saleService.getCustomerLookup().subscribe({
            next: response => {
                this.customers = response.items || [];
            },
            error: () => {
                this.toaster.error('Failed to load customers');
            },
        });
    }

    addMedicine(medicine: MedicineDto): void {
        if (!medicine.id) {
            return;
        }

        const availableBatches = this.getAvailableBatches(medicine.id);

        if (!availableBatches.length) {
            this.toaster.warn(`${medicine.name} is not available in stock`);
            return;
        }

        const selectedBatch = this.getBestBatch(availableBatches);

        if (!selectedBatch) {
            this.toaster.warn(`${medicine.name} has no valid batch available`);
            return;
        }

        const existingItem = this.cart.find(
            item =>
                item.medicineId === medicine.id &&
                item.batchNumber === selectedBatch.batchNumber
        );

        if (existingItem) {
            if (existingItem.quantity + 1 > existingItem.availableQuantity) {
                this.toaster.warn('Quantity cannot exceed available stock');
                return;
            }

            existingItem.quantity += 1;
            this.recalculateItem(existingItem);
            this.searchText = '';
            return;
        }

        const cartItem: PosCartItem = {
            medicineId: medicine.id,
            medicineName: medicine.name || '',
            barcode: medicine.barcode,
            batchNumber: selectedBatch.batchNumber,
            expiryDate: selectedBatch.expiryDate,
            availableQuantity: Number(selectedBatch.quantity || 0),
            quantity: 1,
            unitPrice: Number(medicine.salePrice || 0),
            lineTotal: Number(medicine.salePrice || 0),
        };

        this.cart.push(cartItem);
        this.searchText = '';
    }

    updateBatch(item: PosCartItem, batchNumber: string): void {
        const stock = this.stocks.find(
            x => x.medicineId === item.medicineId && x.batchNumber === batchNumber
        );

        if (!stock) {
            return;
        }

        item.batchNumber = stock.batchNumber;
        item.expiryDate = stock.expiryDate;
        item.availableQuantity = Number(stock.quantity || 0);

        if (item.quantity > item.availableQuantity) {
            item.quantity = item.availableQuantity;
        }

        this.recalculateItem(item);
    }

    updateQuantity(item: PosCartItem): void {
        item.quantity = Number(item.quantity || 0);

        if (item.quantity < 1) {
            item.quantity = 1;
        }

        if (item.quantity > item.availableQuantity) {
            item.quantity = item.availableQuantity;
            this.toaster.warn('Quantity adjusted to available stock');
        }

        this.recalculateItem(item);
    }

    updatePrice(item: PosCartItem): void {
        item.unitPrice = Number(item.unitPrice || 0);

        if (item.unitPrice < 0) {
            item.unitPrice = 0;
        }

        this.recalculateItem(item);
    }

    removeItem(index: number): void {
        this.cart.splice(index, 1);
    }

    clearCart(): void {
        this.cart = [];
        this.discountAmount = 0;
        this.notes = '';
        this.selectedCustomerId = null;
        this.saleDate = this.today();
        this.searchText = '';
        this.saleNumber = this.generateSaleNumber();
    }

    completeSale(): void {
        if (!this.cart.length) {
            this.toaster.warn('Please add at least one medicine to the cart');
            return;
        }

        const invalidItem = this.cart.find(
            item =>
                !item.medicineId ||
                !item.batchNumber ||
                !item.quantity ||
                item.quantity <= 0 ||
                item.quantity > item.availableQuantity ||
                item.unitPrice < 0
        );

        if (invalidItem) {
            this.toaster.warn('Please check cart item batch, quantity, and price');
            return;
        }

        if (this.discountInvalid) {
            this.toaster.warn('Discount cannot be negative or greater than subtotal');
            return;
        }

        const items: CreateUpdateSaleItemDto[] = this.cart.map(item => ({
            medicineId: item.medicineId,
            batchNumber: item.batchNumber || undefined,
            quantity: item.quantity,
            unitPrice: item.unitPrice,
        }));

        const input: CreateUpdateSaleDto = {
            saleNumber: this.saleNumberPreview,
            customerId: this.selectedCustomerId || undefined,
            saleDate: new Date(this.saleDate).toISOString(),
            notes: this.notes || undefined,
            discountAmount: Number(this.discountAmount || 0),
            items,
        };

        this.isSaving = true;

        this.saleService.create(input).subscribe({
            next: sale => {
                this.isSaving = false;

                this.lastReceipt = this.buildReceipt(sale?.id);
                this.showReceiptModal = true;

                this.toaster.success('Sale completed successfully');

                this.cart = [];
                this.discountAmount = 0;
                this.notes = '';
                this.selectedCustomerId = null;
                this.saleDate = this.today();
                this.searchText = '';
                this.saleNumber = this.generateSaleNumber();

                this.loadData();

                if (sale?.id) {
                    // Later we can route to invoice print page here.
                    // this.router.navigate(['/sales', sale.id]);
                }
            },
            error: error => {
                this.isSaving = false;
                this.toaster.error(
                    error?.error?.error?.message || 'Failed to complete sale'
                );
            },
        });
    }

    getAvailableBatches(medicineId: string): StockDto[] {
        return this.stocks
            .filter(stock => {
                const quantity = Number(stock.quantity || 0);
                return stock.medicineId === medicineId && quantity > 0 && !this.isExpired(stock.expiryDate);
            })
            .sort((a, b) => {
                const aDate = a.expiryDate ? new Date(a.expiryDate).getTime() : Number.MAX_SAFE_INTEGER;
                const bDate = b.expiryDate ? new Date(b.expiryDate).getTime() : Number.MAX_SAFE_INTEGER;
                return aDate - bDate;
            });
    }

    getTotalStock(medicineId?: string): number {
        if (!medicineId) {
            return 0;
        }

        return this.getAvailableBatches(medicineId).reduce(
            (sum, stock) => sum + Number(stock.quantity || 0),
            0
        );
    }

    isLowStock(medicine: MedicineDto): boolean {
        if (!medicine.id) {
            return false;
        }

        const totalStock = this.getTotalStock(medicine.id);
        const reorderLevel = Number(medicine.reorderLevel || 0);

        return reorderLevel > 0 && totalStock <= reorderLevel;
    }

    isExpired(expiryDate?: string | null): boolean {
        if (!expiryDate) {
            return false;
        }

        const today = new Date();
        today.setHours(0, 0, 0, 0);

        const expiry = new Date(expiryDate);
        expiry.setHours(0, 0, 0, 0);

        return expiry < today;
    }

    isExpiringSoon(expiryDate?: string | null): boolean {
        if (!expiryDate) {
            return false;
        }

        const today = new Date();
        const expiry = new Date(expiryDate);
        const diffTime = expiry.getTime() - today.getTime();
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        return diffDays >= 0 && diffDays <= 30;
    }

    formatDate(value?: string | null): string {
        if (!value) {
            return '-';
        }

        return value.substring(0, 10);
    }

    private getBestBatch(batches: StockDto[]): StockDto | undefined {
        return batches[0];
    }

    private recalculateItem(item: PosCartItem): void {
        item.lineTotal = Number(item.quantity || 0) * Number(item.unitPrice || 0);
    }

    private generateSaleNumber(): string {
        const now = new Date();

        const year = now.getFullYear();
        const month = `${now.getMonth() + 1}`.padStart(2, '0');
        const day = `${now.getDate()}`.padStart(2, '0');
        const hour = `${now.getHours()}`.padStart(2, '0');
        const minute = `${now.getMinutes()}`.padStart(2, '0');
        const second = `${now.getSeconds()}`.padStart(2, '0');

        return `SAL-${year}${month}${day}-${hour}${minute}${second}`;
    }

    private today(): string {
        return new Date().toISOString().substring(0, 10);
    }

    get saleNumberPreview(): string {
        return this.saleNumber;
    }

    get isSaleInvalid(): boolean {
        if (!this.cart.length || this.isSaving) {
            return true;
        }

        if (Number(this.discountAmount || 0) < 0) {
            return true;
        }

        if (Number(this.discountAmount || 0) > this.subtotal) {
            return true;
        }

        return this.cart.some(
            item =>
                !item.medicineId ||
                !item.batchNumber ||
                !item.quantity ||
                item.quantity <= 0 ||
                item.quantity > item.availableQuantity ||
                item.unitPrice < 0
        );
    }

    get discountInvalid(): boolean {
        return Number(this.discountAmount || 0) < 0 || Number(this.discountAmount || 0) > this.subtotal;
    }

    increaseQuantity(item: PosCartItem): void {
        if (item.quantity >= item.availableQuantity) {
            this.toaster.warn('Quantity cannot exceed available stock');
            return;
        }

        item.quantity += 1;
        this.recalculateItem(item);
    }

    decreaseQuantity(item: PosCartItem): void {
        if (item.quantity <= 1) {
            return;
        }

        item.quantity -= 1;
        this.recalculateItem(item);
    }

    getBatchLabel(stock: StockDto): string {
        const batch = stock.batchNumber || 'No Batch';
        const expiry = stock.expiryDate ? this.formatDate(stock.expiryDate) : 'No Expiry';
        const qty = Number(stock.quantity || 0);

        return `${batch} | Exp: ${expiry} | Qty: ${qty}`;
    }

    getStockStatusClass(medicine: MedicineDto): string {
        const stock = this.getTotalStock(medicine.id);

        if (stock <= 0) {
            return 'stock-out';
        }

        if (this.isLowStock(medicine)) {
            return 'stock-low';
        }

        return 'stock-available';
    }

    getStockStatusText(medicine: MedicineDto): string {
        const stock = this.getTotalStock(medicine.id);

        if (stock <= 0) {
            return 'Out of stock';
        }

        if (this.isLowStock(medicine)) {
            return `Low stock: ${stock}`;
        }

        return `Available: ${stock}`;
    }

    getSelectedCustomerName(): string {
        if (!this.selectedCustomerId) {
            return 'Walk-in Customer';
        }

        const customer = this.customers.find(x => x.id === this.selectedCustomerId);

        return customer?.name || 'Walk-in Customer';
    }

    private buildReceipt(saleId?: string): PosReceipt {
        return {
            saleId,
            saleNumber: this.saleNumber,
            saleDate: this.saleDate,
            customerName: this.getSelectedCustomerName(),
            items: this.cart.map(item => ({
                medicineName: item.medicineName,
                batchNumber: item.batchNumber,
                expiryDate: item.expiryDate,
                quantity: item.quantity,
                unitPrice: item.unitPrice,
                lineTotal: item.lineTotal,
            })),
            subtotal: this.subtotal,
            discountAmount: Number(this.discountAmount || 0),
            netTotal: this.netTotal,
            createdAt: new Date().toISOString(),
        };
    }

    printReceipt(): void {
        if (!this.lastReceipt) {
            this.toaster.warn('No receipt available to print');
            return;
        }

        const printWindow = window.open('', '_blank', 'width=420,height=720');

        if (!printWindow) {
            this.toaster.error('Unable to open receipt window. Please allow popups for this site.');
            return;
        }

        printWindow.document.open();
        printWindow.document.write(this.buildReceiptPrintHtml());
        printWindow.document.close();
    }

    downloadReceiptPdf(): void {
        if (!this.lastReceipt) {
            this.toaster.warn('No receipt available to download');
            return;
        }

        const receipt = this.lastReceipt;
        const pageWidth = 80;
        const margin = 4;
        const contentWidth = pageWidth - margin * 2;
        const itemNameWidth = 34;
        const rowGap = 2;

        const measureDoc = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: [pageWidth, 300],
        });

        measureDoc.setFont('helvetica', 'normal');
        measureDoc.setFontSize(8.5);

        const itemLayouts = receipt.items.map(item => {
            const nameLines = measureDoc.splitTextToSize(item.medicineName || '-', itemNameWidth) as string[];
            const batchLine = `Batch: ${item.batchNumber || '-'}`;
            const expiryLine = `Exp: ${this.formatDate(item.expiryDate)}`;
            const rowHeight = Math.max(13, nameLines.length * 3.5 + 8.5);

            return {
                item,
                nameLines,
                batchLine,
                expiryLine,
                rowHeight,
            };
        });

        const itemsHeight = itemLayouts.reduce((sum, layout) => sum + layout.rowHeight + rowGap, 0);
        const pageHeight = Math.max(120, 78 + itemsHeight);

        const doc = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: [pageWidth, pageHeight],
        });

        let y = 6;

        const drawDashedLine = (lineY: number): void => {
            doc.setDrawColor(0);
            doc.setLineWidth(0.15);
            doc.setLineDashPattern([1.2, 1.2], 0);
            doc.line(margin, lineY, pageWidth - margin, lineY);
            doc.setLineDashPattern([], 0);
        };

        const drawInfoRow = (label: string, value: string): void => {
            doc.setFont('helvetica', 'normal');
            doc.setFontSize(8.5);
            doc.text(label, margin, y);

            doc.setFont('helvetica', 'bold');
            const valueLines = doc.splitTextToSize(value || '-', 45) as string[];
            doc.text(valueLines, pageWidth - margin, y, { align: 'right' });
            y += Math.max(4, valueLines.length * 3.5);
        };

        const drawMoneyRow = (label: string, value: number, isGrandTotal = false): void => {
            doc.setFont('helvetica', isGrandTotal ? 'bold' : 'normal');
            doc.setFontSize(isGrandTotal ? 10.5 : 8.5);
            doc.text(label, margin, y);
            doc.text(this.formatMoney(value), pageWidth - margin, y, { align: 'right' });
            y += isGrandTotal ? 5 : 4;
        };

        doc.setTextColor(0);
        doc.setFillColor(255, 255, 255);
        doc.rect(0, 0, pageWidth, pageHeight, 'F');

        doc.setFont('helvetica', 'bold');
        doc.setFontSize(14);
        doc.text('PharmacySystem', pageWidth / 2, y, { align: 'center' });
        y += 5;

        doc.setFont('helvetica', 'normal');
        doc.setFontSize(8.5);
        doc.text('Pharmacy Management System', pageWidth / 2, y, { align: 'center' });
        y += 4;
        doc.text('Sales Receipt', pageWidth / 2, y, { align: 'center' });
        y += 5;

        drawDashedLine(y);
        y += 4;

        drawInfoRow('Sale No:', receipt.saleNumber);
        drawInfoRow('Date:', this.formatReceiptDate(receipt.saleDate));
        drawInfoRow('Time:', this.formatReceiptTime(receipt.createdAt));
        drawInfoRow('Customer:', receipt.customerName);

        y += 1;
        drawDashedLine(y);
        y += 4;

        doc.setFont('helvetica', 'bold');
        doc.setFontSize(7.5);
        doc.text('Item', margin, y);
        doc.text('Qty', 49, y, { align: 'right' });
        doc.text('Rate', 61, y, { align: 'right' });
        doc.text('Amt', pageWidth - margin, y, { align: 'right' });
        y += 2;
        doc.setLineWidth(0.15);
        doc.line(margin, y, pageWidth - margin, y);
        y += 3;

        itemLayouts.forEach(layout => {
            const rowTop = y;

            doc.setFont('helvetica', 'bold');
            doc.setFontSize(8.2);
            doc.text(layout.nameLines, margin, y);

            doc.setFont('helvetica', 'normal');
            doc.setFontSize(8);
            doc.text(String(layout.item.quantity), 49, rowTop, { align: 'right' });
            doc.text(this.formatMoney(layout.item.unitPrice), 61, rowTop, { align: 'right' });
            doc.text(this.formatMoney(layout.item.lineTotal), pageWidth - margin, rowTop, { align: 'right' });

            y += layout.nameLines.length * 3.5 + 1;
            doc.setFontSize(7);
            doc.text(layout.batchLine, margin, y);
            y += 3.2;
            doc.text(layout.expiryLine, margin, y);
            y = rowTop + layout.rowHeight;

            doc.setDrawColor(160);
            doc.setLineWidth(0.1);
            doc.line(margin, y, pageWidth - margin, y);
            y += rowGap;
        });

        y += 1;
        drawDashedLine(y);
        y += 5;

        drawMoneyRow('Subtotal', receipt.subtotal);
        drawMoneyRow('Discount', receipt.discountAmount);

        doc.setLineWidth(0.15);
        doc.line(margin, y - 1, pageWidth - margin, y - 1);
        y += 3;
        drawMoneyRow('NET TOTAL', receipt.netTotal, true);

        y += 1;
        drawDashedLine(y);
        y += 5;

        doc.setFont('helvetica', 'bold');
        doc.setFontSize(8.5);
        doc.text('Thank you for your purchase.', pageWidth / 2, y, { align: 'center' });
        y += 4;
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(7);
        doc.text('This is a computer-generated receipt.', pageWidth / 2, y, { align: 'center' });

        doc.save(`receipt-${this.sanitizeFileName(receipt.saleNumber)}.pdf`);
    }

    private buildReceiptPrintHtml(): string {
        if (!this.lastReceipt) {
            return '';
        }

        const receipt = this.lastReceipt;

        const itemRows = receipt.items
            .map(
                item => `
        <tr>
          <td class="item-name">
            <strong>${this.escapeHtml(item.medicineName)}</strong>
            <small>
              Batch: ${this.escapeHtml(item.batchNumber || '-')}<br />
              Exp: ${this.escapeHtml(this.formatDate(item.expiryDate))}
            </small>
          </td>
          <td class="text-end">${item.quantity}</td>
          <td class="text-end">${this.formatMoney(item.unitPrice)}</td>
          <td class="text-end">${this.formatMoney(item.lineTotal)}</td>
        </tr>
      `
            )
            .join('');

        return `
<!doctype html>
<html>
<head>
  <meta charset="utf-8" />
  <title>Receipt ${this.escapeHtml(receipt.saleNumber)}</title>

  <style>
    @page {
      size: 80mm auto;
      margin: 0;
    }

    * {
      box-sizing: border-box;
    }

    html,
    body {
      margin: 0;
      padding: 0;
      background: #ffffff;
      color: #000000;
      font-family: Arial, Helvetica, sans-serif;
      font-size: 11px;
      line-height: 1.35;
    }

    body {
      width: 80mm;
      margin: 0 auto;
    }

    .receipt {
      width: 80mm;
      padding: 4mm;
      background: #ffffff;
    }

    .print-actions {
      width: 80mm;
      margin: 12px auto;
      display: flex;
      gap: 8px;
      justify-content: center;
    }

    .print-actions button {
      border: 0;
      border-radius: 6px;
      padding: 8px 12px;
      cursor: pointer;
      font-weight: 700;
    }

    .print-actions button:first-child {
      background: #057a68;
      color: #ffffff;
    }

    .print-actions button:last-child {
      background: #e5e7eb;
      color: #111827;
    }

    .header {
      text-align: center;
      margin-bottom: 3mm;
    }

    .header h1 {
      margin: 0;
      font-size: 18px;
      font-weight: 900;
      line-height: 1.15;
    }

    .header p {
      margin: 1mm 0 0;
      font-size: 11px;
    }

    .muted {
      font-size: 10px;
    }

    .line {
      border-top: 1px dashed #000000;
      margin: 3mm 0;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      gap: 3mm;
      margin-bottom: 1mm;
      font-size: 10.5px;
    }

    .info-row span {
      font-weight: 400;
    }

    .info-row strong {
      font-weight: 700;
      text-align: right;
    }

    table {
      width: 100%;
      border-collapse: collapse;
    }

    th {
      padding: 1.5mm 0;
      border-bottom: 1px solid #000000;
      font-size: 9px;
      text-transform: uppercase;
      text-align: left;
    }

    td {
      padding: 1.8mm 0;
      border-bottom: 1px dotted #999999;
      vertical-align: top;
      font-size: 10px;
    }

    .item-name {
      width: 42%;
      padding-right: 2mm;
    }

    .item-name strong {
      display: block;
      font-size: 10.5px;
    }

    .item-name small {
      display: block;
      margin-top: 0.7mm;
      font-size: 8.5px;
      line-height: 1.25;
    }

    .text-end {
      text-align: right;
    }

    .totals {
      margin-top: 2mm;
    }

    .total-row {
      display: flex;
      justify-content: space-between;
      padding: 0.8mm 0;
      font-size: 10.5px;
    }

    .grand-total {
      margin-top: 1.5mm;
      padding-top: 2mm;
      border-top: 1px solid #000000;
      font-size: 13px;
      font-weight: 900;
    }

    .footer {
      text-align: center;
      margin-top: 3mm;
      font-size: 10px;
    }

    .footer strong {
      display: block;
      margin-bottom: 1mm;
    }

    .footer small {
      font-size: 8.5px;
    }

    @media print {
      html,
      body {
        width: 80mm;
        margin: 0;
        padding: 0;
      }

      .print-actions {
        display: none !important;
      }

      .receipt {
        width: 80mm;
        padding: 4mm;
      }
    }
  </style>
</head>

<body>
  <div class="print-actions">
    <button type="button" onclick="window.print()">Print Receipt</button>
    <button type="button" onclick="window.close()">Close</button>
  </div>

  <div class="receipt">
    <div class="header">
      <h1>PharmacySystem</h1>
      <p>Pharmacy Management System</p>
      <p class="muted">Sales Receipt</p>
    </div>

    <div class="line"></div>

    <div class="info-row">
      <span>Sale No:</span>
      <strong>${this.escapeHtml(receipt.saleNumber)}</strong>
    </div>

    <div class="info-row">
      <span>Date:</span>
      <strong>${this.escapeHtml(this.formatReceiptDate(receipt.saleDate))}</strong>
    </div>

    <div class="info-row">
      <span>Time:</span>
      <strong>${this.escapeHtml(this.formatReceiptTime(receipt.createdAt))}</strong>
    </div>

    <div class="info-row">
      <span>Customer:</span>
      <strong>${this.escapeHtml(receipt.customerName)}</strong>
    </div>

    <div class="line"></div>

    <table>
      <thead>
        <tr>
          <th>Item</th>
          <th class="text-end">Qty</th>
          <th class="text-end">Rate</th>
          <th class="text-end">Amt</th>
        </tr>
      </thead>

      <tbody>
        ${itemRows}
      </tbody>
    </table>

    <div class="line"></div>

    <div class="totals">
      <div class="total-row">
        <span>Subtotal</span>
        <strong>${this.formatMoney(receipt.subtotal)}</strong>
      </div>

      <div class="total-row">
        <span>Discount</span>
        <strong>${this.formatMoney(receipt.discountAmount)}</strong>
      </div>

      <div class="total-row grand-total">
        <span>NET TOTAL</span>
        <strong>${this.formatMoney(receipt.netTotal)}</strong>
      </div>
    </div>

    <div class="line"></div>

    <div class="footer">
      <strong>Thank you for your purchase.</strong>
      <small>This is a computer-generated receipt.</small>
    </div>
  </div>


</body>
</html>
  `;
    }

    private escapeHtml(value: string): string {
        return String(value ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    private formatMoney(value: number): string {
        return Number(value || 0).toFixed(2);
    }

    private sanitizeFileName(value: string): string {
        return String(value || 'receipt').replace(/[\\/:*?"<>|]+/g, '-');
    }

    private formatReceiptDate(value?: string | null): string {
        if (!value) {
            return '-';
        }

        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return value.substring(0, 10);
        }

        const year = date.getFullYear();
        const month = `${date.getMonth() + 1}`.padStart(2, '0');
        const day = `${date.getDate()}`.padStart(2, '0');

        return `${year}-${month}-${day}`;
    }

    private formatReceiptTime(value?: string | null): string {
        if (!value) {
            return '-';
        }

        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return '-';
        }

        const hour = `${date.getHours()}`.padStart(2, '0');
        const minute = `${date.getMinutes()}`.padStart(2, '0');

        return `${hour}:${minute}`;
    }

    newSale(): void {
        this.showReceiptModal = false;
        this.lastReceipt = null;
        this.clearCart();
    }

    closeReceiptModal(): void {
        this.showReceiptModal = false;
    }




}

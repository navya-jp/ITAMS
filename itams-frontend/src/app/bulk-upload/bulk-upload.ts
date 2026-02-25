import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface BulkUploadResult {
  totalRows: number;
  successCount: number;
  failedCount: number;
  errors: BulkUploadError[];
  message: string;
}

interface BulkUploadError {
  rowNumber: number;
  assetTag: string;
  errorMessage: string;
}

@Component({
  selector: 'app-bulk-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bulk-upload.html',
  styleUrls: ['./bulk-upload.scss']
})
export class BulkUpload {
  selectedFile: File | null = null;
  uploading = false;
  uploadResult: BulkUploadResult | null = null;
  dragOver = false;
  private baseUrl = 'http://localhost:5066/api';

  constructor(private http: HttpClient) {}

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.uploadResult = null;
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;

    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      const file = event.dataTransfer.files[0];
      if (file.name.endsWith('.xlsx')) {
        this.selectedFile = file;
        this.uploadResult = null;
      } else {
        alert('Please select an Excel file (.xlsx)');
      }
    }
  }

  clearFile() {
    this.selectedFile = null;
    this.uploadResult = null;
  }

  async uploadFile() {
    if (!this.selectedFile) {
      alert('Please select a file first');
      return;
    }

    this.uploading = true;
    this.uploadResult = null;

    try {
      const formData = new FormData();
      formData.append('file', this.selectedFile);

      const token = localStorage.getItem('token');
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`
      });

      const result = await this.http.post<BulkUploadResult>(
        `${this.baseUrl}/assets/bulk-upload`, 
        formData,
        { 
          headers,
          withCredentials: true
        }
      ).toPromise();
      
      if (result) {
        this.uploadResult = result;
        
        if (result.successCount > 0) {
          alert(`Successfully uploaded ${result.successCount} assets!`);
        }
        
        if (result.failedCount > 0) {
          alert(`${result.failedCount} rows failed. Please check the error details below.`);
        }
      }
    } catch (error: any) {
      console.error('Upload error:', error);
      alert(error.error?.message || 'An error occurred during upload');
    } finally {
      this.uploading = false;
    }
  }

  downloadTemplate() {
    // Use the API service to get the base URL
    window.open(`${this.baseUrl}/assets/download-template`, '_blank');
  }

  getFileName(): string {
    return this.selectedFile?.name || '';
  }

  getFileSize(): string {
    if (!this.selectedFile) return '';
    const sizeInMB = (this.selectedFile.size / (1024 * 1024)).toFixed(2);
    return `${sizeInMB} MB`;
  }
}

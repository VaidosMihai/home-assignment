import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  books: any[] = [];
  selectedFile: File | null = null;
  selectedFileName: string = '';
  message: string = '';
  isError: boolean = false;
  isDragging: boolean = false;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadBooks();
  }

  loadBooks() {
    this.http.get<any[]>('/api/books').subscribe({
      next: (data) => this.books = data,
      error: (err) => console.error('Error fetching books:', err)
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    this.processFile(file);
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    if (event.dataTransfer && event.dataTransfer.files.length > 0) {
      const file = event.dataTransfer.files[0];
      this.processFile(file);
    }
  }

  processFile(file: File) {
    if (!file) return;

    if (!file.name.endsWith('.csv')) {
      this.message = 'Invalid file type. Please upload a .csv file.';
      this.isError = true;
      this.selectedFile = null;
      this.selectedFileName = '';
      return;
    }

    this.selectedFile = file;
    this.selectedFileName = file.name;
    this.message = '';
  }

  uploadCsv() {
    if (!this.selectedFile) {
      this.message = 'Please select a CSV file first!';
      this.isError = true;
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.http.post('/api/imports', formData).subscribe({
      next: (res: any) => {
        this.message = `Import successful! Added books count: ${res.imported}`;
        this.isError = false;
        this.selectedFile = null;
        this.selectedFileName = '';
        this.loadBooks();
      },
      error: (err) => {
        this.message = 'Error importing the CSV file.';
        this.isError = true;
        console.error(err);
      }
    });
  }
}
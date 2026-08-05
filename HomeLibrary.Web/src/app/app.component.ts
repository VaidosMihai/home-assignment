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
  message: string = '';
  isError: boolean = false;

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
    this.selectedFile = event.target.files[0];
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
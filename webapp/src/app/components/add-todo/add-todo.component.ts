import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormsModule, Validators } from '@angular/forms';
import { FileUploadEvent, FileUploadModule } from 'primeng/fileupload';
import { AddTodo, FileData } from '../../models/addTodo';
import { AsyncSubject, Observable } from 'rxjs';
import { TodoService } from '../../service/todo.service';
import { Router } from '@angular/router';
import { CalendarModule } from 'primeng/calendar';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../service/auth.service';
import { LoaderService } from '../../service/loader.service';
import { format } from 'date-fns/format';

@Component({
  selector: 'app-add-todo',
  standalone: true,
  imports: [FormsModule, FileUploadModule, CommonModule, CalendarModule],
  templateUrl: './add-todo.component.html',
  styleUrl: './add-todo.component.css'
})
export class AddTodoComponent {
  uploadedFiles: any[] = [];
  base64Files: FileData[] = [];
  title: string = '';
  notes: string = '';
  dueOn!: Date;
  file: FileData | null = null;
  minDate: string;
  minDate1: Date;
  minDateError = false;

  constructor(private todoService: TodoService, private router: Router,
    private toaster: ToastrService, private authService: AuthService, private loader: LoaderService) {
    this.minDate1 = new Date(new Date().getTime() + 5 * 60000);
    this.minDate = format(this.minDate1, "yyyy-MM-dd'T'HH:mm");
  }

  remove(event: any) {
    this.uploadedFiles = this.uploadedFiles.filter(t => event.file.name != t.name);
  }

  onSelect(event: any) {
    this.uploadedFiles = event.currentFiles;
  }

  addTodo() {
    var user = this.authService.getUser();
    if (user != null && user.Email && user.Otp) {
      this.loader.showLoader();
      this.toFilesBase64(this.uploadedFiles, this.base64Files).subscribe((res: FileData[]) => {
        var todo: AddTodo = {
          Title: this.title,
          Notes: this.notes,
          DueOn: this.dueOn,
          Email: user?.Email as string,
          Token: user?.Otp as number,
          File: res[0]
        }
        this.todoService.addTodo(todo).subscribe(
          {
            next: () => {
              this.loader.hideLoader();
              this.toaster.success("To-Do added");
              this.router.navigateByUrl('/todos');
            },
            error: (err) => {
              this.loader.hideLoader();
              console.error('Error occurred:', err);
            }
          });
      });
    } else {
      this.toaster.error("Unauthorized Action, Please sign again");
      this.router.navigateByUrl("/login");
    }
  }

  checkMinDate() {
    this.minDateError = false;
    if (new Date(this.dueOn) < this.minDate1) {
      this.minDateError = true;
    }
  }

  public toFilesBase64(files: File[], base64Files: FileData[]): Observable<FileData[]> {
    const result = new AsyncSubject<FileData[]>();
    if (files?.length) {
      Object.keys(files)?.forEach(async (file, i) => {
        const reader = new FileReader();
        reader.readAsDataURL(files[i]);
        reader.onload = (e) => {
          base64Files = base64Files?.filter(f => f?.FileName != files[i]?.name)
          base64Files.push({ FileName: files[i]?.name, FileSize: files[i].size, FileB64: (reader?.result as string).split(',')[1], FileType: files[i].type })
          result.next(base64Files);
          if (files?.length === (i + 1)) {
            result.complete();
          }
        };
      });
      return result;
    } else {
      result.next([]);
      result.complete();
      return result;
    }
  }
}

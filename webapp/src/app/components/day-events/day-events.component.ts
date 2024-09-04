import { Component, Input, OnChanges, SimpleChanges, input } from '@angular/core';
import { todo } from '../../models/events';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TodoService } from '../../service/todo.service';
import { LoaderService } from '../../service/loader.service';
import { ToastrService } from 'ngx-toastr';
import FileSaver, { saveAs } from 'file-saver';

@Component({
  selector: 'app-day-events',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './day-events.component.html',
  styleUrl: './day-events.component.css'
})
export class DayEventsComponent {
  @Input() todos: todo[] | undefined = [];
  @Input() pastDay: boolean = false;

  constructor(private router: Router, private todoService: TodoService, private loader: LoaderService,
    private toaster: ToastrService
  ) {

  }

  addTodo() {
    this.router.navigateByUrl("/add-todo");
  }

  markComplete() {
    const todoIds = this.todos?.filter(t => t.softComplete).map(t => t.TodoId);
    if (todoIds && todoIds?.length > 0) {
      this.loader.showLoader();
      this.todoService.completeTodos(todoIds).subscribe(
        {
          next: (resp) => {
            this.loader.hideLoader();
            this.toaster.success("Tasks marked complete");
            this.todos?.forEach(element => {
              element.IsCompleted = element.softComplete
            });
          },
          error: (err) => {
            this.loader.hideLoader();
            console.error('Error occurred:', err);
          }
        }
      );
    }
  }

  async getFile(todoId: string) {
    this.loader.showLoader();
    this.todoService.getTodoFile(todoId).subscribe(
      {
        next: (resp) => {
          this.loader.hideLoader();
          if (resp != null) {
            FileSaver.saveAs(resp.Url, resp.FileName);
          }
        },
        error: (err) => {
          this.loader.hideLoader();
          console.error('Error occurred:', err);
        }
      }
    );
  }
}

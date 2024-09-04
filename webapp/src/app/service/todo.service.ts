import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { events } from '../models/events';
import { AddTodo } from '../models/addTodo';
import { HttpClient } from '@angular/common/http';
import { TodoResponse } from '../models/todoResponse';
import { AuthService } from './auth.service';
import { FileResponse } from '../models/fileResponse';
import { CompleteTodoRequest } from '../models/completeTodoRequest';
import { EnvironmentService } from './environment.service';

@Injectable({
  providedIn: 'root'
})
export class TodoService {
  baseUrl: string;
  constructor(private htppClient: HttpClient, private authService: AuthService, envService: EnvironmentService) {
    this.baseUrl = envService.apiUrl;
  }


  getTodos(month: number, year: number): Observable<any> {
    const user = this.authService.getUser();
    if (!user) {
      throw of(Error("Unauthorized"));
    }
    else {
      return this.htppClient.get<any>(this.baseUrl + 'todo/get' + `?email=${user?.Email}` + `&month=${month}` + `&year=${year}`);
    }
  }

  addTodo(todo: AddTodo) {
    return this.htppClient.post<AddTodo>(this.baseUrl + 'todo/add', todo);
  }

  getTodoFile(todoId: string) {
    const user = this.authService.getUser();
    if (!user) {
      throw of(Error("Unauthorized"));
    }
    else {
      return this.htppClient.get<FileResponse | null>(this.baseUrl + 'todo/get-files'
        + `?email=${user?.Email}` + `&token=${user.Otp}` + `&todoId=${todoId}`);
    }
  }

  completeTodos(todos: string[]) {
    const user = this.authService.getUser();
    if (!user) {
      throw of(Error("Unauthorized"));
    }
    else {
      const request: CompleteTodoRequest = {
        Email: user.Email as string,
        Token: user.Otp as number,
        TodoIds: todos
      }
      return this.htppClient.patch<any>(this.baseUrl + 'todo/complete', request);
    }
  }
}

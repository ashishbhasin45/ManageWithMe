import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { CalendarComponent } from './components/calendar/calendar.component';
import { AddTodoComponent } from './components/add-todo/add-todo.component';

export const routes: Routes = [
    { path: '', component: LoginComponent },
    { path: 'login', component: LoginComponent },
    { path: 'todos', component: CalendarComponent },
    { path: 'add-todo', component: AddTodoComponent },
];

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { events, todo } from '../../models/events';
import { TodoService } from '../../service/todo.service';
import { DayEventsComponent } from '../day-events/day-events.component';
import { ToastrService } from 'ngx-toastr';
import { LoaderService } from '../../service/loader.service';
import { startOfDay } from 'date-fns';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, DayEventsComponent],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.css'
})
export class CalendarComponent implements OnInit {
  currentDate: Date = new Date();
  today: Date = startOfDay(new Date());
  selectedDate: Date = new Date();
  daysOfWeek: string[] = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  events: events[] = [];
  selectedDateTodos: todo[] | undefined;
  pastDaySeleced = false;
  constructor(private todoService: TodoService, private toaster: ToastrService, private loader: LoaderService) {

  }

  ngOnInit(): void {
    this.updateEventsForMonth();
  }

  handlePreviousMonth() {
    this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() - 1, 1);
    this.updateEventsForMonth();
  }

  handleNextMonth() {
    this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, 1);
    this.updateEventsForMonth();
  }

  get currentMonth() {
    return this.currentDate.getMonth();
  }

  get currentYear() {
    return this.currentDate.getFullYear();
  }

  get monthDays() {
    return Array(this.daysInMonth).fill(0);
  }

  get weekDays() {
    return Array(this.firstDayOfMonth).fill(0);
  }

  get daysInMonth() {
    return new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, 0).getDate();
  }

  get firstDayOfMonth() {
    return new Date(this.currentDate.getFullYear(), this.currentDate.getMonth(), 1).getDay()
  }

  getEventCount(day: number): number {
    let count = this.events.find((event) =>
      new Date(event.Date).getUTCDate() === day &&
      new Date(event.Date).getUTCMonth() === this.currentMonth &&
      new Date(event.Date).getUTCFullYear() === this.currentYear,
    )?.Todos.length;
    return count ?? 0;
  }

  showEvents(monthDay: number, month: number, year: number) {
    this.selectedDate = new Date(year, month, monthDay);
    this.pastDaySeleced = false;
    if (this.selectedDate < this.today) {
      this.pastDaySeleced = true;
    }
    this.selectedDateTodos = this.events.find(t => this.compareDatesWithoutTime(new Date(t.Date), this.selectedDate))?.Todos;
  }

  updateEventsForMonth() {
    this.loader.showLoader();
    this.todoService.getTodos(this.currentDate.getMonth() + 1, this.currentDate.getFullYear()).subscribe(
      {
        next: (resp) => {
          this.loader.hideLoader();
          this.events = resp.Events;
          this.showEvents(this.today.getDate(), this.today.getMonth(), this.today.getFullYear());
        },
        error: (err) => {
          this.loader.hideLoader();
          this.toaster.error("Something went wrong, please try again");
          console.error('Error occurred:', err);
        }
      }
    );
  }

  compareDatesWithoutTime(date1: Date, date2: Date): boolean {
    return date1.getUTCDate() === date2.getUTCDate() &&
      date1.getUTCMonth() === date2.getUTCMonth() &&
      date1.getUTCFullYear() === date2.getUTCFullYear();
  }
}
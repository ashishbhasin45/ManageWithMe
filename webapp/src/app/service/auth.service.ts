import { Injectable } from '@angular/core';
import { User } from '../models/user';
import { HttpClient } from '@angular/common/http';
import { EnvironmentService } from './environment.service';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  passwordRegex: RegExp = /^(?=[^A-Z]*[A-Z])(?=[^a-z]*[a-z])(?=\D*\d).{8,}$/;
  baseUrl: string;
  constructor(private http: HttpClient, private envService: EnvironmentService) {
    this.baseUrl = envService.apiUrl;
  }

  login(email: string) {
    return this.http.post(this.baseUrl + 'auth/logon', { Email: email });
  }

  verify(model: any) {
    return this.http.post<User>(this.baseUrl + 'auth/verify', model).pipe(
      map(() => {
        this.setCurrentUser(model);
      })
    )
  }

  setCurrentUser(user: User) {
    if (user.Email && user.Otp) {
      localStorage.setItem('access_token', user.Otp.toString());
      localStorage.setItem('userName', user.Email);
    }
  }

  getUser(): User | null {
    if (localStorage.getItem('access_token')) {
      const user: User = {
        Otp: Number.parseInt(localStorage.getItem('access_token') as string),
        Email: localStorage.getItem('userName')
      }

      return user;
    }

    return null;
  }

  logoutUser() {
    localStorage.clear();
  }
}

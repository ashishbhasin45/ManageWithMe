import { CommonModule, JsonPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormGroup, ReactiveFormsModule, FormControl, Validators, FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../service/auth.service';
import { catchError } from 'rxjs';
import { User } from '../../models/user';
import { LoaderService } from '../../service/loader.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, JsonPipe, RouterLink, CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginAttempted = false;
  email: string = '';
  otp!: number;
  constructor(public authService: AuthService, private router: Router, private loader: LoaderService) {
  }

  login() {
    this.loader.showLoader();
    this.authService.login(this.email).subscribe({
      next: (resp) => {
        this.loader.hideLoader();
        this.loginAttempted = true;
      },
      error: (e) => {
        this.loader.hideLoader()
        console.error('Error occurred:', e);
      }
    })
  }

  authorize() {
    const user: User = {
      Email: this.email,
      Otp: this.otp
    }
    this.loader.showLoader();
    this.authService.verify(user).subscribe({
      next: () => {
        this.loader.hideLoader();
        this.router.navigateByUrl('/todos')
      },
      error: (e) => {
        this.loader.hideLoader();
        console.error('Error occurred:', e);
      }
    })
  }
}

import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { NavBarComponent } from './components/nav-bar/nav-bar.component';
import { LoaderService } from './service/loader.service';
import { CommonModule } from '@angular/common';
import { AuthService } from './service/auth.service';
import { EnvironmentService } from './service/environment.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, LoginComponent, NavBarComponent, ProgressSpinnerModule, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'frontend';
  isVisible = false;
  constructor(private loaderService: LoaderService, private authService: AuthService, public envService: EnvironmentService) {
    console.log(envService.apiUrl);
  }

  ngOnInit(): void {
    this.loaderService.isLoading.subscribe(
      {
        next: (x) => this.isVisible = x,
      }
    )
  }

  get isLoggedIn() {
    return this.authService.getUser() != null;
  }
}

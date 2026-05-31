import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, ConfigStateService } from '@abp/ng.core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly configState = inject(ConfigStateService);

  ngOnInit(): void {
    const currentUser = this.configState.getOne('currentUser');

    if (currentUser?.isAuthenticated) {
      this.router.navigateByUrl('/dashboard');
    }
  }

  login(): void {
    this.authService.navigateToLogin();
  }
}
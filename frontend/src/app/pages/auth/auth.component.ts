import { Component, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.scss',
})
export class AuthComponent implements OnInit {
  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    // Check if the user has a valid token
    if (this.authService.isTokenValid()) {
      // Token is valid, redirect to dashboard
      this.router.navigate(['/dashboard']);
    } else {
      // Token is invalid or doesn't exist
      const token = this.authService.getToken();
      if (token) {
        // There was an invalid token, logout to clean up
        this.authService.logout();
        // logout() already redirects to sign-in
      }
      // If no token, stay on auth page which will show sign-in
    }
  }
}

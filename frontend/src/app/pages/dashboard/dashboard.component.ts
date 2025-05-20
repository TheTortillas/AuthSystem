import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit, OnDestroy {
  private tokenValidationInterval: any;
  private readonly warningTimeInSeconds = 300; // Show warning 5 minutes before expiration
  private hasShownWarning = false;

  // User information from token
  username: string | null = null;
  userEmail: string | null = null;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    // Initial token validation
    this.validateCurrentSession();

    // Set up periodic token validation (every 30 seconds)
    this.tokenValidationInterval = setInterval(() => {
      this.validateCurrentSession();
    }, 30000);

    // Get user details from token
    this.loadUserDetails();
  }

  ngOnDestroy(): void {
    // Clean up the interval when component is destroyed
    if (this.tokenValidationInterval) {
      clearInterval(this.tokenValidationInterval);
    }
  }

  private loadUserDetails(): void {
    this.authService.userClaims$.subscribe((claims) => {
      if (claims) {
        this.username = claims.username;
        this.userEmail = claims.email;
        // You can access other claims as needed
      } else {
        // No claims available - token might be invalid
        this.handleTokenExpiration();
      }
    });
  }

  private validateCurrentSession(): void {
    if (!this.authService.isTokenValid()) {
      // Token is expired or invalid
      this.handleTokenExpiration();
      return;
    }

    // Check if token is about to expire
    const claims = this.authService.extractClaims();
    if (claims) {
      const currentTime = Math.floor(Date.now() / 1000);
      const timeToExpire = claims.exp - currentTime;

      // If token is about to expire and we haven't shown a warning yet
      if (timeToExpire <= this.warningTimeInSeconds && !this.hasShownWarning) {
        this.hasShownWarning = true;
        this.showExpirationWarning(timeToExpire);
      }
    }
  }

  handleTokenExpiration() {
    this.authService.clear();
    Swal.fire({
      title: 'Sesión expirada',
      text: 'Tu sesión ha expirado. Por favor, inicia sesión nuevamente.',
      icon: 'warning',
      confirmButtonText: 'Aceptar',
    }).then(() => {
      this.router.navigate(['/dashborard']);
    });
  }

  private showExpirationWarning(timeToExpire: number): void {
    Swal.fire({
      title: 'Sesión a punto de expirar',
      text: `Tu sesión expirará en ${Math.ceil(
        timeToExpire / 60
      )} minutos. ¿Deseas extender la sesión?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, extender sesión',
      cancelButtonText: 'No, mantener sesión actual',
    }).then((result) => {
      if (result.isConfirmed) {
        this.refreshSession();
      }
    });
  }

  private refreshSession(): void {
    // You'll need to add a refreshToken method to your AuthService
    // For now, I'll show how to call it
    this.authService.refreshToken().subscribe({
      next: () => {
        // Session extended successfully
        this.hasShownWarning = false;
        Swal.fire({
          title: 'Sesión extendida',
          text: 'Tu sesión ha sido extendida exitosamente.',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
        });
      },
      error: (error) => {
        console.error('Error al refrescar la sesión:', error);
        Swal.fire({
          title: 'Error',
          text: 'No se pudo extender la sesión. Por favor, inicia sesión nuevamente.',
          icon: 'error',
          confirmButtonText: 'Aceptar',
        }).then(() => {
          this.authService.logout();
        });
      },
    });
  }

  public logout(): void {
    // Optional: Show confirmation dialog
    Swal.fire({
      title: '¿Cerrar sesión?',
      text: '¿Estás seguro de que deseas cerrar la sesión?',
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Sí, cerrar sesión',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#d33',
    }).then((result) => {
      if (result.isConfirmed) {
        // Use the AuthService logout method
        this.authService.logout();

        // This will clear the token and redirect to sign-in
        // The redirect is already handled in the AuthService.logout() method
      }
    });
  }
}

import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EmailManagementService } from '../../core/services/email-management.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './verify-email.component.html',
  styleUrls: ['./verify-email.component.scss'],
})
export class VerifyEmailComponent implements OnInit {
  isVerifying: boolean = true;
  verificationSuccess: boolean = false;
  errorMessage: string | null = null;
  specificErrorType: 'expired' | 'invalid' | 'other' | null = null;
  showErrorState: boolean = false;

  constructor(
    public route: ActivatedRoute,
    private emailManagementService: EmailManagementService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: any
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const token = params['token'];

      if (!token) {
        this.handleError(
          'No se encontró token de verificación en la URL.',
          'invalid'
        );
        return;
      }

      this.verifyEmail(token);
    });
  }

  verifyEmail(token: string): void {
    // Reinicia todos los estados
    this.isVerifying = true;
    this.verificationSuccess = false;
    this.errorMessage = null;
    this.specificErrorType = null;
    this.showErrorState = false;

    this.emailManagementService.verifyEmail(token).subscribe({
      next: (response) => {
        // Primero desactivamos el loader
        this.isVerifying = false;
        // Luego activamos el estado de éxito
        setTimeout(() => {
          this.verificationSuccess = true;

          // Mostrar mensaje de éxito con SweetAlert2 solo en navegador
          if (isPlatformBrowser(this.platformId)) {
            Swal.fire({
              title: '¡Verificación Exitosa!',
              text: response.message,
              icon: 'success',
              confirmButtonText: 'Iniciar Sesión',
              confirmButtonColor: '#3B82F6',
            }).then((result) => {
              if (result.isConfirmed) {
                this.router.navigate(['/auth/sign-in']);
              }
            });
          }
        }, 100);
      },
      error: (error) => {
        // Determinar el tipo de error
        if (error.status === 0) {
          // Ahora tratamos errores de red como errores generales
          this.handleError(
            'Error de conexión. Por favor, verifica tu conexión a internet y vuelve a intentarlo.',
            'other'
          );
        } else if (error.error?.message?.includes('expirado')) {
          this.handleError(
            'El enlace de verificación ha expirado. Por favor, solicita uno nuevo.',
            'expired'
          );
        } else if (
          error.error?.message?.includes('inválido') ||
          error.error?.message?.includes('incorrecto')
        ) {
          this.handleError(
            'El token de verificación es inválido. Por favor, solicita uno nuevo.',
            'invalid'
          );
        } else {
          this.handleError(
            error.error?.message ||
              'Ha ocurrido un error durante la verificación',
            'other'
          );
        }
      },
    });
  }

  handleError(message: string, type: 'expired' | 'invalid' | 'other'): void {
    // Primero desactivamos el loader
    this.isVerifying = false;
    this.verificationSuccess = false;

    // Luego, con un pequeño retraso, mostramos el error
    setTimeout(() => {
      this.errorMessage = message;
      this.specificErrorType = type;

      // Only show SweetAlert in browser environment
      if (isPlatformBrowser(this.platformId)) {
        let title = 'Error de Verificación';
        let confirmButtonText = 'Entendido';
        let showCancelButton = false;
        let cancelButtonText = '';
        this.showErrorState = true;

        switch (type) {
          case 'expired':
            title = 'Enlace Expirado';
            confirmButtonText = 'Solicitar Nuevo Enlace';
            showCancelButton = true;
            cancelButtonText = 'Volver al Inicio';
            break;
          case 'invalid':
            title = 'Token Inválido';
            confirmButtonText = 'Volver al Registro';
            break;
        }

        // Swal.fire({
        //   title: title,
        //   text: message,
        //   icon: 'error',
        //   confirmButtonText: confirmButtonText,
        //   confirmButtonColor: '#3B82F6',
        //   showCancelButton: showCancelButton,
        //   cancelButtonText: cancelButtonText,
        // }).then((result) => {
        //   if (result.isConfirmed) {
        //     if (type === 'expired') {
        //       this.router.navigate(['/auth/resend-verification']);
        //     } else if (type === 'invalid') {
        //       this.router.navigate(['/auth/sign-up']);
        //     }
        //     // Caso 'network' eliminado
        //   } else if (result.dismiss === Swal.DismissReason.cancel) {
        //     this.router.navigate(['/']);
        //   }
        // });
      }
    }, 100);
  }
}

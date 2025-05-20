import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, LoginRequest } from '../../../core/services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './sign-in.component.html',
  styleUrl: './sign-in.component.scss',
})
export class SignInComponent {
  signinForm: FormGroup;
  isSubmitting = false;
  passwordVisible = false;
  loginError = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService, // Use AuthService instead of UserManagementService
    private router: Router
  ) {
    this.signinForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required]],
    });
  }

  togglePasswordVisibility() {
    this.passwordVisible = !this.passwordVisible;
  }

  onSubmit() {
    if (this.signinForm.valid) {
      this.isSubmitting = true;
      this.loginError = '';

      const loginData: LoginRequest = {
        username: this.signinForm.value.username,
        password: this.signinForm.value.password,
      };

      this.authService.signIn(loginData).subscribe({
        // Use authService directly
        next: (response) => {
          // Token is already saved by AuthService.signIn()

          // Mostrar mensaje de éxito
          // Swal.fire({
          //   title: '¡Inicio de sesión exitoso!',
          //   text: 'Has ingresado correctamente.',
          //   icon: 'success',
          //   confirmButtonText: 'Continuar',
          //   confirmButtonColor: '#3B82F6',
          // }).then(() => {
          //   // Redirigir al dashboard o página principal
          //   this.router.navigate(['/dashboard']);
          // });

          this.router.navigate(['/dashboard']);

          this.isSubmitting = false;
        },
        error: (error) => {
          this.isSubmitting = false;

          // Manejo específico de errores según el código de estado
          if (error.status === 401) {
            this.loginError = 'Nombre de usuario o contraseña incorrectos';
          } else if (error.status === 403) {
            this.loginError =
              'Tu cuenta está bloqueada. Contacta al administrador.';
          } else if (error.status === 0) {
            this.loginError =
              'No se pudo conectar con el servidor. Verifica tu conexión.';
          } else {
            this.loginError = error.error?.message || 'Error al iniciar sesión';
          }

          // // También mostramos un toast de error para mejor UX
          // Swal.fire({
          //   title: 'Error',
          //   text: this.loginError,
          //   icon: 'error',
          //   confirmButtonText: 'Entendido',
          //   confirmButtonColor: '#3B82F6',
          // });
        },
      });
    } else {
      this.signinForm.markAllAsTouched();
    }
  }
}

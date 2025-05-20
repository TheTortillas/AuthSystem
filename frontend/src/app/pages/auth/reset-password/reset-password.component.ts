import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EmailManagementService } from '../../../core/services/email-management.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  isSubmitting = false;
  isSuccess = false;
  errorMessage = '';
  token: string | null = null;
  passwordVisible = false;
  confirmPasswordVisible = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private emailService: EmailManagementService
  ) {
    this.resetPasswordForm = this.fb.group(
      {
        password: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', [Validators.required]],
      },
      { validators: this.passwordMatchValidator }
    );
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.token = params['token'];
      if (!this.token) {
        this.errorMessage =
          'Token no proporcionado. Por favor, usa el enlace completo enviado a tu correo electrónico.';
      }
    });
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (
      password &&
      confirmPassword &&
      password.value !== confirmPassword.value
    ) {
      return { passwordMismatch: true };
    }
    return null;
  }

  togglePasswordVisibility(field: string): void {
    if (field === 'password') {
      this.passwordVisible = !this.passwordVisible;
    } else if (field === 'confirmPassword') {
      this.confirmPasswordVisible = !this.confirmPasswordVisible;
    }
  }

  onSubmit() {
    if (!this.token) {
      this.errorMessage =
        'Token no proporcionado. Por favor, usa el enlace completo enviado a tu correo electrónico.';
      return;
    }

    if (this.resetPasswordForm.valid) {
      this.isSubmitting = true;
      this.errorMessage = '';
      const newPassword = this.resetPasswordForm.get('password')?.value;

      this.emailService.resetPassword(this.token, newPassword).subscribe({
        next: (response) => {
          this.isSubmitting = false;
          this.isSuccess = true;
        },
        error: (error) => {
          this.isSubmitting = false;
          this.errorMessage =
            error.error?.message || 'Error al restablecer la contraseña';
        },
      });
    } else {
      this.resetPasswordForm.markAllAsTouched();
    }
  }

  redirectToLogin() {
    this.router.navigate(['/auth/sign-in']);
  }
}

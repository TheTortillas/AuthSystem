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
  passwordStrength = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private emailService: EmailManagementService
  ) {
    this.resetPasswordForm = this.fb.group(
      {
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(12),
            this.uppercaseValidator,
            this.lowercaseValidator,
            this.numberValidator,
            this.symbolValidator,
          ],
        ],
        confirmPassword: ['', [Validators.required]],
      },
      { validators: this.passwordMatchValidator }
    );

    // Subscribe to password changes to calculate strength
    this.resetPasswordForm
      .get('password')
      ?.valueChanges.subscribe((password) => {
        this.calculatePasswordStrength(password);
      });
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

  uppercaseValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasUppercase = /[A-Z]/.test(value);
    return hasUppercase ? null : { hasNoUppercase: true };
  }

  lowercaseValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasLowercase = /[a-z]/.test(value);
    return hasLowercase ? null : { hasNoLowercase: true };
  }

  numberValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasNumber = /[0-9]/.test(value);
    return hasNumber ? null : { hasNoNumber: true };
  }

  symbolValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasSymbol = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(value);
    return hasSymbol ? null : { hasNoSymbol: true };
  }

  calculatePasswordStrength(password: string): void {
    if (!password) {
      this.passwordStrength = '';
      return;
    }

    let score = 0;

    // Length check
    if (password.length >= 12) score += 1;
    if (password.length >= 14) score += 1;

    // Character type checks
    if (/[A-Z]/.test(password)) score += 1;
    if (/[a-z]/.test(password)) score += 1;
    if (/[0-9]/.test(password)) score += 1;
    if (/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) score += 1;

    // Variety check
    const uniqueChars = new Set(password).size;
    if (uniqueChars > password.length * 0.7) score += 1;

    // Extra points for exceptional length
    if (password.length >= 16) score += 1;
    if (password.length >= 20) score += 1;

    // Determine strength level
    if (score <= 2) this.passwordStrength = 'muy-débil';
    else if (score <= 4) this.passwordStrength = 'débil';
    else if (score <= 6) this.passwordStrength = 'media';
    else if (score < 8) this.passwordStrength = 'fuerte';
    else this.passwordStrength = 'muy-fuerte';
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

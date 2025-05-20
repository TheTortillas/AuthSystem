import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { EmailManagementService } from '../../../core/services/email-management.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss',
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  isSubmitting = false;
  requestSent = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private emailService: EmailManagementService
  ) {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit() {
    if (this.forgotPasswordForm.valid) {
      this.isSubmitting = true;
      this.errorMessage = '';

      const email = this.forgotPasswordForm.value.email;

      this.emailService.requestPasswordReset(email).subscribe({
        next: (response) => {
          this.isSubmitting = false;
          this.requestSent = true;
        },
        error: (error) => {
          this.isSubmitting = false;
          this.errorMessage =
            error.error.message ||
            'Error al solicitar restablecimiento de contraseña';
        },
      });
    } else {
      this.forgotPasswordForm.markAllAsTouched();
    }
  }
}

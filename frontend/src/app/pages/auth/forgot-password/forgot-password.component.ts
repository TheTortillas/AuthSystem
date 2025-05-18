import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';

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

  constructor(private fb: FormBuilder) {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit() {
    if (this.forgotPasswordForm.valid) {
      this.isSubmitting = true;
      this.errorMessage = '';

      // Here you would call your password reset service
      console.log('Reset requested for', this.forgotPasswordForm.value.email);

      // Simulate API call
      setTimeout(() => {
        this.isSubmitting = false;
        this.requestSent = true;
        // For demo purposes, you might show an error
        // this.errorMessage = 'Email not found in our records.';
      }, 1500);
    } else {
      this.forgotPasswordForm.markAllAsTouched();
    }
  }
}

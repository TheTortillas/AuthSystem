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

  constructor(private fb: FormBuilder) {
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

      // Here you would call your authentication service
      console.log('Login attempt', this.signinForm.value);

      // Simulate API call
      setTimeout(() => {
        this.isSubmitting = false;
        // For demo purposes, show an error
        // this.loginError = 'Invalid username or password';
      }, 1500);
    } else {
      this.signinForm.markAllAsTouched();
    }
  }
}

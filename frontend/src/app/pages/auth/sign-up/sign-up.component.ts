import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import {
  UserManagementService,
  RegisterRequest,
} from '../../../core/services/user-management.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-sign-up',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.scss',
})
export class SignUpComponent {
  signupForm: FormGroup;
  isSubmitting = false;
  passwordVisible = false;
  confirmPasswordVisible = false;
  passwordStrength = '';

  constructor(
    private fb: FormBuilder,
    private userManagementService: UserManagementService,
    private router: Router
  ) {
    this.signupForm = this.fb.group(
      {
        username: ['', [Validators.required, Validators.minLength(3)]],
        email: ['', [Validators.required, Validators.email]],
        // givenNames: ['', Validators.required],
        // pSurname: ['', Validators.required],
        // mSurname: [''],
        // phoneNumber: [
        //   '',
        //   [Validators.required, Validators.pattern('^[0-9]{10,15}$')],
        // ],
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
        confirmPassword: ['', Validators.required],
      },
      { validators: this.passwordMatchValidator }
    );

    // Subscribe to password changes to calculate strength
    this.signupForm.get('password')?.valueChanges.subscribe((password) => {
      this.calculatePasswordStrength(password);
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
    else this.passwordStrength = 'muy-fuerte'; // Changed from "score <= 8" to "score < 8"
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      form.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    return null;
  }

  togglePasswordVisibility(field: 'password' | 'confirmPassword') {
    if (field === 'password') {
      this.passwordVisible = !this.passwordVisible;
    } else {
      this.confirmPasswordVisible = !this.confirmPasswordVisible;
    }
  }

  onSubmit() {
    if (this.signupForm.valid) {
      this.isSubmitting = true;

      // Extraer datos del formulario
      const formData = this.signupForm.value;

      // Crear objeto para enviar al backend
      const registerData: RegisterRequest = {
        username: formData.username,
        email: formData.email,
        // givenNames: formData.givenNames,
        // pSurname: formData.pSurname,
        // mSurname: formData.mSurname || '',
        // phoneNumber: formData.phoneNumber,
        password: formData.password,
      };

      // Llamar al servicio
      this.userManagementService.signUp(registerData).subscribe({
        next: (response) => {
          this.isSubmitting = false;

          // Mostrar notificación de éxito con Swal
          Swal.fire({
            title: '¡Registro Exitoso!',
            text: response.message,
            icon: 'success',
            confirmButtonText: 'Continuar',
            confirmButtonColor: '#3B82F6',
          }).then((result) => {
            // Cuando el usuario cierra la alerta, redirigir a sign-in
            if (result.isConfirmed) {
              this.router.navigate(['/auth/sign-in']);
            }
          });
        },
        error: (error) => {
          this.isSubmitting = false;

          // Mostrar notificación de error con Swal
          Swal.fire({
            title: '¡Error!',
            text: error.error?.message || 'Error al registrar usuario',
            icon: 'error',
            confirmButtonText: 'Intentar nuevamente',
            confirmButtonColor: '#3B82F6',
          });

          console.error('Error en registro:', error);
        },
      });
    } else {
      // Mostrar mensaje si el formulario es inválido
      Swal.fire({
        title: 'Datos Incompletos',
        text: 'Por favor completa correctamente todos los campos requeridos.',
        icon: 'warning',
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#3B82F6',
      });

      this.signupForm.markAllAsTouched();
    }
  }
}

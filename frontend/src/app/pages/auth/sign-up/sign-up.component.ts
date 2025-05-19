import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
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

  constructor(
    private fb: FormBuilder,
    private userManagementService: UserManagementService,
    private router: Router
  ) {
    this.signupForm = this.fb.group(
      {
        username: ['', [Validators.required, Validators.minLength(3)]],
        email: ['', [Validators.required, Validators.email]],
        givenNames: ['', Validators.required],
        pSurname: ['', Validators.required],
        mSurname: [''],
        phoneNumber: [
          '',
          [Validators.required, Validators.pattern('^[0-9]{10,15}$')],
        ],
        password: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', Validators.required],
      },
      { validators: this.passwordMatchValidator }
    );
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
        givenNames: formData.givenNames,
        pSurname: formData.pSurname,
        mSurname: formData.mSurname || '',
        phoneNumber: formData.phoneNumber,
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

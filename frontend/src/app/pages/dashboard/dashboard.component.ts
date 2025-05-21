import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import Swal from 'sweetalert2';
import { SafeHtml, DomSanitizer } from '@angular/platform-browser';
import jsPDF from 'jspdf';

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

  // Cryptography topics
  selectedTopic: string | null = null;
  topicTitle: string = '';
  topicContent: SafeHtml = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private sanitizer: DomSanitizer
  ) {}

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

  // Navigate to a cryptography topic
  navigateToCryptoTopic(topic: string): void {
    this.selectedTopic = topic;

    switch (topic) {
      case 'clasica':
        this.topicTitle = 'Criptografía Clásica';
        this.topicContent = this.sanitizer.bypassSecurityTrustHtml(`
          <p>La criptografía clásica abarca métodos de cifrado utilizados históricamente antes de la era computacional moderna.</p>
          <p>Incluye técnicas como:</p>
          <ul>
            <li>Cifrado César</li>
            <li>Cifrado por sustitución</li>
            <li>Cifrado de Vigenère</li>
          </ul>
          <p>Estos métodos fueron fundamentales en la comunicación segura durante siglos.</p>
        `);
        break;
      case 'moderna':
        this.topicTitle = 'Criptografía Moderna';
        this.topicContent = this.sanitizer.bypassSecurityTrustHtml(`
          <p>La criptografía moderna utiliza matemáticas avanzadas y computación para crear sistemas de cifrado seguros.</p>
          <p>Los principales tipos son:</p>
          <ul>
            <li>Criptografía de clave simétrica</li>
            <li>Criptografía de clave pública</li>
            <li>Funciones hash criptográficas</li>
          </ul>
          <p>Estos sistemas protegen las comunicaciones digitales en la actualidad.</p>
        `);
        break;
      case 'enigma':
        this.topicTitle = 'Máquina Enigma';
        this.topicContent = this.sanitizer.bypassSecurityTrustHtml(`
          <p>La máquina Enigma fue un dispositivo electromecánico utilizado por Alemania durante la Segunda Guerra Mundial para cifrar y descifrar mensajes secretos. Su complejidad y eficiencia la convirtieron en un pilar de las comunicaciones militares nazis.</p>
          <h3>¿Cómo Funcionaba?</h3>
          <p>Esta máquina era un conjunto de rotores que se conectaban eléctricamente entre sí, cada rotor tenía 26 contactos, correspondientes a las letras del alfabeto, así, al pulsar una tecla, una corriente eléctrica...</p>
        `);
        break;
      case 'tendencias':
        this.topicTitle = 'Tendencias Criptográficas';
        this.topicContent = this.sanitizer.bypassSecurityTrustHtml(`
          <p>Las tendencias actuales en criptografía incluyen:</p>
          <ul>
            <li>Criptografía post-cuántica</li>
            <li>Criptografía homomórfica</li>
            <li>Blockchain y criptomonedas</li>
            <li>Zero-knowledge proofs</li>
          </ul>
          <p>Estas tecnologías emergentes están definiendo el futuro de la seguridad digital.</p>
        `);
        break;
      default:
        this.topicTitle = '';
        this.topicContent = '';
    }
  }

  // Generate PDF with the current topic information
  generatePDF(): void {
    if (!this.selectedTopic) return;

    // Create a new PDF document
    const pdf = new jsPDF();
    const pageWidth = pdf.internal.pageSize.getWidth();

    // Set initial position for text
    let y = 20;

    // Add title
    pdf.setFont('helvetica', 'bold');
    pdf.setFontSize(22);

    // Center the title
    const titleText = 'CRIPTOGRAFÍA';
    const titleWidth =
      (pdf.getStringUnitWidth(titleText) * pdf.getFontSize()) /
      pdf.internal.scaleFactor;
    const titleX = (pageWidth - titleWidth) / 2;
    pdf.text(titleText, titleX, y);
    y += 15;

    // Add topic title
    pdf.setFontSize(18);
    pdf.text(this.topicTitle, 20, y);
    y += 15;

    // Reset font for content
    pdf.setFont('helvetica', 'normal');
    pdf.setFontSize(12);

    // Extract plain text from HTML content
    let plainContent = '';

    switch (this.selectedTopic) {
      case 'clasica':
        plainContent =
          'La criptografía clásica abarca métodos de cifrado utilizados históricamente antes de la era computacional moderna.\n\n' +
          'Incluye técnicas como:\n' +
          '• Cifrado César\n' +
          '• Cifrado por sustitución\n' +
          '• Cifrado de Vigenère\n\n' +
          'Estos métodos fueron fundamentales en la comunicación segura durante siglos.';
        break;
      case 'moderna':
        plainContent =
          'La criptografía moderna utiliza matemáticas avanzadas y computación para crear sistemas de cifrado seguros.\n\n' +
          'Los principales tipos son:\n' +
          '• Criptografía de clave simétrica\n' +
          '• Criptografía de clave pública\n' +
          '• Funciones hash criptográficas\n\n' +
          'Estos sistemas protegen las comunicaciones digitales en la actualidad.';
        break;
      case 'enigma':
        plainContent =
          'La máquina Enigma fue un dispositivo electromecánico utilizado por Alemania durante la Segunda Guerra Mundial para ' +
          'cifrar y descifrar mensajes secretos. Su complejidad y eficiencia la convirtieron en un pilar de las comunicaciones militares nazis.\n\n' +
          '¿Cómo Funcionaba?\n\n' +
          'Esta máquina era un conjunto de rotores que se conectaban eléctricamente entre sí, cada rotor tenía 26 contactos, correspondientes ' +
          'a las letras del alfabeto, así, al pulsar una tecla, una corriente eléctrica...';
        break;
      case 'tendencias':
        plainContent =
          'Las tendencias actuales en criptografía incluyen:\n\n' +
          '• Criptografía post-cuántica\n' +
          '• Criptografía homomórfica\n' +
          '• Blockchain y criptomonedas\n' +
          '• Zero-knowledge proofs\n\n' +
          'Estas tecnologías emergentes están definiendo el futuro de la seguridad digital.';
        break;
      default:
        plainContent = '';
    }

    // Add content text with word wrapping
    const splitText = pdf.splitTextToSize(plainContent, pageWidth - 40);
    pdf.text(splitText, 20, y);

    // Calculate new y position after text
    y = pdf.getTextDimensions(splitText).h + y + 20;

    // Add footer
    pdf.setFontSize(10);
    const footerText =
      '© 2024 Criptografía CNC. Todos los derechos reservados.';
    const footerWidth =
      (pdf.getStringUnitWidth(footerText) * pdf.getFontSize()) /
      pdf.internal.scaleFactor;
    const footerX = (pageWidth - footerWidth) / 2;

    // Ensure footer is always at bottom of page
    const bottomY = pdf.internal.pageSize.getHeight() - 10;
    pdf.text(footerText, footerX, bottomY);

    // Save the PDF
    pdf.save(`${this.topicTitle.toLowerCase().replace(/\s+/g, '_')}.pdf`);

    // Show success message
    Swal.fire({
      title: 'PDF Generado',
      text: 'El PDF se ha generado correctamente.',
      icon: 'success',
      confirmButtonText: 'Aceptar',
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
    // Only show expiration message if we haven't manually logged out
    if (this.authService.getToken()) {
      this.router.navigate(['/auth/sign-in']);

      this.authService.clear();
      Swal.fire({
        title: 'Sesión expirada',
        text: 'Tu sesión ha expirado. Por favor, inicia sesión nuevamente.',
        icon: 'warning',
        confirmButtonText: 'Aceptar',
      });
    } else {
      // If no token exists, just redirect without showing the message
      this.router.navigate(['/auth/sign-in']);
    }
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
        // Use the AuthService logout method but don't trigger the token expiration flow
        // Clear token and update authentication state
        this.authService.clear();

        // Redirect directly to sign-in page without showing the "Session expired" message
        this.router.navigate(['/auth/sign-in']);
      }
    });
  }
}

import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, of, throwError } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { jwtDecode } from 'jwt-decode';

const TOKEN_KEY = 'auth_token';

const HttpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Access-Control-Allow-Origin': '*',
  }),
};

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
}

export interface TokenClaims {
  // User information
  id: string;
  username: string;
  email: string;
  givennames: string;
  psurname: string;
  msurname: string;
  phonenumber: string;
  createdat: string;
  lastlogin: string;
  role?: string; // Keep this if your system uses roles

  // Standard JWT claims - important for validation
  nbf: number; // Not Before timestamp
  exp: number; // Expiration timestamp
  iat: number; // Issued At timestamp
  iss: string; // Issuer
  aud: string; // Audience

  // Allow additional properties
  [key: string]: any;
}
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(
    this.hasValidToken()
  );
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  public userClaimsSubject = new BehaviorSubject<TokenClaims | null>(
    this.extractClaims()
  );
  public userClaims$ = this.userClaimsSubject.asObservable();

  private isLoggingOut = false;

  private URLBase = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    // Periodically check token validity (every minute)
    if (isPlatformBrowser(this.platformId)) {
      setInterval(() => this.checkTokenValidity(), 60000);
    }
  }

  signIn(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(
        `${this.URLBase}/UserManagement/SignInUsername`,
        credentials,
        HttpOptions
      )
      .pipe(
        tap((response) => {
          this.setToken(response.token);
          this.isAuthenticatedSubject.next(true);
          const claims = this.extractClaims();
          this.userClaimsSubject.next(claims);
        })
      );
  }

  logout(): void {
    // Si ya estamos en proceso de logout, no hacer nada
    if (this.isLoggingOut) {
      return;
    }

    // Marcar que estamos en proceso de logout
    this.isLoggingOut = true;

    try {
      // 1. Borrar el token del localStorage
      this.removeItem(TOKEN_KEY);

      // 2. Actualizar los observables para los suscriptores
      this.isAuthenticatedSubject.next(false);
      this.userClaimsSubject.next(null);

      // 3. Redirigir al login
      this.router.navigate(['/auth/sign-in']);
    } finally {
      // Restablecer la bandera cuando termine, con o sin error
      setTimeout(() => {
        this.isLoggingOut = false;
      }, 100);
    }
  }

  clear(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.clear();
    }
  }

  // Platform-aware localStorage methods
  getItem(key: string): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(key);
    }
    return null;
  }

  setItem(key: string, value: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(key, value);
    }
  }

  removeItem(key: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(key);
    }
  }

  // Token methods
  getToken(): string | null {
    return this.getItem(TOKEN_KEY);
  }

  private setToken(token: string): void {
    this.setItem(TOKEN_KEY, token);
  }

  private hasToken(): boolean {
    return !!this.getToken();
  }

  // JWT specific methods
  public extractClaims(): TokenClaims | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      return jwtDecode<TokenClaims>(token);
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  refreshToken(): Observable<AuthResponse> {
    // Get current token
    const token = this.getToken();

    // If no token, we can't refresh
    if (!token) {
      return throwError(() => new Error('No token available for refresh'));
    }

    // Headers with the current token for authorization
    const refreshHeaders = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    });

    // Call refresh token endpoint
    return this.http
      .post<AuthResponse>(
        `${this.URLBase}/UserManagement/RefreshToken`,
        {},
        { headers: refreshHeaders }
      )
      .pipe(
        tap((response) => {
          // Save the new token
          this.setToken(response.token);

          // Update authentication state with the new token
          this.isAuthenticatedSubject.next(true);

          // Extract and update user claims
          const claims = this.extractClaims();
          this.userClaimsSubject.next(claims);
        })
      );
  }

  isTokenValid(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const decoded = jwtDecode<TokenClaims>(token);
      const currentTime = Math.floor(Date.now() / 1000);

      // Check expiration - token must not be expired
      if (decoded.exp <= currentTime) {
        console.log('Token expired');
        return false;
      }

      // Check "not before" - token must not be used before nbf
      if (decoded.nbf && decoded.nbf > currentTime) {
        console.log('Token not yet valid (before nbf time)');
        return false;
      }

      // Additional validation for issuer if needed
      // if (decoded.iss !== 'your-expected-issuer') {
      //   console.log('Invalid token issuer');
      //   return false;
      // }

      return true;
    } catch (error) {
      console.error('Error validating token:', error);
      return false;
    }
  }

  private hasValidToken(): boolean {
    return this.hasToken() && this.isTokenValid();
  }

  // Auto-check token validity
  private checkTokenValidity(): void {
    const isValid = this.hasValidToken();

    // If token was valid but now is invalid, logout user
    if (this.isAuthenticatedSubject.value && !isValid) {
      this.logout();
    }

    // Update authentication state if needed
    if (this.isAuthenticatedSubject.value !== isValid) {
      this.isAuthenticatedSubject.next(isValid);
    }
  }

  // Legacy method kept for compatibility
  isLoggedIn(): boolean {
    return this.hasValidToken();
  }
}

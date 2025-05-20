import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Observable } from 'rxjs';

const HttpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Access-Control-Allow-Origin': '*',
  }),
};

export interface RegisterRequest {
  username: string;
  email: string;
  givenNames: string;
  pSurname: string;
  mSurname?: string;
  phoneNumber: string;
  password: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface ApiResponse {
  message: string;
}

export interface AuthResponse {
  token: string;
}

@Injectable({
  providedIn: 'root',
})
export class UserManagementService {
  constructor(private httpClient: HttpClient) {}
  private URLBase = environment.apiUrl;

  public signUp(userData: RegisterRequest): Observable<ApiResponse> {
    const url = this.URLBase + '/UserManagement/SignUp';
    return this.httpClient.post<ApiResponse>(url, userData, HttpOptions);
  }

  public signIn(loginData: LoginRequest): Observable<AuthResponse> {
    const url = this.URLBase + '/UserManagement/SignInUsername';
    return this.httpClient.post<AuthResponse>(url, loginData, HttpOptions);
  }

  public saveToken(token: string): void {
    localStorage.setItem('auth_token', token);
  }

  public getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  public isLoggedIn(): boolean {
    return !!this.getToken();
  }

  public logout(): void {
    localStorage.removeItem('auth_token');
  }
}

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

export interface ApiResponse {
  message: string;
}

export interface EmailRequestDTO {
  email: string;
}

export interface ResetPasswordRequestDTO {
  token: string;
  newPassword: string;
}

@Injectable({
  providedIn: 'root',
})
export class EmailManagementService {
  constructor(private httpClient: HttpClient) {}
  private URLBase = environment.apiUrl;

  public verifyEmail(token: string): Observable<ApiResponse> {
    const url = `${this.URLBase}/EmailVerification/verify?token=${token}`;
    return this.httpClient.get<ApiResponse>(url, HttpOptions);
  }

  public requestPasswordReset(email: string): Observable<ApiResponse> {
    const url = `${this.URLBase}/PasswordReset/forgot-password`;
    const body: EmailRequestDTO = { email };
    return this.httpClient.post<ApiResponse>(url, body, HttpOptions);
  }

  public resetPassword(
    token: string,
    newPassword: string
  ): Observable<ApiResponse> {
    const url = `${this.URLBase}/PasswordReset/reset-password`;
    const body: ResetPasswordRequestDTO = { token, newPassword };
    return this.httpClient.post<ApiResponse>(url, body, HttpOptions);
  }
}

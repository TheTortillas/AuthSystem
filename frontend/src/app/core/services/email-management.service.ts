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
}

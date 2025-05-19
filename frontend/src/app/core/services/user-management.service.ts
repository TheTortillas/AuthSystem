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

export interface ApiResponse {
  message: string;
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
}

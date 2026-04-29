import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface GenerateCertificateDto {
  userId: string;
  courseId: string;
  userName: string;
  userEmail: string;
  courseTitle: string;
}

export interface CertificateResponseDto {
  certificateId: string;
  filePath: string;
}

@Injectable({
  providedIn: 'root'
})
export class CertificateService {
  private baseUrl = 'http://localhost:5271/gateway/certificates';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  private get authHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.authService.getAccessToken()}`
    });
  }

  generateCertificate(dto: GenerateCertificateDto): Observable<CertificateResponseDto> {
    return this.http.post<CertificateResponseDto>(`${this.baseUrl}/generate`, dto, {
      headers: this.authHeaders
    });
  }

  downloadCertificate(certificateId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/download/${certificateId}`, {
      headers: this.authHeaders,
      responseType: 'blob'
    });
  }
}

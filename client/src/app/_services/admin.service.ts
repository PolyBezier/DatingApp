import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { UserPhoto } from '../_models/userPhoto';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsersWithRoles() {
    return this.http.get<User[]>(`${this.baseUrl}/admin/users-with-roles`);
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.put<string[]>(`${this.baseUrl}/admin/edit-roles/${username}?roles=${roles}`, {});
  }

  getPhotosToModerate() {
    return this.http.get<UserPhoto[]>(`${this.baseUrl}/admin/photos-to-moderate`);
  }

  approvePhoto(username: string, photoId: number) {
    return this.http.put(`${this.baseUrl}/admin/approve-photo/${username}/${photoId}`, {});
  }
}

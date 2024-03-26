import { Component, OnInit } from '@angular/core';
import { UserPhoto } from 'src/app/_models/userPhoto';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: UserPhoto[] = [];

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.adminService.getPhotosToModerate().subscribe({
      next: photos => this.photos = photos
    });
  }

  approvePhoto(username: string, photoId: number) {
    this.adminService.approvePhoto(username, photoId).subscribe({
      next: () => this.photos = this.photos.filter(p => p.id !== photoId)
    });
  }
}

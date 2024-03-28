import { CommonModule } from '@angular/common';
import { AfterViewChecked, ChangeDetectionStrategy, Component, ElementRef, Input, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
import { MessagesService } from 'src/app/_services/messages.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  standalone: true,
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  imports: [CommonModule, TimeagoModule, FormsModule]
})
export class MemberMessagesComponent implements AfterViewChecked {
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild('scrollMe') scroll?: ElementRef;
  @Input() username?: string;
  messageContent = '';
  loading = false;

  constructor(public messageService: MessagesService) { }

  ngAfterViewChecked(): void {
    this.scrollDown();
  }

  sendMessage() {
    if (!this.username) {
      return;
    }

    this.loading = true;

    this.messageService.sendMessage(this.username, this.messageContent)
      .then(() => this.messageForm?.reset())
      .finally(() => this.loading = false);
  }

  scrollDown() {
    try {
      this.scroll!.nativeElement.scrollTop = this.scroll?.nativeElement.scrollHeight;
    } catch { }
  }
}

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ReviewerIdentityService {
    public reviewId: number = 0;
    public ticket: string = "";
    public userId: number = 0;
    public name: string = "";
    public accountExpiration: string = "";
    public reviewExpiration: string = "";
    public isSiteAdmin: boolean = false;
    public loginMode: string = "";
    public roles: string[] = [];
    public token: string = "";
    isAuthenticated: boolean = false;
    public Report()  {
        console.log('Reporting Cid: ' + this.userId);
        console.log('And also: ' + this.name);
        console.log('Expires on: ' + this.accountExpiration);
    }
}

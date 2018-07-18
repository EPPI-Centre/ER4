import { Injectable } from '@angular/core';

@Injectable()
export class ReviewerIdentityService {
    public ReviewId: number = 0;
    public LogonTicket: string = "";
    public ContactId: number = 0;
    public ContactName: string = "";
    public AccountExpiration: string = "";
    public ReviewExpiration: string = "";

    public Report()  {
        console.log('Reporting Cid: ' + this.ContactId);
    }
}

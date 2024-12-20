//built starting from http://jasonwatmore.com/post/2018/05/16/angular-6-user-registration-and-login-example-tutorial
import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReviewerIdentity, ReviewerIdentityService } from '../services/revieweridentity.service';
 
@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    constructor(private ReviewerIdentityServ: ReviewerIdentityService) {}
    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        //const userJson = localStorage.getItem('currentErUser');
        
        //let currentUser: ReviewerIdentity = userJson !== null ? JSON.parse(userJson) : new ReviewerIdentity();
		
        const currentUser: ReviewerIdentity = this.ReviewerIdentityServ.reviewerIdentity;
        const chk: boolean = currentUser && currentUser.token && !currentUser.token.startsWith("Error: ") ?  true : false;
        if (request.method == 'POST') {
            if (chk) {
                if (request.url.indexOf('ItemDocumentList/Upload') > 0
                    || request.url.indexOf('WebDB/UploadImage') > 0
                ) {
                    request = request.clone({
                        setHeaders: {
                            Authorization: `Bearer ${currentUser.token}`
                        }
                    });
                }
                else {
                    request = request.clone({
                        setHeaders: {
                            Authorization: `Bearer ${currentUser.token}`,
                            'Content-Type': 'application/json; charset=utf-8'
                        }
                    });
                }
            }
            else {
                request = request.clone({
                    setHeaders: {
                        'Content-Type': 'application/json; charset=utf-8'
                    }
                });
            }
        }
        else {
            if (chk) {
                request = request.clone({
                    setHeaders: {
                        Authorization: `Bearer ${currentUser.token}`,
                    }
                });
            }
        }
        //if (request.url.endsWith('TrainingRunCommand')) {
        //    request = request.clone({
        //        setHeaders: {
        //            timeout: `20000`,
        //        }
        //    });
        //}
 
        return next.handle(request);
    }
}
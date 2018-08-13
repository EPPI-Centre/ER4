//built starting from http://jasonwatmore.com/post/2018/05/16/angular-6-user-registration-and-login-example-tutorial
import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReviewerIdentity } from '../services/revieweridentity.service';
 
@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        const userJson = localStorage.getItem('currentErUser');
        
        let currentUser: ReviewerIdentity = userJson !== null ? JSON.parse(userJson) : new ReviewerIdentity();
        console.log('Intercepted! Cid =' + currentUser.userId);
        console.log(request.method);//&& request.method == 'POST'
        if (request.method == 'POST') {
            if (currentUser && currentUser.token) {
                request = request.clone({
                    setHeaders: {
                        Authorization: `Bearer ${currentUser.token}`,
                        'Content-Type': 'application/json; charset=utf-8'
                    }
                });
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
            if (currentUser && currentUser.token) {
                request = request.clone({
                    setHeaders: {
                        Authorization: `Bearer ${currentUser.token}`,
                    }
                });
            }
        }
 
        return next.handle(request);
    }
}
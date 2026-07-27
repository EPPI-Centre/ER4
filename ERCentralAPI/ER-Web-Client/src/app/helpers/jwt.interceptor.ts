//built starting from http://jasonwatmore.com/post/2018/05/16/angular-6-user-registration-and-login-example-tutorial
import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, retry, throwError, timer } from 'rxjs';
import { ReviewerIdentity, ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from './HelperMethods';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private ReviewerIdentityServ: ReviewerIdentityService) { }

  private readonly MAX_NUMBER_OF_RETRY: number = 5;
  private readonly DEFAULT_RETRY_DELAY: number = 500;//miliseconds
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    //const userJson = localStorage.getItem('currentErUser');

    //let currentUser: ReviewerIdentity = userJson !== null ? JSON.parse(userJson) : new ReviewerIdentity();

    const currentUser: ReviewerIdentity = this.ReviewerIdentityServ.reviewerIdentity;
    const chk: boolean = currentUser && currentUser.token && !currentUser.token.startsWith("Error: ") ? true : false;
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

    return next.handle(request).pipe(
      retry({
        count: this.MAX_NUMBER_OF_RETRY,
        delay: (error: HttpErrorResponse, retryAttempt: number): Observable<number> => {
          // if maximum number of retries have been met, thow error
          if (retryAttempt > this.MAX_NUMBER_OF_RETRY) {
            return throwError(() => error);
          }
          if (error.status == 429) {
            let delay = this.DEFAULT_RETRY_DELAY;
            if (error.headers.has('Retry-After')) {
              let tmpSt = error.headers.get('Retry-After'); //gives a value in seconds
              console.log("Got RETRY-AFTER: " +tmpSt + ". URL:" + error.url?.toString());
              var tmp: number | null = null;
              if (tmpSt) tmp = Helpers.SafeParseInt(tmpSt);
              if (tmp) delay = tmp * 1000;
            }


            console.log(`Attempt ${retryAttempt}: retrying in ${delay}ms`);
            // retry after 1s, 2s, etc...
            return timer(delay);
          }
          // or response is a status code we don't wish to retry, throw error
          //console.log("Error but wouldn't retry... Count is:", retryAttempt);
          return throwError(() => error);
        },
      })     
    );
  }
}



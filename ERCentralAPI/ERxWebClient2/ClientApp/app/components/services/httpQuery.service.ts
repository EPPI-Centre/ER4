import { Injectable } from '@angular/core';
import { Http, Response, Headers, RequestOptions, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';

// Observable class extensions

// Observable operators
import { map } from 'rxjs/operators';

@Injectable()
export class WebApiObservableService {
	headers: Headers = new Headers();
	options: RequestOptions = new RequestOptions();

	constructor(private _httpC: HttpClient) {
		this.headers = new Headers({
			'Content-Type': 'application/json; charset=utf-8',
			'Accept': 'application/json, text/plain, */ *'
		});
		this.options = new RequestOptions({ headers: this.headers });
	}

	getServiceWithComplexObjectAsQueryString(url: string, param: any): Observable<any> {
		let params: HttpParams = new HttpParams();
		for (var key in param) {
			if (param.hasOwnProperty(key)) {
				let val = param[key];
				params.set(key, val);
			}
		}
		this.options = new RequestOptions({ headers: this.headers, params: params });
		return this._httpC
			.get(url, { params })
			.pipe(map(() => { this.extractData }))
			//.catch(this.handleError)

	}

	private extractData(res: Response) {
		let body = res.json();
		return body || {};
	}

	private handleError(error: any) {
		let errMsg = (error.message) ? error.message :
			error.status ? `${error.status} - ${error.statusText}` : 'Server error';
		console.error(errMsg);
		return Observable.throw(errMsg);
	}
}
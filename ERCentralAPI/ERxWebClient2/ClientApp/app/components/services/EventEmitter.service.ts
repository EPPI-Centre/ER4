import { Injectable, EventEmitter } from '@angular/core';

@Injectable()
export class EventEmitterService {
	public PleaseSelectItemsListTab = new EventEmitter();
	public nodeSelected: boolean = false;
	public nodeName: string = '';

	constructor() { }
}
import { Injectable, EventEmitter } from '@angular/core';
import { singleNode } from './ReviewSets.service';

@Injectable()
export class EventEmitterService {
	public PleaseSelectItemsListTab = new EventEmitter();
	public nodeSelected: singleNode | null | undefined;
	public nodeName: string = '';

	constructor() {

	}
}
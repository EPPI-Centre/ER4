import { Injectable, EventEmitter } from '@angular/core';

@Injectable()
export class EventEmitterService {

	dataStr = new EventEmitter();
	tabSelectEventf = new EventEmitter();
	showFreqView = new EventEmitter();
	tabChange = new EventEmitter();

	public codingTreeVar: boolean = false;
	public nodeSelected: boolean = false;
	public nodeName: string = '';

	constructor() { }

	sendMessage(data: any) {
		this.dataStr.emit(data);
	}

	selectTabItems() {
		this.tabSelectEventf.emit();
	}

	showData(value: any) {

		this.showFreqView.emit(value);
	}

	tabSelected(value: any) {

		if (value.nextId != 'SearchListTab') {
			this.codingTreeVar = false;
		}
		this.tabChange.emit(value);
	}

}
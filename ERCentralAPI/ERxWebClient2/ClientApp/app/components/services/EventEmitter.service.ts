import { Injectable, EventEmitter } from '@angular/core';

@Injectable()
export class EventEmitterService {

	dataStr = new EventEmitter();
	tabSelectEventf = new EventEmitter();
	showFreqView = new EventEmitter();
	dropDownChange = new EventEmitter();

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

	dropDownSelect(value: any) {

		this.dropDownChange.emit(value);
	}

}
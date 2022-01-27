import { Injectable } from '@angular/core';
import * as FileSaver from '@progress/kendo-file-saver';
import * as XLSX from 'xlsx';

const EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
const EXCEL_EXTENSION = '.xlsx';


@Injectable({
	providedIn: 'root',
})
export class ExcelService {

  constructor() { }

  public exportAsExcelFile(json: any[], excelFileName: string): void {

	  const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(json);

    //const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(json);
    //console.log('worksheet',worksheet);
    const workbook: XLSX.WorkBook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
    const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    //const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'buffer' });
    this.saveAsExcelFile(excelBuffer, excelFileName);
  }
    public exportHISscreeningFile(json: any[], excelFileName: string) {
        const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(json, {} as XLSX.JSON2SheetOpts);
        if (worksheet) {
            worksheet['!cols'] = [];
            let cols = worksheet['!cols'];
            if (cols) {
                cols.push({ wch: 10 });
                cols.push({ wch: 20 });
                cols.push({ wch: 15 });
                cols.push({ wch: 4 });
                cols.push({ wch: 30 });
                cols.push({ wch: 30 });
                cols.push({ wch: 15 });
                cols.push({ wch: 6 });
                cols.push({ wch: 6 });
                cols.push({ wch: 9 });
                cols.push({ wch: 13 });
                cols.push({ wch: 20 });
                cols.push({ wch: 20 });
                cols.push({ wch: 80 });
                cols.push({ wch: 80 });
                cols.push({ wch: 80 });
            }
        }

        //console.log('worksheet',worksheet);
        const workbook: XLSX.WorkBook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };

        const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
        //const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'buffer' });
        this.saveAsExcelFile(excelBuffer, excelFileName);
    }
  private saveAsExcelFile(buffer: any, fileName: string): void {
    const data: Blob = new Blob([buffer], {
      type: EXCEL_TYPE
    });
    FileSaver.saveAs(data, fileName + EXCEL_EXTENSION);
  }

}
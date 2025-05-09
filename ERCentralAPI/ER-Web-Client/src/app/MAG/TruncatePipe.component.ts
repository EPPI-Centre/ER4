import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'truncate'
})
export class TruncatePipe implements PipeTransform {
    transform(value: string, limit = 60, completeWords = true, ellipsis = '...') {
        if (completeWords) {
            limit = value.substring(0, limit).lastIndexOf(' ');
        }
      return value.length > limit ? value.substring(0, limit) + ellipsis : value;
    }
}

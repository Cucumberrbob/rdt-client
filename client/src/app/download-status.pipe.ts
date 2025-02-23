import { Pipe, PipeTransform } from '@angular/core';
import { Download } from './models/download.model';
import { filesize } from './filesize.util';

@Pipe({
  name: 'downloadStatus',
})
export class DownloadStatusPipe implements PipeTransform {
  transform(value: Download): string {
    if (!value) {
      return 'Pending';
    }

    if (value.error) {
      return value.error;
    }

    if (value.completed != null) {
      return 'Finished';
    }

    if (value.unpackingFinished) {
      return 'Unpacking finished';
    }

    if (value.unpackingStarted) {
      let progress = (value.bytesDone / value.bytesTotal) * 100;

      if (isNaN(progress)) {
        progress = 0;
      }

      return `Unpacking ${progress.toFixed(2)}%`;
    }

    if (value.unpackingQueued) {
      return 'Unpacking queued';
    }

    if (value.downloadFinished) {
      return 'Download finished';
    }

    if (value.downloadStarted) {
      let progress = (value.bytesDone / value.bytesTotal) * 100;

      if (isNaN(progress)) {
        progress = 0;
      }

      const speed = filesize(value.speed);

      return `Downloading ${progress.toFixed(2)}% (${speed}/s)`;
    }

    if (value.downloadQueued) {
      return 'Download queued';
    }

    return 'Pending';
  }
}

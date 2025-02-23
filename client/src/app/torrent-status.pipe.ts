import { Pipe, PipeTransform } from '@angular/core';
import { RealDebridStatus, Torrent } from './models/torrent.model';
import { filesize } from './filesize.util';

@Pipe({
  name: 'status',
})
export class TorrentStatusPipe implements PipeTransform {
  transform(torrent: Torrent): string {
    if (torrent.error) {
      return torrent.error;
    }

    if (torrent.downloads.length > 0) {
      const allFinished = torrent.downloads.every((m) => m.completed != null);

      if (allFinished) {
        return 'Finished';
      }

      const downloading = torrent.downloads.filter((m) => m.downloadStarted && !m.downloadFinished && m.bytesDone > 0);
      const downloaded = torrent.downloads.filter((m) => m.downloadFinished != null);

      if (downloading.length > 0) {
        const bytesDone = downloading.reduce((sum, m) => sum + m.bytesDone, 0);
        const bytesTotal = downloading.reduce((sum, m) => sum + m.bytesTotal, 0);
        let progress = (bytesDone / bytesTotal) * 100;

        if (isNaN(progress)) {
          progress = 0;
        }

        let allSpeeds = downloading.reduce((sum, m) => sum + m.speed, 0);

        const speed = filesize(allSpeeds);

        return `Downloading file ${downloading.length + downloaded.length}/${
          torrent.downloads.length
        } (${progress.toFixed(2)}% - ${speed}/s)`;
      }

      const unpacking = torrent.downloads.filter((m) => m.unpackingStarted && !m.unpackingFinished && m.bytesDone > 0);
      const unpacked = torrent.downloads.filter((m) => m.unpackingFinished != null);

      if (unpacking.length > 0) {
        const bytesDone = unpacking.reduce((sum, m) => sum + m.bytesDone, 0);
        const bytesTotal = unpacking.reduce((sum, m) => sum + m.bytesTotal, 0);
        let progress = (bytesDone / bytesTotal) * 100;

        if (isNaN(progress)) {
          progress = 0;
        }

        return `Extracting file ${unpacking.length + unpacked.length}/${torrent.downloads.length} (${progress.toFixed(
          2,
        )}%)`;
      }

      const queuedForUnpacking = torrent.downloads.filter((m) => m.unpackingQueued && !m.unpackingStarted);

      if (queuedForUnpacking.length > 0) {
        return `Queued for unpacking`;
      }

      const queuedForDownload = torrent.downloads.filter((m) => !m.downloadStarted && !m.downloadFinished);

      if (queuedForDownload.length > 0) {
        return `Queued for downloading`;
      }

      if (unpacked.length > 0) {
        return `Files unpacked`;
      }

      if (downloaded.length > 0) {
        return `Files downloaded to host`;
      }
    }

    if (torrent.completed) {
      return 'Finished';
    }

    switch (torrent.rdStatus) {
      case RealDebridStatus.Downloading:
        if (torrent.rdSeeders < 1) {
          return `Torrent stalled`;
        }
        const speed = filesize(torrent.rdSpeed);
        return `Torrent downloading (${torrent.rdProgress}% - ${speed}/s)`;
      case RealDebridStatus.Processing:
        return `Torrent processing`;
      case RealDebridStatus.WaitingForFileSelection:
        return `Torrent waiting for file selection`;
      case RealDebridStatus.Error:
        return `Torrent error: ${torrent.rdStatusRaw}`;
      case RealDebridStatus.Finished:
        return `Torrent finished, waiting for download links`;
      case RealDebridStatus.Uploading:
        return `Torrent uploading`;
      default:
        return 'Unknown status';
    }
  }
}

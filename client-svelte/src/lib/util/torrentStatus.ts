import { type Torrent, TorrentStatus } from '$lib/generated/apiClient';
import { formatFileSize } from '$lib/util/filesize';

export function formatTorrentStatus(torrent: Torrent) {
	if (torrent.error) {
		return torrent.error;
	}

	if (torrent.downloads && torrent.downloads.length > 0) {
		const allFinished = torrent.downloads.every((m) => m.completed != null);

		if (allFinished) {
			return 'Finished';
		}

		const downloading = torrent.downloads.filter(
			(m) => m.downloadStarted && !m.downloadFinished && (m.bytesDone ?? 0) > 0
		);
		const downloaded = torrent.downloads.filter((m) => !!m.downloadFinished);

		if (downloading.length > 0) {
			const progress = calculateProgress(downloading);

			const allSpeeds = downloading.reduce((acc, m) => acc + (m.speed ?? 0), 0);

			const speed = formatFileSize(allSpeeds);

			return `Downloading file ${downloading.length + downloaded.length}/${torrent.downloads.length} (${progress.toFixed(2)}% - ${speed}/s)`;
		}

		const unpacking = torrent.downloads.filter(
			(m) => m.unpackingStarted && !m.unpackingFinished && (m.bytesDone ?? 0) > 0
		);
		const unpacked = torrent.downloads.filter((m) => !!m.unpackingFinished);

		if (unpacking.length > 0) {
			const progress = calculateProgress(unpacking);

			return `Extracting file ${unpacking.length + unpacked.length}/${torrent.downloads.length} (${progress.toFixed(2)}%)`;
		}

		if (torrent.downloads.some((m) => !!m.unpackingQueued && !m.unpackingStarted)) {
			return 'Queued for unpacking';
		}

		if (torrent.downloads.some((m) => !m.downloadStarted && !m.downloadFinished)) {
			return 'Queued for downloading';
		}

		if (unpacked.length > 0) {
			return 'Files unpacked';
		}

		if (downloaded.length > 0) {
			return 'Files downloaded to host';
		}
	}

	if (torrent.completed) {
		return 'Finished';
	}

	switch (torrent.rdStatus) {
		case TorrentStatus.Downloading:
			if (torrent.rdSeeders === 0) {
				return 'Torrent stalled';
			}

			return `Torrent downloading (${torrent.rdProgress}% - ${formatFileSize(torrent.rdSpeed ?? 0)}/s)`;

		case TorrentStatus.Processing:
			return 'Torrent processing';
		case TorrentStatus.WaitingForFileSelection:
			return 'Torrent waiting for file selection';
		case TorrentStatus.Error:
			return `Torrent error: ${torrent.rdStatusRaw}`;
		case TorrentStatus.Finished:
			return 'Torrent finished, waiting for download links';
		case TorrentStatus.Uploading:
			return 'Torrent uploading';
		default:
			return 'Unknown status';
	}
}

function calculateProgress(arr: { bytesDone?: number; bytesTotal?: number }[]): number {
	const bytesDone = arr.reduce((acc, m) => acc + (m.bytesDone ?? 0), 0);
	const bytesTotal = arr.reduce((acc, m) => acc + (m.bytesTotal ?? 0), 0);

	const fraction = bytesDone / bytesTotal;
	return (isNaN(fraction) ? 0 : fraction) * 100;
}

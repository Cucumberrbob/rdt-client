import type { Download } from '$lib/generated/apiClient';
import { formatFileSize } from './filesize';

export function formatDownloadStatus(download: Download) {
	if (!download) {
		return 'Pending';
	}

	if (download.error) {
		return download.error;
	}

	if (download.completed != null) {
		return 'Finished';
	}

	if (download.unpackingFinished) {
		return 'Unpacking finished';
	}

	if (download.unpackingStarted) {
		const progress = calculateProgress(download);

		return `Unpacking ${progress.toFixed(2)}%`;
	}

	if (download.unpackingQueued) {
		return 'Unpacking queued';
	}

	if (download.downloadFinished) {
		return 'Download finished';
	}

	if (download.downloadStarted) {
		const progress = calculateProgress(download);

		const speed = download.speed !== undefined ? formatFileSize(download.speed) : 'unknown';

		return `Downloading ${progress.toFixed(2)}% (${speed}/s)`;
	}

	if (download.downloadQueued) {
		return 'Download queued';
	}

	return 'Pending';
}

function calculateProgress({ bytesDone, bytesTotal }: { bytesDone?: number; bytesTotal?: number }) {
	const done = bytesDone ?? 0;
	const total = bytesTotal ?? 0;

	const progress = done / total;
	if (isNaN(progress)) return 0;

	return progress * 100;
}

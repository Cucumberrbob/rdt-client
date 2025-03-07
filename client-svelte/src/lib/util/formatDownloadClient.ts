import { DownloadClient } from '$lib/generated/apiClient';

export function formatDownloadClient(downloadClient: DownloadClient) {
	switch (downloadClient) {
		case DownloadClient.Internal:
			return 'Internal Downloader';
		case DownloadClient.Bezzad:
			return 'Bezzad';
		case DownloadClient.Aria2c:
			return 'Aria2c';
		case DownloadClient.Symlink:
			return 'Symlink Downloader';
		case DownloadClient.DownloadStation:
			return 'Synology DownloadStation';
		default:
			return 'Unknown Downloader';
	}
}

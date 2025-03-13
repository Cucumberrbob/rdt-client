import {
	TorrentDownloadAction,
	TorrentFinishedAction,
	TorrentHostDownloadAction
} from '$lib/generated/apiClient';

export function formatDownloadAction(action: TorrentDownloadAction) {
	switch (action) {
		case TorrentDownloadAction.DownloadAll:
			return 'Download all files above a certain size';
		case TorrentDownloadAction.DownloadAvailableFiles:
			return 'Download all available files on Real-Debrid above a certain size';
		case TorrentDownloadAction.DownloadManual:
			return 'Pick files I want to download';
		default:
			return 'Unknown action';
	}
}

export function formatFinishedAction(action: TorrentFinishedAction) {
	switch (action) {
		case TorrentFinishedAction.None:
			return 'Do nothing';
		case TorrentFinishedAction.RemoveAllTorrents:
			return 'Remove torrent from Real-Debrid and Real-Debrid Client';
		case TorrentFinishedAction.RemoveRealDebrid:
			return 'Remove torrent from Real-Debrid';
		case TorrentFinishedAction.RemoveClient:
			return 'Remove torrent from client';
		default:
			return 'Unknown action';
	}
}

export function formatHostAction(action: TorrentHostDownloadAction) {
	switch (action) {
		case TorrentHostDownloadAction.DownloadAll:
			return 'Download all files to host';
		case TorrentHostDownloadAction.DownloadNone:
			return "Don't download files to host";
		default:
			return 'Unknown action';
	}
}

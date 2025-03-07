import { invalidate } from '$app/navigation';
import { type Torrent, TorrentsClient } from '$lib/generated/apiClient';
import type { LoadEvent } from '@sveltejs/kit';
import { base } from '$app/paths';

class TorrentsCache {
	static invalidationString = 'rdtclient:torrents' as const;

	private torrents: Torrent[] | undefined = undefined;

	public maxAge: number = 60_000;
	private lastFetch: Date | undefined;

	async clearCache({ invalidate: shouldInvalidate } = { invalidate: true }) {
		this.torrents = undefined;
		if (shouldInvalidate) await invalidate(TorrentsCache.invalidationString);
	}

	async getTorrents({ fetch, depends }: Pick<LoadEvent, 'fetch' | 'depends'>) {
		depends(TorrentsCache.invalidationString);

		if (
			this.torrents &&
			this.lastFetch !== undefined &&
			+Date.now() < +this.lastFetch + this.maxAge
		) {
			console.log('using cache');
			return this.torrents;
		}

		console.log('fetching live data');
		this.lastFetch = new Date();
		const torrentsClient = new TorrentsClient(base, { fetch });
		this.torrents = await torrentsClient.getAll();

		return this.torrents;
	}

	async getById(torrentId: string, { fetch, depends }: Pick<LoadEvent, 'fetch' | 'depends'>) {
		depends('rdtclient:torrents');

		if (this.torrents) {
			console.log('using cache');
			return this.torrents.find((t) => t.torrentId === torrentId);
		}

		console.log('fetching live data');
		const torrentsClient = new TorrentsClient(base, { fetch });
		return await torrentsClient.getById(torrentId);
	}
}

export const torrentsCache = new TorrentsCache();

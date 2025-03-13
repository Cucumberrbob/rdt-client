import { invalidate } from '$app/navigation';
import type { LoadEvent } from '@sveltejs/kit';
import { base } from '$app/paths';
import { type SettingProperty, SettingsClient } from '$lib/generated/apiClient';

class SettingsCache {
	static invalidationString = 'rdtclient:settings' as const;
	private settings: SettingProperty[] | undefined = undefined;

	/** Maximum age in ms for the cache to remain non-stale. After this time, the cache will be reset on the next `get` call  */
	public maxAge: number = 60_000;
	private lastFetch: Date | undefined;

	async clearCache({ invalidate: shouldInvalidate } = { invalidate: true }) {
		this.settings = undefined;
		if (shouldInvalidate) await invalidate(SettingsCache.invalidationString);
	}

	async get({ fetch, depends }: Pick<LoadEvent, 'fetch' | 'depends'>) {
		depends(SettingsCache.invalidationString);

		if (
			this.settings &&
			this.lastFetch !== undefined &&
			+new Date() < +this.lastFetch + this.maxAge
		) {
			console.log('[settings] using cache');
			return this.settings;
		}

		console.log('[settings] fetching live data');
		this.lastFetch = new Date();
		const torrentsClient = new SettingsClient(base, { fetch });
		this.settings = await torrentsClient.get();

		return this.settings;
	}
}

export const settingsCache = new SettingsCache();

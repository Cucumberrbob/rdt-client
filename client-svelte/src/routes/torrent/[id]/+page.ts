import type { PageLoad } from './$types';
import { torrentsCache } from '$lib/caches/torrents';
import { settingsCache } from '$lib/caches/settings';

export const load: PageLoad = async ({ params, fetch, depends }) => {
	return {
		torrent: await torrentsCache.getById(params.id, { fetch, depends }),
		settings: await settingsCache.get({ fetch, depends })
	};
};

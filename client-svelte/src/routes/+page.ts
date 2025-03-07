import type { PageLoad } from './$types';
import { torrentsCache } from '$lib/caches/torrents';
import { settingsCache } from '$lib/caches/settings';

export const load: PageLoad = async ({ fetch, depends }) => {
	return {
		torrents: await torrentsCache.getTorrents({ fetch, depends }),
		settings: settingsCache.get({ fetch, depends })
	};
};

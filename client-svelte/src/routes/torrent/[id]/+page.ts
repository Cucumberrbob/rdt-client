import type { PageLoad } from './$types';
import { torrentsCache } from '$lib/caches/torrents';

export const load: PageLoad = async ({ params, fetch, depends }) => {
	return {
		torrent: await torrentsCache.getById(params.id, { fetch, depends })
	};
};

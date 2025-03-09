import type { PageLoad } from './$types';
import { settingsCache } from '$lib/caches/settings';

export const load: PageLoad = async ({ fetch, depends }) => {
	return { settings: await settingsCache.get({ fetch, depends }) };
};

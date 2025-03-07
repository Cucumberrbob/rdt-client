import type { PageLoad } from './$types';
import { SettingProperty } from '$lib/generated/apiClient';
import { settingsCache } from '$lib/caches/settings';

export const load: PageLoad = async ({ fetch, depends }) => {
	const settings = await settingsCache.get({ fetch, depends });

	const tabs = settings.filter(
		(s): s is SettingProperty & { key: string } => !s.key?.includes(':')
	);

	const categorizedSettings = Object.fromEntries(
		tabs.map((tab) => [
			tab.key,
			settings.filter((s) => s.key?.startsWith(`${tab.key}:`) && (s.description || s.displayName))
		])
	);

	return { categorizedSettings, tabs, settings };
};

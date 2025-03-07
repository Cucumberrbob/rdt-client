import type { PageLoad } from './$types';
import { SettingsClient } from '$lib/generated/apiClient';
import { base } from '$app/paths';

export const load: PageLoad = async ({ fetch }) => {
	const settingsClient = new SettingsClient(base, { fetch });

	const settings = await settingsClient.get();

	console.log(settings);

	return { settings };
};

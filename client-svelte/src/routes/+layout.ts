import { base } from '$app/paths';

export const ssr = false;
export const prerender = false;

import type { LayoutLoad } from './$types';
import { redirect } from '@sveltejs/kit';
import { AuthClient, ProblemDetails } from '$lib/generated/apiClient';
import { isLoggedIn, setIsLoggedIn } from '$lib/caches/isLoggedIn';

export const load: LayoutLoad = async ({ route, fetch }) => {
	const result = await loggedIn(fetch);

	if (result.isLoggedIn) {
		if (route.id === '/login' || route.id === '/setup/user') redirect(302, '/');

		return {
			auth: true
		};
	}

	if (result.reason === 'Needs Setup' && route.id !== '/setup/user') redirect(302, '/setup/user');
	if (result.reason === 'Needs Login' && route.id !== '/login') redirect(302, '/login');
};

async function loggedIn(fetch: (typeof window)['fetch']) {
	if (isLoggedIn === true) {
		return { isLoggedIn: true as const };
	}

	const apiClient = new AuthClient(base, { fetch });
	try {
		await apiClient.isLoggedIn();

		// not throwing means success
		setIsLoggedIn(true);

		return {
			isLoggedIn: true as const
		};
	} catch (err) {
		if (err instanceof ProblemDetails) {
			if (err.status === 403) {
				return {
					isLoggedIn: false as const,
					reason: 'Needs Login' as const
				};
			}
			if (err.status === 402) {
				return {
					isLoggedIn: false as const,
					reason: 'Needs Setup' as const
				};
			}
		}
		return { isLoggedIn: false as const };
	}
}

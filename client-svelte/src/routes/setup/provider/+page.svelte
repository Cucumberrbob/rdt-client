<script lang="ts">
	import {
		AuthClient,
		AuthControllerSetupProviderRequest,
		Provider
	} from '$lib/generated/apiClient';
	import { goto } from '$app/navigation';
	import { ProgressRing } from '@skeletonlabs/skeleton-svelte';

	const apiKeyLinks: { [K in Provider]: string } = {
		[Provider.RealDebrid]: 'https://real-debrid.com/apitoken',
		[Provider.AllDebrid]: 'https://alldebrid.com/apikeys',
		[Provider.Premiumize]: 'https://premiumize.me/account',
		[Provider.TorBox]: 'https://torbox.app/settings',
		[Provider.DebridLink]: 'https://debrid-link.com/webapp/apikey'
	};

	let provider = $state<Provider>(Provider.RealDebrid);
	let apiKey = $state('');
	let error = $state('');

	let submitting = $state(false);

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		error = '';

		const client = new AuthClient();
		const request = new AuthControllerSetupProviderRequest({
			provider,
			token: apiKey
		});

		try {
			submitting = true;
			await client.setupProvider(request);
			submitting = false;
			await goto('/');
		} catch (err) {
			submitting = false;
			error = String(err);
		}
	}
</script>

<div class="flex min-h-screen items-center justify-center">
	<div class="card border-surface-300-700 w-full max-w-sm space-y-4 border-[1px] p-6">
		<div class="flex-col space-y-2 text-center">
			<h1 class="text-2xl font-bold">Set up provider</h1>
			<p class="text-sm opacity-50">
				To be able to use the Real-Debrid Client you need a premium subscription with a supported
				debrid provider.
			</p>
		</div>

		<form onsubmit={handleSubmit} class="space-y-4">
			{#if error}
				<div class="card preset-tonal-error border p-4">
					{error}
				</div>
			{/if}

			<label for="provider" class="label">
				<span class="label-text">Provider</span>
				<select name="provider" id="provider" class="select" bind:value={provider}>
					<option value={Provider.RealDebrid}>Real Debrid</option>
					<option value={Provider.AllDebrid}>All Debrid</option>
					<option value={Provider.Premiumize}>Premiumize</option>
					<option value={Provider.TorBox}>TorBox</option>
					<option value={Provider.DebridLink}>Debrid Link</option>
				</select>
			</label>

			<label for="apiKey" class="label">
				<span class="label-text">Api Key</span>
				<input name="apiKey" id="apiKey" type="text" bind:value={apiKey} required class="input" />
				<span class="text-sm opacity-50">
					Find your API key
					<a href={apiKeyLinks[provider]} class="anchor" target="_blank" rel="noopener">here</a>
				</span>
			</label>

			<button
				type="submit"
				class="button preset-filled-primary-500 flex w-full justify-center p-4"
				disabled={submitting}
			>
				{#if submitting}
					<ProgressRing value={null} size="size-4" strokeWidth="0.25rem" />
				{:else}
					<span>Sign Up</span>
				{/if}
			</button>
		</form>
	</div>
</div>

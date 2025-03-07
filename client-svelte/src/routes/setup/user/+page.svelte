<script lang="ts">
	import { AuthClient, AuthControllerLoginRequest, ProblemDetails } from '$lib/generated/apiClient';
	import { goto } from '$app/navigation';
	import { ProgressRing } from '@skeletonlabs/skeleton-svelte';

	let username = $state('');
	let password = $state('');
	let error = $state('');

	let submitting = $state(false);

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		error = '';

		const client = new AuthClient();
		const request = new AuthControllerLoginRequest({
			userName: username,
			password: password
		});

		try {
			submitting = true;
			await client.create(request);
			await goto('/setup/provider');
		} catch (err) {
			submitting = false;

			if (err instanceof ProblemDetails && err.detail) {
				error = err.detail;
			} else {
				error = 'An unknown error occurred';
			}
		}
	}
</script>

<div class="flex min-h-screen items-center justify-center">
	<div class="card border-surface-300-700 w-full max-w-sm space-y-4 border-[1px] p-6">
		<div class="flex-col space-y-2 text-center">
			<h1 class="text-2xl font-bold">Sign Up</h1>
			<p class="text-sm opacity-50">
				Welcome to Real-Debrid Client. Please create your account by entering a username and
				password.
			</p>
		</div>

		<form onsubmit={handleSubmit} class="space-y-4">
			{#if error}
				<div class="card preset-tonal-error border p-4">
					{error}
				</div>
			{/if}

			<label for="username" class="label">
				<span class="label-text">Username</span>
				<input id="username" type="text" bind:value={username} required class="input" />
			</label>

			<label for="password" class="label">
				<span class="label-text">Password</span>
				<input
					id="password"
					type="password"
					autocomplete="new-password"
					bind:value={password}
					required
					class="input"
				/>
			</label>

			<button type="submit" class="button preset-filled-primary-500 flex w-full justify-center p-4">
				{#if submitting}
					<ProgressRing value={null} size="size-4" strokeWidth="0.25rem" />
				{:else}
					<span>Sign Up</span>
				{/if}
			</button>
		</form>
	</div>
</div>

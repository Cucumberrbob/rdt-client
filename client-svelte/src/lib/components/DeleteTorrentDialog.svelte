<script lang="ts">
	import type { SvelteSet } from 'svelte/reactivity';
	import { TorrentControllerDeleteRequest, TorrentsClient } from '$lib/generated/apiClient';
	import { base } from '$app/paths';
	import { ProgressRing } from '@skeletonlabs/skeleton-svelte';
	import { invalidate } from '$app/navigation';

	let {
		torrentIdsToDelete,
		deleteDialogElement = $bindable()
	}: {
		torrentIdsToDelete: SvelteSet<string>;
		deleteDialogElement: HTMLDialogElement | undefined;
	} = $props();

	let isDeleting = $state(false);
	let error = $state('');

	let deleteClient = $state(false);
	let deleteProvider = $state(false);
	let deleteLocal = $state(false);

	async function deleteTorrents(event: SubmitEvent) {
		event.preventDefault();

		isDeleting = true;
		error = '';

		const torrentsClient = new TorrentsClient(base);
		const deleteRequest = new TorrentControllerDeleteRequest({
			deleteData: deleteClient,
			deleteRdTorrent: deleteProvider,
			deleteLocalFiles: deleteLocal
		});

		const results = await Promise.allSettled(
			[...torrentIdsToDelete].map(async (id) => ({
				id,
				res: await torrentsClient.delete(id, deleteRequest)
			}))
		);

		isDeleting = false;

		const rejected = results.filter((r) => r.status === 'rejected');
		const fulfilled = results.filter((r) => r.status === 'fulfilled');

		if (rejected.length > 0) {
			console.log(rejected);

			if (torrentIdsToDelete.size === 1) error = 'An unknown error occurred';
			else
				error = `An unknown error occurred deleting ${rejected.length}/${torrentIdsToDelete.size} torrents`;

			throw rejected.at(0)?.reason;
		}

		deleteDialogElement?.close();

		await Promise.all([
			await invalidate(`${base}/Api/Torrents`),
			...fulfilled.map((r) => invalidate(`${base}/Api/Torrents/Get/${r.value.id}`))
		]);
	}
</script>

<dialog
	bind:this={deleteDialogElement}
	class="card preset-filled-surface-100-900 absolute top-[50%] left-[50%] -translate-x-1/2 -translate-y-1/2 space-y-4 p-4"
>
	<h4 class="h4">
		{#if torrentIdsToDelete.size === 1}
			Delete Torrent
		{:else}
			Delete Torrents
		{/if}
	</h4>

	{#if error}
		<div class="card preset-filled-error-500">
			<span>{error}</span>
		</div>
	{/if}

	<form class="flex min-w-md flex-col space-y-4" onsubmit={deleteTorrents}>
		<label class="label flex flex-row items-center gap-2">
			<input type="checkbox" class="checkbox" tabindex="0" bind:checked={deleteClient} />
			<span class="label-text">Delete from client</span>
		</label>
		<label class="label flex flex-row items-center gap-2">
			<input type="checkbox" class="checkbox" tabindex="0" bind:checked={deleteProvider} />
			<span class="label-text">Delete from provider</span>
		</label>
		<label class="label flex flex-row items-center gap-2">
			<input type="checkbox" class="checkbox" tabindex="0" bind:checked={deleteLocal} />
			<span class="label-text">Delete local files</span>
		</label>

		<hr class="hr" />

		<div class="flex justify-end space-x-4">
			<button
				class="button preset-outlined-surface-500 rounded-base px-4 py-2"
				tabindex="0"
				type="button"
				onclick={() => deleteDialogElement?.close()}
			>
				Cancel
			</button>
			<button
				type="submit"
				class="button preset-filled-success-300-700 rounded-base px-4 py-2 transition-opacity"
				tabindex="0"
				disabled={!deleteClient && !deleteProvider && !deleteLocal}
			>
				{#if isDeleting}
					<ProgressRing value={null} size="size-8" meterStroke="stroke-surface-500" />
				{:else}
					Delete
				{/if}
			</button>
		</div>
	</form>
</dialog>

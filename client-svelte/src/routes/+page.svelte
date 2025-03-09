<script lang="ts">
	import { formatFileSize } from '$lib/util/filesize';
	import { formatTorrentStatus } from '$lib/util/torrentStatus';
	import type { PageProps } from './$types';
	import { type Torrent, TorrentsClient } from '$lib/generated/apiClient';
	import { ArrowDown, Pencil, RotateCcw, X } from 'lucide-svelte';
	import { SvelteSet } from 'svelte/reactivity';
	import { fade } from 'svelte/transition';
	import DeleteTorrentDialog from '$lib/components/DeleteTorrentDialog.svelte';
	import { onDestroy, onMount } from 'svelte';
	import { invalidate } from '$app/navigation';
	import { torrentsCache } from '$lib/caches/torrents';
	import { base } from '$app/paths';
	import EditTorrentDialog from '$lib/components/EditTorrentDialog.svelte';

	let { data }: PageProps = $props();
	let torrents = $derived(structuredClone(data.torrents));

	type SortingMethod = {
		field: Extract<
			'rdName' | 'category' | 'files' | 'rdSize' | 'added' | 'rdStatus',
			keyof Torrent
		>;
		direction: 'asc' | 'desc';
	};

	let sortingMethod = $state<SortingMethod>({ field: 'rdName', direction: 'desc' });
	const sorted = $derived(
		torrents.toSorted((a, b) => {
			const { field, direction } = sortingMethod;

			const aVal = a[field];
			const bVal = b[field];

			if (Array.isArray(aVal) && Array.isArray(bVal)) {
				return direction === 'asc' ? bVal.length - aVal.length : aVal.length - bVal.length;
			}

			if (
				(typeof aVal === 'number' && typeof bVal === 'number') ||
				(aVal instanceof Date && bVal instanceof Date)
			) {
				return direction === 'asc' ? +bVal - +aVal : +aVal - +bVal;
			}

			if (typeof aVal === 'string' && typeof bVal === 'string') {
				return direction === 'asc' ? bVal.localeCompare(aVal) : aVal.localeCompare(bVal);
			}

			return 0;
		})
	);

	function switchSortingMethod(field: SortingMethod['field']) {
		if (sortingMethod.field === field) {
			sortingMethod.direction = sortingMethod.direction === 'asc' ? 'desc' : 'asc';
			return;
		}

		sortingMethod = { field, direction: 'desc' };
	}

	let selected = new SvelteSet<string | undefined>();

	let torrentIdsToDelete = new SvelteSet<string>();
	let deleteDialogElement: HTMLDialogElement | undefined = $state();

	let torrentsToEdit = $state<Torrent[]>([]);
	let editDialogElement: HTMLDialogElement | undefined = $state();

	const columns: [field: SortingMethod['field'], displayName: string][] = [
		['rdName', 'Name'],
		['category', 'Category'],
		['files', 'Files'],
		['rdSize', 'Size'],
		['added', 'Added'],
		['rdStatus', 'Status']
	];

	function openDeleteTorrentModalSingle(torrentId?: string) {
		if (!torrentId) return;

		torrentIdsToDelete.clear();
		torrentIdsToDelete.add(torrentId);

		deleteDialogElement?.showModal();
	}

	function openDeleteTorrentModalMulti(torrentIds: SvelteSet<string | undefined>) {
		torrentIdsToDelete.clear();
		torrentIds.forEach((id) => id !== undefined && torrentIdsToDelete.add(id));

		deleteDialogElement?.showModal();
	}

	function openEditTorrentModalSingle(torrent: Torrent) {
		torrentsToEdit = [torrent];

		editDialogElement?.showModal();
	}
	function openEditTorrentModalMulti(torrents: Torrent[]) {
		torrentsToEdit = torrents;

		editDialogElement?.showModal();
	}

	async function retrySingle(torrentId: string) {
		return retryMultiple([torrentId]);
	}

	async function retryMultiple(torrentIds: string[]) {
		const torrentsClient = new TorrentsClient(base);

		const results = await Promise.allSettled(torrentIds.map((id) => torrentsClient.retry(id)));

		if (results.every((r) => r.status === 'fulfilled')) {
			await torrentsCache.clearCache();
			return;
		}

		console.error(results);
	}

	let interval: ReturnType<typeof setInterval> | undefined = undefined;
	onMount(() => {
		interval = setInterval(() => {
			console.log('refreshing data');
			torrentsCache.clearCache();
		}, 10_000);
	});

	onDestroy(() => {
		clearInterval(interval);
	});
</script>

<table class="table w-full">
	<thead>
		<tr>
			<td>
				<input
					type="checkbox"
					class="checkbox"
					checked={selected.size === sorted.length && sorted.length !== 0}
					disabled={sorted.length === 0}
					indeterminate={0 < selected.size && selected.size < sorted.length}
					onchange={({ currentTarget }) =>
						currentTarget.checked
							? sorted.forEach(({ torrentId }) => selected.add(torrentId))
							: selected.clear()}
				/>
			</td>
			{#each columns as [key, displayName]}
				<td onclick={() => switchSortingMethod(key)}>
					<div class="group flex">
						<span>{displayName}</span>
						<ArrowDown
							class={`transition-[opacity,rotate] ${sortingMethod.direction === 'asc' && sortingMethod.field === key ? 'rotate-180' : ''} ${sortingMethod.field === key ? 'opacity-100' : 'opacity-0 group-hover:opacity-50'}`}
						/>
					</div>
				</td>
			{/each}
			<td
				class="flex items-center justify-around {selected.size >= 2
					? 'opacity-100'
					: 'opacity-0'} transition-opacity"
			>
				<button class="button text-error-500" onclick={() => openDeleteTorrentModalMulti(selected)}>
					<X size={16} />
				</button>

				<button
					class="button text-tertiary-500"
					onclick={() =>
						openEditTorrentModalMulti(
							[...selected]
								.filter((id) => id !== undefined)
								.map((id) => torrents.find((t) => t.torrentId === id))
								.filter((t) => t !== undefined)
						)}
				>
					<Pencil size={16} />
				</button>

				<button
					class="button text-primary-500"
					onclick={() => retryMultiple([...selected].filter((id) => id !== undefined))}
				>
					<RotateCcw size={16} />
				</button>
			</td>
		</tr>
	</thead>
	<tbody>
		{#each sorted as torrent (torrent.torrentId)}
			<tr transition:fade>
				<td>
					<input
						type="checkbox"
						class="checkbox"
						checked={selected.has(torrent.torrentId)}
						onchange={({ currentTarget }) =>
							currentTarget.checked
								? selected.add(torrent.torrentId)
								: selected.delete(torrent.torrentId)}
					/>
				</td>
				<td>
					<a href={`/torrent/${torrent.torrentId}`}>
						{torrent.rdName}
					</a>
				</td>

				<td><span>{torrent.category}</span></td>

				<td><span>{torrent.files?.length ?? 0}</span></td>

				<td><span>{formatFileSize(torrent.rdSize ?? 0)}</span></td>

				<td>
					<span>
						{torrent.added?.toLocaleString(undefined, {
							dateStyle: 'short',
							timeStyle: 'short'
						})}
					</span>
				</td>

				<td>
					<span>{formatTorrentStatus(torrent)}</span>
				</td>

				<td class="flex items-center justify-around">
					<button
						class="button text-error-500"
						onclick={() => openDeleteTorrentModalSingle(torrent.torrentId)}
					>
						<X size={16} />
					</button>

					<button
						class="button text-tertiary-500"
						onclick={() => openEditTorrentModalSingle(torrent)}
					>
						<Pencil size={16} />
					</button>

					<button
						class="button text-primary-500"
						title="Retry"
						onclick={() => retrySingle(torrent.torrentId as string)}
					>
						<RotateCcw size={16} />
					</button>
				</td>
			</tr>
		{/each}
	</tbody>
</table>

<DeleteTorrentDialog bind:deleteDialogElement {torrentIdsToDelete} />
{#await data.settings}
	<!--  -->
{:then settings}
	<EditTorrentDialog {settings} bind:editDialogElement {torrentsToEdit} />
{/await}

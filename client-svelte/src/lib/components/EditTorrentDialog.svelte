<script lang="ts">
	import { SettingProperty, Torrent, TorrentsClient } from '$lib/generated/apiClient';
	import { base } from '$app/paths';
	import { ProgressRing } from '@skeletonlabs/skeleton-svelte';
	import Setting from './Setting.svelte';
	import { torrentsCache } from '$lib/caches/torrents';

	let {
		torrentsToEdit,
		editDialogElement = $bindable(),
		settings: _settings
	}: {
		torrentsToEdit: Torrent[];
		editDialogElement: HTMLDialogElement | undefined;
		settings: SettingProperty[];
	} = $props();

	let isEditing = $state(false);
	let error = $state('');

	const hasMultipleValuesSymbol = Symbol.for('hasMultipleValues');
	function getCommonSetting<TKey extends keyof Torrent>(
		editedTorrents: Torrent[],
		key: TKey
	): Torrent[TKey] | typeof hasMultipleValuesSymbol {
		const first = editedTorrents[0]?.[key];
		return editedTorrents.every((t) => t[key] === first) ? first : hasMultipleValuesSymbol;
	}

	const settingsToShow = [
		'downloadClient',
		'hostDownloadAction',
		'category',
		'priority',
		'downloadRetryAttempts',
		'torrentRetryAttempts',
		'deleteOnError',
		'lifetime'
	] satisfies (keyof Torrent)[];

	/** Used to store definitions *only*. The values should be taken from the *torrent* **NOT** this */
	const settingsDefinitions: {
		[K in (typeof settingsToShow)[number]]: SettingProperty | undefined;
	} = {
		downloadClient: _settings.find((s) => s.key === 'DownloadClient:Client'),
		hostDownloadAction: _settings.find((s) => s.key === 'Provider:Default:FinishedAction'),
		category: _settings.find((s) => s.key === 'Provider:Default:Category'),
		priority: _settings.find((s) => s.key === 'Provider:Default:Priority'),
		downloadRetryAttempts: _settings.find(
			(s) => s.key === 'Provider:Default:DownloadRetryAttempts'
		),
		torrentRetryAttempts: _settings.find((s) => s.key === 'Provider:Default:TorrentRetryAttempts'),
		deleteOnError: _settings.find((s) => s.key === 'Provider:Default:DeleteOnError'),
		lifetime: _settings.find((s) => s.key === 'Provider:Default:TorrentLifetime')
	};

	let torrentSettings = $state<{
		[K in (typeof settingsToShow)[number]]: ReturnType<typeof getCommonSetting<K>>;
	}>();
	$effect(() => {
		torrentSettings = {
			downloadClient: getCommonSetting(torrentsToEdit, 'downloadClient'),
			hostDownloadAction: getCommonSetting(torrentsToEdit, 'hostDownloadAction'),
			category: getCommonSetting(torrentsToEdit, 'category'),
			priority: getCommonSetting(torrentsToEdit, 'priority'),
			downloadRetryAttempts: getCommonSetting(torrentsToEdit, 'downloadRetryAttempts'),
			torrentRetryAttempts: getCommonSetting(torrentsToEdit, 'torrentRetryAttempts'),
			deleteOnError: getCommonSetting(torrentsToEdit, 'deleteOnError'),
			lifetime: getCommonSetting(torrentsToEdit, 'lifetime')
		};
	});

	async function editTorrents(event: SubmitEvent) {
		event.preventDefault();
		if (!torrentSettings)
			throw Error('Please report this bug! (EditTorrentDialog#editTorrents - no torrentSettings)');

		isEditing = true;
		error = '';

		const torrentsClient = new TorrentsClient(base);
		const results = await Promise.allSettled(
			[...torrentsToEdit].map(async (torrent) => {
				if (!torrentSettings)
					throw Error(
						'Please report this bug! (EditTorrentDialog#editTorrents - no torrentSettings)'
					);
				return {
					id: torrent,
					res: await torrentsClient.update(
						new Torrent({
							torrentId: torrent.torrentId,
							downloadClient:
								typeof torrentSettings.downloadClient !== 'symbol'
									? torrentSettings.downloadClient
									: torrent.downloadClient,
							hostDownloadAction:
								typeof torrentSettings.hostDownloadAction !== 'symbol'
									? torrentSettings.hostDownloadAction
									: torrent.hostDownloadAction,
							category:
								typeof torrentSettings.category !== 'symbol'
									? torrentSettings.category
									: torrent.category,
							priority:
								typeof torrentSettings.priority !== 'symbol'
									? torrentSettings.priority
									: torrent.priority,
							downloadRetryAttempts:
								typeof torrentSettings.downloadRetryAttempts !== 'symbol'
									? torrentSettings.downloadRetryAttempts
									: torrent.downloadRetryAttempts,
							torrentRetryAttempts:
								typeof torrentSettings.torrentRetryAttempts !== 'symbol'
									? torrentSettings.torrentRetryAttempts
									: torrent.torrentRetryAttempts,
							deleteOnError:
								typeof torrentSettings.deleteOnError !== 'symbol'
									? torrentSettings.deleteOnError
									: torrent.deleteOnError,
							lifetime:
								typeof torrentSettings.lifetime !== 'symbol'
									? torrentSettings.lifetime
									: torrent.lifetime
						})
					)
				};
			})
		);

		isEditing = false;

		const rejected = results.filter((r) => r.status === 'rejected');
		if (rejected.length > 0) {
			console.log(rejected);

			if (torrentsToEdit.length === 1) error = 'An unknown error occurred';
			else
				error = `An unknown error occurred deleting ${rejected.length}/${torrentsToEdit.length} torrents`;

			throw rejected.at(0)?.reason;
		}

		editDialogElement?.close();

		await torrentsCache.clearCache();
	}

	$inspect(torrentsToEdit);
	$inspect(torrentSettings);
</script>

<dialog
	bind:this={editDialogElement}
	class="card preset-filled-surface-100-900 absolute top-[50%] left-[50%] -translate-x-1/2 -translate-y-1/2 space-y-4 p-4"
>
	<h4 class="h4">
		{#if torrentsToEdit.length === 1}
			Edit Torrent
		{:else}
			Edit Torrents
		{/if}
	</h4>

	{#if error}
		<div class="card preset-filled-error-500 p-4">
			<span>{error}</span>
		</div>
	{/if}

	<form class="flex min-w-md flex-col space-y-4" onsubmit={editTorrents}>
		{#each settingsToShow as key}
			{@const settingDefinition = settingsDefinitions[key]}
			{#if settingDefinition === undefined}
				Something went wrong
			{:else if torrentSettings !== undefined}
				<Setting setting={settingDefinition} bind:value={torrentSettings[key]} />
			{/if}
		{/each}

		<hr class="hr" />

		<div class="flex justify-end space-x-4">
			<button
				class="button preset-outlined-surface-500 rounded-base px-4 py-2"
				tabindex="0"
				type="button"
				onclick={() => editDialogElement?.close()}
			>
				Cancel
			</button>
			<button
				type="submit"
				class="button preset-filled-success-300-700 rounded-base px-4 py-2 transition-opacity"
				tabindex="0"
			>
				{#if isEditing}
					<ProgressRing value={null} size="size-8" meterStroke="stroke-surface-500" />
				{:else}
					Save
				{/if}
			</button>
		</div>
	</form>
</dialog>

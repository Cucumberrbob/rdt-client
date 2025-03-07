<script lang="ts">
	import { FileUpload } from '@skeletonlabs/skeleton-svelte';
	import type { PageData, PageProps } from './$types';
	import Setting from '$lib/components/Setting.svelte';
	import { base } from '$app/paths';
	import { goto } from '$app/navigation';
	import {
		type FileParameter,
		Torrent,
		TorrentControllerUploadFileRequest,
		TorrentControllerUploadMagnetRequest,
		TorrentsClient
	} from '$lib/generated/apiClient';
	import { torrentsCache } from '$lib/caches/torrents';

	let { data }: PageProps = $props();

	function byKey(data: PageData, key: string) {
		const foundSetting = data.settings.find((e) => e.key === key);
		if (!foundSetting) throw new Error(`Setting ${key} not found`);
		return structuredClone(foundSetting);
	}

	const settings = $state({
		downloadClient: byKey(data, 'DownloadClient:Client'),
		hostDownloadAction: byKey(data, 'Gui:Default:HostDownloadAction'),
		minFileSize: byKey(data, 'Gui:Default:MinFileSize'),
		includeRegex: byKey(data, 'Gui:Default:IncludeRegex'),
		excludeRegex: byKey(data, 'Gui:Default:ExcludeRegex'),
		finishedAction: byKey(data, 'Gui:Default:FinishedAction'),
		category: byKey(data, 'Gui:Default:Category'),
		priority: byKey(data, 'Gui:Default:Priority'),
		downloadRetryAttempts: byKey(data, 'Gui:Default:DownloadRetryAttempts'),
		deleteOnError: byKey(data, 'Gui:Default:DeleteOnError'),
		lifetime: byKey(data, 'Gui:Default:TorrentLifetime')
	});

	const settingsToShow: (keyof typeof settings)[] = [
		'downloadClient',
		'hostDownloadAction',
		'minFileSize',
		'includeRegex',
		'excludeRegex',
		'finishedAction',
		'category',
		'priority',
		'downloadRetryAttempts',
		'deleteOnError',
		'lifetime'
	];

	let magnetLink = $state<string | undefined>();
	let torrentFile = $state<FileParameter | undefined>();

	async function handleSubmit(ev: SubmitEvent) {
		ev.preventDefault();
		const torrentsClient = new TorrentsClient(base);

		const torrent = new Torrent({
			downloadClient: settings.downloadClient.value,
			hostDownloadAction: settings.hostDownloadAction.value,
			downloadMinSize: settings.minFileSize.value,
			includeRegex: settings.includeRegex.value,
			excludeRegex: settings.excludeRegex.value,
			finishedAction: settings.finishedAction.value,
			category: settings.category.value,
			priority: settings.priority.value,
			downloadRetryAttempts: settings.downloadRetryAttempts.value,
			deleteOnError: settings.deleteOnError.value,
			lifetime: settings.lifetime.value
		});

		try {
			if (torrentFile) {
				await torrentsClient.uploadFile(
					JSON.stringify(
						new TorrentControllerUploadFileRequest({
							torrent: torrent
						})
					) as unknown as TorrentControllerUploadFileRequest,
					torrentFile
				);
			} else if (magnetLink) {
				await torrentsClient.uploadMagnet(
					new TorrentControllerUploadMagnetRequest({
						magnetLink,
						torrent: torrent
					})
				);
			}

			await torrentsCache.clearCache();

			await goto(`${base}/`);
		} catch (err) {
			console.log(err);
		}
	}
</script>

<form class="flex flex-col space-y-4" onsubmit={handleSubmit}>
	<h2 class="h2">Add Torrent</h2>

	<label class="label" for="torrentFile">
		<span class="label-text">Torrent File</span>
		<FileUpload
			maxFiles={1}
			onFileAccept={async (ev) => {
				const file = ev.files.at(0);
				if (!file) return;
				torrentFile = { data: file, fileName: file.name };
			}}
		/>
	</label>

	<span>Or</span>

	<label class="label" for="magnetLink">
		<span class="label-text">Magnet Link</span>
		<textarea class="textarea" rows="4" bind:value={magnetLink}></textarea>
	</label>

	<hr class="hr" />

	{#each settingsToShow as key}
		<Setting setting={settings[key]} bind:value={settings[key].value} />
	{/each}

	<button
		type="submit"
		disabled={!torrentFile && !magnetLink}
		class="button preset-outlined-success-500">Add Torrent</button
	>
</form>

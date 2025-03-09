<script lang="ts">
	import type { PageProps } from './$types';
	import { Tabs } from '@skeletonlabs/skeleton-svelte';
	import Info from './Info.svelte';
	import { formatDownloadClient } from '$lib/util/formatDownloadClient.js';
	import { formatProvider } from '$lib/util/formatProvider';
	import { formatTorrentStatus } from '$lib/util/torrentStatus.js';
	import {
		formatDownloadAction,
		formatFinishedAction,
		formatHostAction
	} from '$lib/util/formatAction';
	import { formatFileSize } from '$lib/util/filesize.js';
	import { onDestroy, onMount } from 'svelte';
	import { torrentsCache } from '$lib/caches/torrents';

	let { data }: PageProps = $props();

	let selectedTab = $state('General');

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

<div class="card space-y-4">
	{#if data.torrent === undefined || data.torrent === null}
		Could not find torrent
	{:else}
		<h4 class="h4">
			{data.torrent.rdName}
		</h4>

		<Tabs value={selectedTab} onValueChange={({value}) => (selectedTab = value)}>
			{#snippet list()}
				<Tabs.Control value="General">General</Tabs.Control>
				<Tabs.Control value="Files">Files</Tabs.Control>
				<Tabs.Control value="Downloads">Downloads</Tabs.Control>
			{/snippet}
			{#snippet content()}
				{@const provider = data.torrent?.clientKind ? formatProvider(data.torrent.clientKind) : ''}
				<Tabs.Panel value="General" classes="flex flex-row flex-wrap gap-4">
					<div class="card preset-outlined-surface-500 grow flex-col space-y-2 p-4">
						<h6 class="h6">Status</h6>

						<Info
							title="{provider} Status"
							value="{formatTorrentStatus(data.torrent as Torrent)} ({data.torrent?.rdStatusRaw})"
						/>
						<Info
							title="Retries"
							value="{data.torrent?.retryCount} / {data.torrent?.downloadRetryAttempts}"
						/>

						<Info title="{provider} Progress" value="{data.torrent?.rdProgress?.toString()}%" />
						<Info title="{provider} Speed" value={data.torrent?.rdSpeed?.toString()} />
						<Info title="{provider} Seeders" value={data.torrent?.rdSeeders?.toString()} />
					</div>
					<div class="card preset-outlined-surface-500 grow space-y-2 p-4">
						<h6 class="h6">Times</h6>

						<Info
							title="Added"
							value={data.torrent?.added?.toLocaleString(undefined, {
								dateStyle: 'long',
								timeStyle: 'medium'
							})}
						/>
						<Info
							title="Files Selected On"
							value={data.torrent?.filesSelected?.toLocaleString(undefined, {
								dateStyle: 'long',
								timeStyle: 'medium'
							})}
						/>
						<Info
							title="Completed On"
							value={data.torrent?.completed?.toLocaleString(undefined, {
								dateStyle: 'long',
								timeStyle: 'medium'
							})}
						/>
						<Info
							title="{provider} Added"
							value={data.torrent?.rdAdded?.toLocaleString(undefined, {
								dateStyle: 'long',
								timeStyle: 'medium'
							})}
						/>
						<Info
							title="{provider} Ended"
							value={data.torrent?.rdEnded?.toLocaleString(undefined, {
								dateStyle: 'long',
								timeStyle: 'medium'
							})}
						/>
					</div>

					<div class="card preset-outlined-surface-500 grow flex-col space-y-2 p-4">
						<h6 class="h6">Info</h6>

						<Info title="Hash">
							{#snippet content()}
								<pre class="text-lg">{data.torrent?.hash}</pre>
							{/snippet}
						</Info>
						<Info title="Category" value={data.torrent?.category} />
						<Info title="Priority" value={data.torrent?.priority?.toString()} />
						<Info
							title="Downloader"
							value={data.torrent?.downloadClient
								? formatDownloadClient(data.torrent.downloadClient)
								: undefined}
						/>
						<Info title="{provider} ID" value={data.torrent?.rdId} />
					</div>

					<div class="card preset-outlined-surface-500 grow flex-col space-y-2 p-4">
						<h6 class="h6">Filtering</h6>

						<Info title="Include Regex" value={data.torrent?.includeRegex} />
						<Info title="Exclude Regex" value={data.torrent?.excludeRegex} />
						<Info
							title="Download Min Size"
							value={formatFileSize(data.torrent?.downloadMinSize ?? 0)}
						/>
					</div>

					<div class="card preset-outlined-surface-500 grow flex-col space-y-2 p-4">
						<h6 class="h6">Settings</h6>

						<Info
							title="Post Torrent Download Action"
							value={data.torrent?.downloadAction !== undefined
								? formatDownloadAction(data.torrent.downloadAction)
								: undefined}
						/>
						<Info
							title="Finished Action"
							value={data.torrent?.finishedAction !== undefined
								? formatFinishedAction(data.torrent.finishedAction)
								: undefined}
						/>
						<Info
							title="Post Download Action"
							value={data.torrent?.hostDownloadAction !== undefined
								? formatHostAction(data.torrent.hostDownloadAction)
								: undefined}
						/>
					</div>
				</Tabs.Panel>
				<Tabs.Panel value="Files">Files</Tabs.Panel>
				<Tabs.Panel value="Downloads"
					>Downloads
					{#each data.torrent?.downloads ?? [] as download}
						{download.fileName}
					{/each}
				</Tabs.Panel>
			{/snippet}
		</Tabs>
	{/if}
</div>

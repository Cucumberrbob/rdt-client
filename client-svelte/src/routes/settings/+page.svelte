<script lang="ts">
	import type { PageProps } from './$types';
	import { Tabs, ProgressRing } from '@skeletonlabs/skeleton-svelte';
	import { SettingsClient } from '$lib/generated/apiClient';
	import Setting from '$lib/components/Setting.svelte';
	import { settingsCache } from '$lib/caches/settings';

	let { data }: PageProps = $props();

	let selectedTab = $state(data.tabs[0].key);

	let categorizedSettings = $state(structuredClone(data.categorizedSettings));

	let saving = $state(false);
	let settingsClient: SettingsClient;
	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();

		saving = true;
		settingsClient ??= new SettingsClient();

		try {
			await settingsClient.update([...data.tabs, ...Object.values(categorizedSettings).flat()]);
			saving = false;
			await settingsCache.clearCache();
		} catch {
			saving = false;
		}
	}
</script>

<Tabs value={selectedTab} onValueChange={({value}) => (selectedTab = value)}>
	{#snippet list()}
		{#each data.tabs as { key, displayName }}
			<Tabs.Control value={key}>
				{displayName}
			</Tabs.Control>
		{/each}
	{/snippet}
	{#snippet content()}
		{#each data.tabs as { key, description }}
			{@const settings = categorizedSettings[key] ?? []}

			<Tabs.Panel value={key}>
				<form class="flex flex-col space-y-4" onsubmit={handleSubmit}>
					{#if description}
						<span class="text-surface-500 text-sm">
							{description}
						</span>
					{/if}
					{#each settings as setting}
						<Setting {setting} bind:value={setting.value} />
					{/each}
					<button
						class="button preset-outlined-success-500 rounded-base float-right ml-auto p-4"
						type="submit"
						disabled={saving}
					>
						{#if saving}
							<ProgressRing value={null} size="size-8" meterStroke="stroke-surface-500" />
						{:else}
							Save
						{/if}
					</button>
				</form>
			</Tabs.Panel>
		{/each}
	{/snippet}
</Tabs>

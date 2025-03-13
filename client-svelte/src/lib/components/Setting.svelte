<script lang="ts">
	import { SettingProperty } from '$lib/generated/apiClient';

	const hasMultipleValuesSymbol = Symbol.for('hasMultipleValues');

	let { setting, value = $bindable() } = $props<{
		setting: SettingProperty;
		value: SettingProperty['value'];
	}>();
</script>

<div>
	<label
		class={['label flex', setting.type === 'Boolean' ? 'flex-row items-center gap-2' : 'flex-col']}
	>
		<span
			class={[
				'label-text',
				setting.type === 'Object' ? 'h4' : 'text-sm',
				setting.type === 'Boolean' && 'order-2'
			]}>{setting.displayName}</span
		>
		{#if setting.type === 'Enum'}
			<select
				class="select"
				value={value === hasMultipleValuesSymbol ? null : value?.toString()}
				onchange={({ currentTarget }) => {
					value = parseInt(currentTarget.value);
				}}
			>
				{#if value === hasMultipleValuesSymbol}
					<option value={null}>---</option>
					<hr />
				{/if}

				{#each Object.entries(setting.enumValues ?? {}) as [enumVal, name]}
					<option value={enumVal}>{name}</option>
				{/each}
			</select>
		{:else if setting.type === 'String'}
			{#if value === hasMultipleValuesSymbol}
				<input
					class="input"
					type="text"
					placeholder="Multiple Values"
					oninput={({ currentTarget }) => (value = currentTarget.value)}
				/>
			{:else}
				<input class="input" type="text" bind:value />
			{/if}
		{:else if setting.type === 'Int32' || setting.type === 'Int64'}
			{#if value === hasMultipleValuesSymbol}
				<input
					class="input"
					type="text"
					placeholder="Multiple Values"
					oninput={({ currentTarget }) => (value = currentTarget.value)}
				/>
			{:else}
				<input class="input" type="number" bind:value />
			{/if}
		{:else if setting.type === 'Boolean'}
			{#if value === hasMultipleValuesSymbol}
				<input
					class="checkbox"
					type="checkbox"
					indeterminate={true}
					oninput={({ currentTarget }) => (value = currentTarget.checked)}
				/>
			{:else}
				<input class="checkbox" type="checkbox" bind:checked={value} />
			{/if}
		{/if}
	</label>

	<p class="text-surface-500 text-xs">
		<!-- eslint-disable-next-line svelte/no-at-html-tags -->
		{@html setting.description}
	</p>
</div>

<style lang="postcss">
	@reference "../../app.css";

	p :global(a) {
		@apply !anchor;
		@apply opacity-100;
	}
</style>

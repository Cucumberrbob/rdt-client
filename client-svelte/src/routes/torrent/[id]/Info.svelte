<script lang="ts">
	import type { Snippet } from 'svelte';

	let { title, value, content } = $props<
		| {
				title: string;
				value: string | undefined;
		  }
		| { title: string; content?: Snippet }
	>();
</script>

<div>
	<span class="text-md">{title}</span>
	{#if content}
		{@render content()}
	{:else if value}
		<p class="ml-2 text-lg">
			{value}
		</p>
	{:else}
		<pre class="ml-2 text-lg">Not Set</pre>
	{/if}
</div>

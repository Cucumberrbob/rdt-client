<script lang="ts">
	import { AppBar } from '@skeletonlabs/skeleton-svelte';

	import '../app.css';
	import { CircleUser } from 'lucide-svelte';
	import { AuthClient } from '$lib/generated/apiClient';
	import { base } from '$app/paths';
	import { goto } from '$app/navigation';
	import { version } from '$app/environment';
	import { setIsLoggedIn } from '$lib/caches/isLoggedIn';

	let { children } = $props();

	async function logout() {
		const authClient = new AuthClient(base);

		await authClient.logout();

		setIsLoggedIn(false);
		await goto('/', { invalidateAll: true });
	}
</script>

<AppBar
	toolbarClasses="container mx-auto w-full pl-2"
	leadClasses="items-center"
	trailClasses="items-center justify-end"
>
	{#snippet lead()}
		<a href="/" class="text-surface-950-50 text-3xl font-bold">R</a>
		<a href="/settings">Settings</a>
		<a href="/addTorrent">Add Torrent</a>
	{/snippet}
	{#snippet trail()}
		<select class="select preset-outlined-tertiary-500" onchange={({currentTarget}) => document.querySelector('html')?.setAttribute("data-theme", currentTarget.value)}>
			<option selected value="catppuccin">Catppuccin</option>
			<option value="cerberus">Cerberus</option>
			<option value="concord">concord</option>
			<option value="crimson">crimson</option>
			<option value="fennec">fennec</option>
			<option value="hamlindigo">hamlindigo</option>
			<option value="legacy">legacy</option>
			<option value="mint">mint</option>
			<option value="modern">modern</option>
			<option value="mona">mona</option>
			<option value="nosh">nosh</option>
			<option value="nouveau">nouveau</option>
			<option value="pine">pine</option>
			<option value="reign">reign</option>
			<option value="rocket">rocket</option>
			<option value="rose">rose</option>
			<option value="sahara">sahara</option>
			<option value="seafoam">seafoam</option>
			<option value="terminus">terminus</option>
			<option value="vintage">vintage</option>
			<option value="vox">vox</option>
			<option value="wintry">wintry</option>
		</select>
		<button class="button mr-0 p-2" popovertarget="profilepopover">
			<CircleUser />
		</button>

		<div
			id="profilepopover"
			class="card preset-outlined-surface-500 inset-[unset] translate-y-[calc(50%+2rem)] p-4"
			popover="auto"
		>
			<ul class="ul flex flex-col space-y-2">
				<li class="hover:bg-surface-200-800 rounded-base w-full p-2 text-left">
					<button class="button" onclick={() => logout()}>Logout</button>
				</li>
				<li class="hover:bg-surface-200-800 rounded-base w-full p-2 text-left">
					<a href="/changePassword">Change Password</a>
				</li>
				<hr class="hr" />
				<li class="hover:bg-surface-200-800 rounded-base w-full p-2 text-left">
					<a href="https://github.com/rogerfar/rdt-client/releases" target="_blank"
						>Client {version}</a
					>
				</li>
				<!-- <li><a href="https://github.com/rogerfar/rdt-client/releases" target="_blank">Server {version}</a></li> -->
			</ul>
		</div>
	{/snippet}
	<div class="invisible"></div>
</AppBar>

<main class="container mx-auto w-full pt-4">
	{@render children()}
</main>

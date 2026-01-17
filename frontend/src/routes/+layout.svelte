<script lang="ts">
	import '../app.css';
	import favicon from '$lib/assets/favicon.svg';
	import { auth } from '$lib/stores/auth';
	import * as Sidebar from '$lib/components/ui/sidebar';
	import AppSidebar from '$lib/components/app-sidebar.svelte';

	let { children, data } = $props();

	const isLoginPage = $derived(data?.pathname === '/login');
	const user = $derived($auth.user);
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
	<title>Research Data Collection Platform</title>
</svelte:head>

<div class="min-h-screen bg-background">
	{#if !isLoginPage && user}
		<Sidebar.Provider>
			<!-- Sidebar (handles both desktop and mobile automatically) -->
			<AppSidebar />

			<!-- Main content area -->
			<Sidebar.Inset>
				<!-- Mobile header with sidebar trigger -->
				<header class="md:hidden flex items-center gap-2 border-b bg-card p-4">
					<Sidebar.Trigger />
					<h1 class="text-lg font-semibold">Research Platform</h1>
				</header>

				<main class="flex-1 overflow-auto">
					<div class="container mx-auto p-4 md:p-6 lg:p-8">
						{@render children()}
					</div>
				</main>
			</Sidebar.Inset>
		</Sidebar.Provider>
	{:else}
		<main>{@render children()}</main>
	{/if}
</div>

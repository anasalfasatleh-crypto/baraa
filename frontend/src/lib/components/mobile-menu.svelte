<script lang="ts">
	import * as Sheet from '$lib/components/ui/sheet';
	import AppSidebar from './app-sidebar.svelte';
	import { Menu } from '@lucide/svelte';

	let open = $state(false);
</script>

<Sheet.Root bind:open>
	<Sheet.Trigger
		class="inline-flex items-center justify-center rounded-md p-2 text-muted-foreground hover:bg-accent hover:text-foreground focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary md:hidden"
		aria-label="Open menu"
	>
		<Menu class="h-6 w-6" />
	</Sheet.Trigger>
	<Sheet.Content side="left" class="p-0 w-72">
		<AppSidebar />
	</Sheet.Content>
</Sheet.Root>

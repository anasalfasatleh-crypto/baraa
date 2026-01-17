<script lang="ts">
	import { onMount } from 'svelte';
	import { materialsApi, type Material } from '$lib/api/materials';
	import MaterialCard from '$lib/components/MaterialCard.svelte';

	let materials = $state<Material[]>([]);
	let isLoading = $state(true);
	let error = $state<string | null>(null);

	onMount(async () => {
		await loadMaterials();
	});

	async function loadMaterials() {
		isLoading = true;
		error = null;

		try {
			materials = await materialsApi.getMaterials();
		} catch (err) {
			if (err instanceof Error && err.message.includes('403')) {
				error = 'You must complete the pre-test before accessing educational materials.';
			} else {
				error = err instanceof Error ? err.message : 'Failed to load materials';
			}
		} finally {
			isLoading = false;
		}
	}
</script>

<div class="max-w-7xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
	<div class="mb-8">
		<h1 class="text-3xl font-bold text-gray-900 mb-2">Educational Materials</h1>
		<p class="text-gray-600">Access videos, PDFs, and reading materials about ICU delirium.</p>
	</div>

	{#if isLoading}
		<div class="flex items-center justify-center py-12">
			<svg
				class="animate-spin h-12 w-12 text-indigo-600"
				fill="none"
				viewBox="0 0 24 24"
			>
				<circle
					class="opacity-25"
					cx="12"
					cy="12"
					r="10"
					stroke="currentColor"
					stroke-width="4"
				></circle>
				<path
					class="opacity-75"
					fill="currentColor"
					d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
				></path>
			</svg>
		</div>
	{:else if error}
		<div class="bg-red-50 border border-red-200 rounded-lg p-6">
			<div class="flex items-start">
				<svg
					class="h-6 w-6 text-red-600 mr-3 flex-shrink-0"
					fill="none"
					stroke="currentColor"
					viewBox="0 0 24 24"
				>
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
					></path>
				</svg>
				<div>
					<h3 class="text-red-800 font-semibold mb-1">Access Denied</h3>
					<p class="text-red-700">{error}</p>
				</div>
			</div>
		</div>
	{:else if materials.length === 0}
		<div class="bg-yellow-50 border border-yellow-200 rounded-lg p-6 text-center">
			<svg
				class="h-12 w-12 text-yellow-600 mx-auto mb-3"
				fill="none"
				stroke="currentColor"
				viewBox="0 0 24 24"
			>
				<path
					stroke-linecap="round"
					stroke-linejoin="round"
					stroke-width="2"
					d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
				></path>
			</svg>
			<p class="text-yellow-800 font-medium">No materials available yet</p>
			<p class="text-yellow-700 text-sm mt-1">Check back later for educational content.</p>
		</div>
	{:else}
		<div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
			{#each materials as material (material.id)}
				<MaterialCard {material} />
			{/each}
		</div>
	{/if}
</div>

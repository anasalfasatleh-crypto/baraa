<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { materialsApi, type MaterialDetail } from '$lib/api/materials';
	import PdfViewer from '$lib/components/PdfViewer.svelte';
	import VideoPlayer from '$lib/components/VideoPlayer.svelte';

	const materialId = $derived($page.params.id ?? '');

	let material = $state<MaterialDetail | null>(null);
	let isLoading = $state(true);
	let error = $state<string | null>(null);

	let accessStartTime: Date | null = null;
	let trackingInterval: ReturnType<typeof setInterval> | null = null;
	let hasTrackedInitialAccess = false;

	onMount(async () => {
		if (!materialId) {
			error = 'Invalid material ID';
			isLoading = false;
			return;
		}

		await loadMaterial();

		// Start tracking access time
		accessStartTime = new Date();

		// Track initial access
		if (material) {
			try {
				await materialsApi.trackAccess(materialId, { completed: false });
				hasTrackedInitialAccess = true;
			} catch (err) {
				console.error('Failed to track initial access:', err);
			}
		}

		// Set up periodic tracking every 30 seconds
		trackingInterval = setInterval(() => {
			if (accessStartTime && material) {
				const durationSeconds = Math.floor((new Date().getTime() - accessStartTime.getTime()) / 1000);
				materialsApi.trackAccess(materialId, {
					durationSeconds,
					completed: false
				}).catch(err => console.error('Failed to track access:', err));
			}
		}, 30000);
	});

	onDestroy(() => {
		// Track final access time when leaving the page
		if (trackingInterval) {
			clearInterval(trackingInterval);
		}

		if (accessStartTime && material && hasTrackedInitialAccess) {
			const durationSeconds = Math.floor((new Date().getTime() - accessStartTime.getTime()) / 1000);
			materialsApi.trackAccess(materialId, {
				durationSeconds,
				completed: false
			}).catch(err => console.error('Failed to track final access:', err));
		}
	});

	async function loadMaterial() {
		isLoading = true;
		error = null;

		try {
			material = await materialsApi.getMaterialDetail(materialId);
		} catch (err) {
			if (err instanceof Error && err.message.includes('403')) {
				error = 'You must complete the pre-test before accessing educational materials.';
			} else if (err instanceof Error && err.message.includes('404')) {
				error = 'Material not found.';
			} else {
				error = err instanceof Error ? err.message : 'Failed to load material';
			}
		} finally {
			isLoading = false;
		}
	}

	function handleVideoEnded() {
		// Mark as completed when video finishes
		if (accessStartTime && material) {
			const durationSeconds = Math.floor((new Date().getTime() - accessStartTime.getTime()) / 1000);
			materialsApi.trackAccess(materialId, {
				durationSeconds,
				completed: true
			}).catch(err => console.error('Failed to mark as completed:', err));
		}
	}

	function goBack() {
		goto('/student/materials');
	}
</script>

<div class="max-w-6xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
	<button
		type="button"
		class="mb-6 flex items-center gap-2 text-gray-600 hover:text-gray-900 transition-colors"
		onclick={goBack}
	>
		<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
			<path
				stroke-linecap="round"
				stroke-linejoin="round"
				stroke-width="2"
				d="M15 19l-7-7 7-7"
			></path>
		</svg>
		Back to Materials
	</button>

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
					<h3 class="text-red-800 font-semibold mb-1">Error</h3>
					<p class="text-red-700">{error}</p>
				</div>
			</div>
		</div>
	{:else if material}
		<div class="mb-6">
			<h1 class="text-3xl font-bold text-gray-900 mb-2">{material.title}</h1>
			{#if material.description}
				<p class="text-gray-600 mb-4">{material.description}</p>
			{/if}
			<div class="flex items-center gap-4 text-sm text-gray-500">
				<span class="flex items-center gap-1">
					<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
						></path>
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
						></path>
					</svg>
					Viewed {material.accessCount} {material.accessCount === 1 ? 'time' : 'times'}
				</span>
			</div>
		</div>

		<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
			{#if material.type === 'Pdf'}
				<PdfViewer url={material.signedUrl} title={material.title} />
			{:else if material.type === 'Video'}
				<VideoPlayer
					url={material.signedUrl}
					title={material.title}
					onended={handleVideoEnded}
				/>
			{:else if material.type === 'Text'}
				<div class="prose max-w-none">
					<iframe
						src={material.signedUrl}
						title={material.title}
						class="w-full min-h-[600px] border-0"
					></iframe>
				</div>
			{/if}
		</div>
	{/if}
</div>

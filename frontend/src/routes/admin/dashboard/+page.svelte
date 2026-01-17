<script lang="ts">
	import { onMount } from 'svelte';
	import { adminApi, type DashboardMetrics } from '$lib/api/admin';
	import DashboardMetricsComponent from '$lib/components/DashboardMetrics.svelte';

	let metrics = $state<DashboardMetrics | null>(null);
	let isLoading = $state(true);
	let error = $state<string | null>(null);

	onMount(async () => {
		await loadMetrics();
	});

	async function loadMetrics() {
		isLoading = true;
		error = null;

		try {
			metrics = await adminApi.getDashboard();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load dashboard metrics';
		} finally {
			isLoading = false;
		}
	}
</script>

<div class="max-w-7xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
	<div class="mb-8">
		<h1 class="text-3xl font-bold text-gray-900 mb-2">Admin Dashboard</h1>
		<p class="text-gray-600">Monitor study progress and manage platform.</p>
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
					<h3 class="text-red-800 font-semibold mb-1">Error</h3>
					<p class="text-red-700">{error}</p>
				</div>
			</div>
		</div>
	{:else if metrics}
		<div class="space-y-8">
			<DashboardMetricsComponent {metrics} />

			<!-- Quick Actions -->
			<div class="bg-white shadow rounded-lg p-6">
				<h2 class="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
				<div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
					<a
						href="/admin/users"
						class="flex items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
					>
						<svg class="h-8 w-8 text-indigo-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
							/>
						</svg>
						<div>
							<div class="font-medium text-gray-900">Manage Users</div>
							<div class="text-sm text-gray-500">Create, edit, and manage users</div>
						</div>
					</a>

					<a
						href="/admin/users"
						class="flex items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
					>
						<svg class="h-8 w-8 text-indigo-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
							/>
						</svg>
						<div>
							<div class="font-medium text-gray-900">Evaluator Assignments</div>
							<div class="text-sm text-gray-500">Assign evaluators to students</div>
						</div>
					</a>

					<div class="flex items-center p-4 border border-gray-200 rounded-lg">
						<svg class="h-8 w-8 text-indigo-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
							/>
						</svg>
						<div>
							<div class="font-medium text-gray-900">Post-test Batch</div>
							<div class="text-sm text-gray-500">
								{#if metrics.posttestBatchOpen}
									<span class="text-green-600">Currently Open</span>
								{:else}
									<span class="text-red-600">Currently Closed</span>
								{/if}
							</div>
						</div>
					</div>
				</div>
			</div>

			<!-- Current Batch Info -->
			{#if metrics.currentBatchName}
				<div class="bg-white shadow rounded-lg p-6">
					<h2 class="text-lg font-semibold text-gray-900 mb-4">Current Post-test Batch</h2>
					<dl class="grid grid-cols-1 gap-4 sm:grid-cols-3">
						<div>
							<dt class="text-sm font-medium text-gray-500">Batch Name</dt>
							<dd class="mt-1 text-sm text-gray-900">{metrics.currentBatchName}</dd>
						</div>
						<div>
							<dt class="text-sm font-medium text-gray-500">Open Date</dt>
							<dd class="mt-1 text-sm text-gray-900">
								{metrics.currentBatchOpenDate ? new Date(metrics.currentBatchOpenDate).toLocaleDateString() : '-'}
							</dd>
						</div>
						<div>
							<dt class="text-sm font-medium text-gray-500">Close Date</dt>
							<dd class="mt-1 text-sm text-gray-900">
								{metrics.currentBatchCloseDate ? new Date(metrics.currentBatchCloseDate).toLocaleDateString() : '-'}
							</dd>
						</div>
					</dl>
				</div>
			{/if}
		</div>
	{/if}
</div>

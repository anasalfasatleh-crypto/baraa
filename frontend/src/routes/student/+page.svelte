<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { auth } from '$lib/stores/auth';
	import { studentApi, type StudentStatus } from '$lib/api/student';

	const user = $derived($auth.user);

	let status = $state<StudentStatus | null>(null);
	let isLoading = $state(true);
	let error = $state<string | null>(null);

	onMount(async () => {
		await loadStatus();
	});

	async function loadStatus() {
		isLoading = true;
		error = null;

		try {
			status = await studentApi.getStatus();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load status';
		} finally {
			isLoading = false;
		}
	}

	function navigateToPretest() {
		goto('/student/pretest');
	}

	function navigateToMaterials() {
		goto('/student/materials');
	}

	function navigateToPosttest() {
		goto('/student/posttest');
	}

	function formatDate(dateString: string | null) {
		if (!dateString) return '';
		const date = new Date(dateString);
		return date.toLocaleDateString('en-US', {
			year: 'numeric',
			month: 'short',
			day: 'numeric'
		});
	}
</script>

<div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
	<div class="px-4 py-6 sm:px-0">
		<div class="mb-6">
			<h1 class="text-3xl font-bold text-gray-900 mb-2">Welcome, {user?.name}!</h1>
			<p class="text-gray-600">Track your progress through the ICU Delirium study.</p>
		</div>

		{#if isLoading}
			<div class="flex items-center justify-center py-12">
				<svg
					class="animate-spin h-8 w-8 text-indigo-600"
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
					/>
					<path
						class="opacity-75"
						fill="currentColor"
						d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
					/>
				</svg>
			</div>
		{:else if error}
			<div class="bg-red-50 border border-red-200 rounded-lg p-4">
				<p class="text-red-800">{error}</p>
			</div>
		{:else if status}
			<!-- Status Cards -->
			<div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 mb-8">
				<div class="bg-white overflow-hidden shadow rounded-lg">
					<div class="px-4 py-5 sm:p-6">
						<dt class="text-sm font-medium text-gray-500 truncate">Pre-Test Status</dt>
						<dd class="mt-1 text-2xl font-semibold {status.pretestCompleted ? 'text-green-600' : 'text-gray-900'}">
							{status.pretestCompleted ? 'Completed' : 'Not Started'}
						</dd>
						{#if status.pretestCompletedAt}
							<p class="mt-1 text-sm text-gray-500">
								{formatDate(status.pretestCompletedAt)}
							</p>
						{/if}
					</div>
				</div>

				<div class="bg-white overflow-hidden shadow rounded-lg">
					<div class="px-4 py-5 sm:p-6">
						<dt class="text-sm font-medium text-gray-500 truncate">Materials Accessed</dt>
						<dd class="mt-1 text-2xl font-semibold text-gray-900">
							{status.materialsAccessed}
						</dd>
						<p class="mt-1 text-sm text-gray-500">Educational materials</p>
					</div>
				</div>

				<div class="bg-white overflow-hidden shadow rounded-lg">
					<div class="px-4 py-5 sm:p-6">
						<dt class="text-sm font-medium text-gray-500 truncate">Post-Test Status</dt>
						<dd class="mt-1 text-2xl font-semibold {status.posttestCompleted ? 'text-green-600' : 'text-gray-900'}">
							{status.posttestCompleted ? 'Completed' : status.pretestCompleted ? 'Available' : 'Locked'}
						</dd>
						{#if status.posttestCompletedAt}
							<p class="mt-1 text-sm text-gray-500">
								{formatDate(status.posttestCompletedAt)}
							</p>
						{/if}
					</div>
				</div>
			</div>

			<!-- Action Cards -->
			<div class="space-y-4">
				<!-- Pre-Test Card -->
				<div class="bg-white shadow rounded-lg overflow-hidden">
					<div class="px-6 py-5">
						<div class="flex items-center justify-between">
							<div class="flex-1">
								<h3 class="text-lg font-semibold text-gray-900 mb-1">
									Pre-Test Assessment
								</h3>
								<p class="text-gray-600 text-sm">
									{status.pretestCompleted
										? 'You have completed the pre-test assessment.'
										: 'Complete the initial assessment before accessing educational materials.'}
								</p>
							</div>
							<div class="ml-4">
								{#if status.pretestCompleted}
									<button
										type="button"
										class="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg font-medium cursor-default"
										disabled
									>
										Completed
									</button>
								{:else}
									<button
										type="button"
										class="px-6 py-2 bg-indigo-600 text-white rounded-lg font-medium hover:bg-indigo-700 transition-all"
										onclick={navigateToPretest}
									>
										Start Pre-Test
									</button>
								{/if}
							</div>
						</div>
					</div>
				</div>

				<!-- Educational Materials Card -->
				<div class="bg-white shadow rounded-lg overflow-hidden {!status.pretestCompleted ? 'opacity-60' : ''}">
					<div class="px-6 py-5">
						<div class="flex items-center justify-between">
							<div class="flex-1">
								<h3 class="text-lg font-semibold text-gray-900 mb-1">
									Educational Materials
									{#if !status.pretestCompleted}
										<span class="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
											Locked
										</span>
									{/if}
								</h3>
								<p class="text-gray-600 text-sm">
									{status.pretestCompleted
										? 'Access educational materials about ICU delirium.'
										: 'Complete the pre-test to unlock educational materials.'}
								</p>
							</div>
							<div class="ml-4">
								<button
									type="button"
									class="px-6 py-2 {status.pretestCompleted
										? 'bg-indigo-600 text-white hover:bg-indigo-700'
										: 'bg-gray-200 text-gray-500 cursor-not-allowed'} rounded-lg font-medium transition-all"
									disabled={!status.pretestCompleted}
									onclick={navigateToMaterials}
								>
									View Materials
								</button>
							</div>
						</div>
					</div>
				</div>

				<!-- Post-Test Card -->
				<div class="bg-white shadow rounded-lg overflow-hidden {!status.pretestCompleted ? 'opacity-60' : ''}">
					<div class="px-6 py-5">
						<div class="flex items-center justify-between">
							<div class="flex-1">
								<h3 class="text-lg font-semibold text-gray-900 mb-1">
									Post-Test Assessment
									{#if !status.pretestCompleted}
										<span class="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
											Locked
										</span>
									{/if}
								</h3>
								<p class="text-gray-600 text-sm">
									{status.posttestCompleted
										? 'You have completed the post-test assessment.'
										: status.pretestCompleted
											? 'Complete the final assessment after reviewing educational materials.'
											: 'Complete the pre-test to unlock the post-test.'}
								</p>
							</div>
							<div class="ml-4">
								{#if status.posttestCompleted}
									<button
										type="button"
										class="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg font-medium cursor-default"
										disabled
									>
										Completed
									</button>
								{:else}
									<button
										type="button"
										class="px-6 py-2 {status.pretestCompleted
											? 'bg-indigo-600 text-white hover:bg-indigo-700'
											: 'bg-gray-200 text-gray-500 cursor-not-allowed'} rounded-lg font-medium transition-all"
										disabled={!status.pretestCompleted}
										onclick={navigateToPosttest}
									>
										Start Post-Test
									</button>
								{/if}
							</div>
						</div>
					</div>
				</div>
			</div>
		{/if}
	</div>
</div>

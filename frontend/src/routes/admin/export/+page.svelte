<script lang="ts">
	import { adminApi } from '$lib/api/admin';
	import ExportButton from '$lib/components/ExportButton.svelte';

	let isExporting = $state(false);
	let error = $state<string | null>(null);
	let successMessage = $state<string | null>(null);
	let exportHistory = $state<Array<{ date: Date; format: string; filename: string }>>([]);

	async function handleExport(format: 'excel' | 'csv') {
		isExporting = true;
		error = null;
		successMessage = null;

		try {
			const blob = await adminApi.exportData(format);
			const url = window.URL.createObjectURL(blob);
			const a = document.createElement('a');
			a.href = url;
			const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
			const filename = `research_data_${timestamp}.${format === 'excel' ? 'xlsx' : 'csv'}`;
			a.download = filename;
			document.body.appendChild(a);
			a.click();
			window.URL.revokeObjectURL(url);
			document.body.removeChild(a);

			successMessage = `Successfully exported data to ${filename}`;

			// Add to export history
			exportHistory = [{ date: new Date(), format, filename }, ...exportHistory];
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to export data';
		} finally {
			isExporting = false;
		}
	}
</script>

<div class="min-h-screen bg-gray-50 py-8">
	<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
		<!-- Header -->
		<div class="md:flex md:items-center md:justify-between mb-8">
			<div class="flex-1 min-w-0">
				<h1 class="text-3xl font-bold text-gray-900">Export Research Data</h1>
				<p class="mt-2 text-sm text-gray-600">
					Export all collected research data in Excel or CSV format compatible with SPSS
				</p>
			</div>
			<div class="mt-4 md:mt-0 md:ml-4">
				<ExportButton onExport={handleExport} {isExporting} />
			</div>
		</div>

		<!-- Success/Error Messages -->
		{#if successMessage}
			<div class="mb-4 bg-green-50 border border-green-200 text-green-800 px-4 py-3 rounded-md">
				<div class="flex">
					<div class="flex-shrink-0">
						<svg
							class="h-5 w-5 text-green-400"
							fill="currentColor"
							viewBox="0 0 20 20"
							aria-hidden="true"
						>
							<path
								fill-rule="evenodd"
								d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
								clip-rule="evenodd"
							></path>
						</svg>
					</div>
					<div class="ml-3">
						<p class="text-sm font-medium">{successMessage}</p>
					</div>
					<div class="ml-auto pl-3">
						<button
							type="button"
							onclick={() => (successMessage = null)}
							class="inline-flex text-green-400 hover:text-green-500"
						>
							<span class="sr-only">Dismiss</span>
							<svg class="h-5 w-5" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
								<path
									fill-rule="evenodd"
									d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
									clip-rule="evenodd"
								></path>
							</svg>
						</button>
					</div>
				</div>
			</div>
		{/if}

		{#if error}
			<div class="mb-4 bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-md">
				<div class="flex">
					<div class="flex-shrink-0">
						<svg
							class="h-5 w-5 text-red-400"
							fill="currentColor"
							viewBox="0 0 20 20"
							aria-hidden="true"
						>
							<path
								fill-rule="evenodd"
								d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
								clip-rule="evenodd"
							></path>
						</svg>
					</div>
					<div class="ml-3">
						<p class="text-sm font-medium">{error}</p>
					</div>
					<div class="ml-auto pl-3">
						<button
							type="button"
							onclick={() => (error = null)}
							class="inline-flex text-red-400 hover:text-red-500"
						>
							<span class="sr-only">Dismiss</span>
							<svg class="h-5 w-5" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
								<path
									fill-rule="evenodd"
									d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
									clip-rule="evenodd"
								></path>
							</svg>
						</button>
					</div>
				</div>
			</div>
		{/if}

		<!-- Export Information -->
		<div class="bg-white shadow rounded-lg mb-8">
			<div class="px-6 py-5">
				<h2 class="text-lg font-medium text-gray-900 mb-4">Export Data</h2>
				<div class="grid grid-cols-1 gap-6 sm:grid-cols-2">
					<!-- Excel Export Info -->
					<div class="border border-gray-200 rounded-lg p-4">
						<div class="flex items-center mb-3">
							<svg
								class="h-8 w-8 text-green-500"
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
							<h3 class="ml-3 text-lg font-medium text-gray-900">Excel Format (.xlsx)</h3>
						</div>
						<p class="text-sm text-gray-600 mb-3">
							Export data as an Excel spreadsheet with formatted columns and headers. Best for:
						</p>
						<ul class="text-sm text-gray-600 space-y-1 ml-4 list-disc">
							<li>Quick data review and analysis</li>
							<li>Creating charts and visualizations</li>
							<li>Sharing with non-technical users</li>
							<li>SPSS import (File → Import → Excel)</li>
						</ul>
					</div>

					<!-- CSV Export Info -->
					<div class="border border-gray-200 rounded-lg p-4">
						<div class="flex items-center mb-3">
							<svg
								class="h-8 w-8 text-blue-500"
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
							<h3 class="ml-3 text-lg font-medium text-gray-900">CSV Format (.csv)</h3>
						</div>
						<p class="text-sm text-gray-600 mb-3">
							Export data as a comma-separated values file. Best for:
						</p>
						<ul class="text-sm text-gray-600 space-y-1 ml-4 list-disc">
							<li>Importing into statistical software</li>
							<li>Processing with scripts/code</li>
							<li>Universal compatibility</li>
							<li>SPSS import (File → Import → CSV)</li>
						</ul>
					</div>
				</div>
			</div>
		</div>

		<!-- Data Included Section -->
		<div class="bg-white shadow rounded-lg mb-8">
			<div class="px-6 py-5">
				<h2 class="text-lg font-medium text-gray-900 mb-4">Included Data</h2>
				<p class="text-sm text-gray-600 mb-4">
					The export includes all collected research data in a flat structure optimized for SPSS
					analysis:
				</p>
				<div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
					<div class="flex items-start">
						<svg class="h-5 w-5 text-green-500 mr-2 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
							<path
								fill-rule="evenodd"
								d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
								clip-rule="evenodd"
							></path>
						</svg>
						<div class="text-sm">
							<span class="font-medium text-gray-900">Student Demographics</span>
							<p class="text-gray-600">Name, email, hospital, gender</p>
						</div>
					</div>
					<div class="flex items-start">
						<svg class="h-5 w-5 text-green-500 mr-2 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
							<path
								fill-rule="evenodd"
								d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
								clip-rule="evenodd"
							></path>
						</svg>
						<div class="text-sm">
							<span class="font-medium text-gray-900">Pre-test Responses</span>
							<p class="text-gray-600">All question answers and submission time</p>
						</div>
					</div>
					<div class="flex items-start">
						<svg class="h-5 w-5 text-green-500 mr-2 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
							<path
								fill-rule="evenodd"
								d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
								clip-rule="evenodd"
							></path>
						</svg>
						<div class="text-sm">
							<span class="font-medium text-gray-900">Post-test Responses</span>
							<p class="text-gray-600">All question answers and submission time</p>
						</div>
					</div>
					<div class="flex items-start">
						<svg class="h-5 w-5 text-green-500 mr-2 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
							<path
								fill-rule="evenodd"
								d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
								clip-rule="evenodd"
							></path>
						</svg>
						<div class="text-sm">
							<span class="font-medium text-gray-900">Material Access</span>
							<p class="text-gray-600">Accessed materials and timestamps</p>
						</div>
					</div>
					<div class="flex items-start">
						<svg class="h-5 w-5 text-green-500 mr-2 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
							<path
								fill-rule="evenodd"
								d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
								clip-rule="evenodd"
							></path>
						</svg>
						<div class="text-sm">
							<span class="font-medium text-gray-900">Time Tracking</span>
							<p class="text-gray-600">Total time spent on questionnaires</p>
						</div>
					</div>
					<div class="flex items-start">
						<svg class="h-5 w-5 text-green-500 mr-2 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
							<path
								fill-rule="evenodd"
								d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
								clip-rule="evenodd"
							></path>
						</svg>
						<div class="text-sm">
							<span class="font-medium text-gray-900">Evaluator Scores</span>
							<p class="text-gray-600">Individual and combined scores</p>
						</div>
					</div>
				</div>
			</div>
		</div>

		<!-- Export History -->
		{#if exportHistory.length > 0}
			<div class="bg-white shadow rounded-lg">
				<div class="px-6 py-5">
					<h2 class="text-lg font-medium text-gray-900 mb-4">Recent Exports</h2>
					<div class="overflow-x-auto">
						<table class="min-w-full divide-y divide-gray-200">
							<thead class="bg-gray-50">
								<tr>
									<th
										class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
									>
										Date
									</th>
									<th
										class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
									>
										Format
									</th>
									<th
										class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
									>
										Filename
									</th>
								</tr>
							</thead>
							<tbody class="bg-white divide-y divide-gray-200">
								{#each exportHistory as export_}
									<tr>
										<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
											{export_.date.toLocaleString()}
										</td>
										<td class="px-6 py-4 whitespace-nowrap">
											<span
												class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full {export_.format ===
												'excel'
													? 'bg-green-100 text-green-800'
													: 'bg-blue-100 text-blue-800'}"
											>
												{export_.format.toUpperCase()}
											</span>
										</td>
										<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
											{export_.filename}
										</td>
									</tr>
								{/each}
							</tbody>
						</table>
					</div>
				</div>
			</div>
		{/if}
	</div>
</div>

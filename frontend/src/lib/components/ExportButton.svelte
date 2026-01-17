<script lang="ts">
	interface Props {
		onExport: (format: 'excel' | 'csv') => void;
		isExporting?: boolean;
	}

	let { onExport, isExporting = false }: Props = $props();

	let showDropdown = $state(false);

	function handleExport(format: 'excel' | 'csv') {
		showDropdown = false;
		onExport(format);
	}
</script>

<div class="relative inline-block text-left">
	<button
		type="button"
		onclick={() => (showDropdown = !showDropdown)}
		disabled={isExporting}
		class="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:bg-indigo-400 disabled:cursor-not-allowed"
	>
		{#if isExporting}
			<svg
				class="animate-spin -ml-1 mr-2 h-4 w-4 text-white"
				fill="none"
				viewBox="0 0 24 24"
			>
				<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"
				></circle>
				<path
					class="opacity-75"
					fill="currentColor"
					d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
				></path>
			</svg>
			Exporting...
		{:else}
			<svg class="-ml-1 mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path
					stroke-linecap="round"
					stroke-linejoin="round"
					stroke-width="2"
					d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
				></path>
			</svg>
			Export Data
		{/if}
		<svg
			class="-mr-1 ml-2 h-5 w-5"
			fill="currentColor"
			viewBox="0 0 20 20"
			aria-hidden="true"
		>
			<path
				fill-rule="evenodd"
				d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
				clip-rule="evenodd"
			></path>
		</svg>
	</button>

	{#if showDropdown}
		<div
			class="origin-top-right absolute right-0 mt-2 w-56 rounded-md shadow-lg bg-white ring-1 ring-black ring-opacity-5 z-10"
		>
			<div class="py-1" role="menu" aria-orientation="vertical">
				<button
					type="button"
					onclick={() => handleExport('excel')}
					class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 hover:text-gray-900"
					role="menuitem"
				>
					<div class="flex items-center">
						<svg class="mr-3 h-5 w-5 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
							></path>
						</svg>
						<div>
							<div class="font-medium">Export to Excel</div>
							<div class="text-xs text-gray-500">Download as .xlsx file</div>
						</div>
					</div>
				</button>
				<button
					type="button"
					onclick={() => handleExport('csv')}
					class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 hover:text-gray-900"
					role="menuitem"
				>
					<div class="flex items-center">
						<svg class="mr-3 h-5 w-5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
							></path>
						</svg>
						<div>
							<div class="font-medium">Export to CSV</div>
							<div class="text-xs text-gray-500">Download as .csv file</div>
						</div>
					</div>
				</button>
			</div>
		</div>
	{/if}
</div>

<!-- Click outside to close dropdown -->
{#if showDropdown}
	<button
		type="button"
		class="fixed inset-0 z-0"
		onclick={() => (showDropdown = false)}
		aria-label="Close dropdown"
		tabindex="-1"
	></button>
{/if}

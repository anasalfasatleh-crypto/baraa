<script lang="ts">
	import type { AssignedStudent } from '$lib/api/evaluator';

	interface Props {
		student: AssignedStudent;
		onclick?: () => void;
	}

	let { student, onclick }: Props = $props();
</script>

<button
	type="button"
	onclick={onclick}
	class="w-full bg-white rounded-lg shadow-sm border border-gray-200 p-6 hover:shadow-md hover:border-indigo-300 transition-all text-left"
>
	<div class="flex items-start justify-between">
		<div class="flex-1">
			<h3 class="text-lg font-semibold text-gray-900 mb-1">
				{student.name}
			</h3>
			<p class="text-sm text-gray-600 mb-2">
				{student.email}
			</p>
			{#if student.hospital || student.gender}
				<div class="flex gap-3 text-sm text-gray-500">
					{#if student.hospital}
						<span class="flex items-center gap-1">
							<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
								/>
							</svg>
							{student.hospital}
						</span>
					{/if}
					{#if student.gender}
						<span class="flex items-center gap-1">
							<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
								/>
							</svg>
							{student.gender}
						</span>
					{/if}
				</div>
			{/if}
		</div>
		<svg
			class="h-5 w-5 text-gray-400 flex-shrink-0"
			fill="none"
			stroke="currentColor"
			viewBox="0 0 24 24"
		>
			<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
		</svg>
	</div>
</button>

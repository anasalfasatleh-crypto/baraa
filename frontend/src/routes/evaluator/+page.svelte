<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { evaluatorApi, type AssignedStudent } from '$lib/api/evaluator';
	import StudentScoreCard from '$lib/components/StudentScoreCard.svelte';

	let students = $state<AssignedStudent[]>([]);
	let isLoading = $state(true);
	let error = $state<string | null>(null);

	onMount(async () => {
		await loadStudents();
	});

	async function loadStudents() {
		isLoading = true;
		error = null;

		try {
			students = await evaluatorApi.getAssignedStudents();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load assigned students';
		} finally {
			isLoading = false;
		}
	}

	function handleStudentClick(studentId: string) {
		goto(`/evaluator/students/${studentId}`);
	}
</script>

<div class="max-w-7xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
	<div class="mb-8">
		<h1 class="text-3xl font-bold text-gray-900 mb-2">Evaluator Dashboard</h1>
		<p class="text-gray-600">Review and score student responses.</p>
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
	{:else if students.length === 0}
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
					d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
				></path>
			</svg>
			<p class="text-yellow-800 font-medium">No students assigned yet</p>
			<p class="text-yellow-700 text-sm mt-1">
				You will see assigned students here once an administrator assigns them to you.
			</p>
		</div>
	{:else}
		<div>
			<h2 class="text-lg font-semibold text-gray-900 mb-4">
				Assigned Students ({students.length})
			</h2>
			<div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
				{#each students as student (student.id)}
					<StudentScoreCard {student} onclick={() => handleStudentClick(student.id)} />
				{/each}
			</div>
		</div>
	{/if}
</div>

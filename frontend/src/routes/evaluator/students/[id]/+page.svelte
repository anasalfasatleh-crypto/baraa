<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { evaluatorApi, type StudentResponses } from '$lib/api/evaluator';
	import ScoringForm from '$lib/components/ScoringForm.svelte';

	let studentId = $derived($page.params.id ?? '');
	let questionnaireType = $state<'pretest' | 'posttest'>('pretest');

	let responses = $state<StudentResponses | null>(null);
	let scores = $state<Record<string, number>>({});
	let isLoading = $state(true);
	let isSaving = $state(false);
	let error = $state<string | null>(null);
	let successMessage = $state<string | null>(null);

	onMount(async () => {
		if (!studentId) {
			error = 'Invalid student ID';
			isLoading = false;
			return;
		}
		await loadResponses();
	});

	async function loadResponses() {
		isLoading = true;
		error = null;

		try {
			responses = await evaluatorApi.getStudentResponses(studentId, questionnaireType);

			// Initialize scores from existing scores
			const scoresMap: Record<string, number> = {};
			responses.responses.forEach(r => {
				if (r.score !== undefined && r.score !== null) {
					scoresMap[r.questionId] = r.score;
				}
			});
			scores = scoresMap;
		} catch (err) {
			if (err instanceof Error && err.message.includes('403')) {
				error = 'You are not assigned to this student or access is denied.';
			} else {
				error = err instanceof Error ? err.message : 'Failed to load student responses';
			}
		} finally {
			isLoading = false;
		}
	}

	function handleScoreChange(questionId: string, score: number) {
		scores = { ...scores, [questionId]: score };
		successMessage = null;
	}

	async function handleSave(finalize: boolean = false) {
		if (!responses) return;

		isSaving = true;
		error = null;
		successMessage = null;

		try {
			const scoreItems = Object.entries(scores).map(([questionId, score]) => ({
				questionId,
				score
			}));

			await evaluatorApi.saveScores(studentId, {
				questionnaireId: responses.questionnaireId,
				scores: scoreItems,
				finalize
			});

			successMessage = finalize
				? 'Scores saved and finalized successfully!'
				: 'Scores saved successfully!';

			// Reload to get updated data
			await loadResponses();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to save scores';
		} finally {
			isSaving = false;
		}
	}

	async function handleTypeChange(type: 'pretest' | 'posttest') {
		questionnaireType = type;
		await loadResponses();
	}

	const allScoresEntered = $derived(() => {
		if (!responses) return false;
		return responses.responses.every(r =>
			r.score !== undefined && r.score !== null || scores[r.questionId] !== undefined
		);
	});

	const hasUnsavedChanges = $derived(() => {
		if (!responses) return false;
		const resp = responses;
		return Object.keys(scores).some(qId => {
			const response = resp.responses.find(r => r.questionId === qId);
			return response && response.score !== scores[qId];
		});
	});

	const isFinalized = $derived(() => {
		if (!responses) return false;
		return responses.responses.every(r => r.isScoreFinalized);
	});
</script>

<div class="max-w-6xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
	<div class="mb-6">
		<button
			type="button"
			onclick={() => goto('/evaluator')}
			class="flex items-center text-indigo-600 hover:text-indigo-800 mb-4"
		>
			<svg class="h-5 w-5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
			</svg>
			Back to Students
		</button>
	</div>

	{#if isLoading}
		<div class="flex items-center justify-center min-h-96">
			<div class="text-center">
				<svg
					class="animate-spin h-12 w-12 text-indigo-600 mx-auto mb-4"
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
				<p class="text-gray-600">Loading student responses...</p>
			</div>
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
	{:else if responses}
		<div class="space-y-6">
			<!-- Header -->
			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h1 class="text-3xl font-bold text-gray-900 mb-2">
					{responses.questionnaireTitle}
				</h1>
				<p class="text-gray-600 mb-4">
					Submitted: {responses.submittedAt ? new Date(responses.submittedAt).toLocaleString() : 'Not submitted'}
				</p>

				<!-- Type Selector -->
				<div class="flex gap-2">
					<button
						type="button"
						onclick={() => handleTypeChange('pretest')}
						class="px-4 py-2 rounded-md {questionnaireType === 'pretest'
							? 'bg-indigo-600 text-white'
							: 'bg-gray-200 text-gray-700 hover:bg-gray-300'}"
						disabled={isLoading}
					>
						Pre-Test
					</button>
					<button
						type="button"
						onclick={() => handleTypeChange('posttest')}
						class="px-4 py-2 rounded-md {questionnaireType === 'posttest'
							? 'bg-indigo-600 text-white'
							: 'bg-gray-200 text-gray-700 hover:bg-gray-300'}"
						disabled={isLoading}
					>
						Post-Test
					</button>
				</div>

				{#if isFinalized()}
					<div class="mt-4 bg-green-50 border border-green-200 rounded-lg p-4">
						<div class="flex items-center">
							<svg
								class="h-5 w-5 text-green-600 mr-2"
								fill="none"
								stroke="currentColor"
								viewBox="0 0 24 24"
							>
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M5 13l4 4L19 7"
								></path>
							</svg>
							<span class="text-green-800 font-medium">All scores have been finalized</span>
						</div>
					</div>
				{/if}
			</div>

			<!-- Success Message -->
			{#if successMessage}
				<div class="bg-green-50 border border-green-200 rounded-lg p-4">
					<div class="flex items-center">
						<svg
							class="h-5 w-5 text-green-600 mr-2"
							fill="none"
							stroke="currentColor"
							viewBox="0 0 24 24"
						>
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M5 13l4 4L19 7"
							></path>
						</svg>
						<span class="text-green-800">{successMessage}</span>
					</div>
				</div>
			{/if}

			<!-- Scoring Form -->
			<ScoringForm
				responses={responses.responses}
				{scores}
				disabled={isSaving || isFinalized()}
				onscorechange={handleScoreChange}
			/>

			<!-- Actions -->
			{#if !isFinalized()}
				<div class="flex gap-4 justify-end bg-white rounded-lg shadow-sm border border-gray-200 p-6">
					<button
						type="button"
						onclick={() => handleSave(false)}
						disabled={isSaving || !hasUnsavedChanges()}
						class="px-6 py-2 border border-gray-300 rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed font-medium"
					>
						{isSaving ? 'Saving...' : 'Save Draft'}
					</button>
					<button
						type="button"
						onclick={() => handleSave(true)}
						disabled={isSaving || !allScoresEntered()}
						class="px-6 py-2 border border-transparent rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed font-medium"
					>
						{isSaving ? 'Finalizing...' : 'Finalize Scores'}
					</button>
				</div>
			{/if}
		</div>
	{:else}
		<div class="bg-yellow-50 border border-yellow-200 rounded-lg p-6">
			<p class="text-yellow-800">No responses found for this student.</p>
		</div>
	{/if}
</div>

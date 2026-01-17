<script lang="ts">
	import { onMount } from 'svelte';
	import { questionnaireStore } from '$lib/stores/questionnaire';
	import QuestionRenderer from '$lib/components/QuestionRenderer.svelte';
	import StepNavigation from '$lib/components/StepNavigation.svelte';
	import SubmissionConfirmation from '$lib/components/SubmissionConfirmation.svelte';

	// Destructure derived stores from questionnaireStore
	const { questionsForCurrentStep, totalSteps, canNavigateNext, isLastStep, isSubmitted } = questionnaireStore;

	let showSubmissionModal = $state(false);
	let submittedAt = $state<string | undefined>(undefined);

	onMount(async () => {
		await questionnaireStore.loadQuestionnaire();

		// If not already submitted, start timing
		if (!$isSubmitted) {
			await questionnaireStore.startQuestionnaire();
		}
	});

	function handleAnswerChange(questionId: string, value: string) {
		questionnaireStore.setAnswer(questionId, value);
	}

	async function handleNext() {
		await questionnaireStore.nextStep();
	}

	async function handlePrevious() {
		await questionnaireStore.previousStep();
	}

	async function handleSubmit() {
		const success = await questionnaireStore.submitQuestionnaire();
		if (success) {
			submittedAt = new Date().toISOString();
			showSubmissionModal = true;
		}
	}
</script>

<div class="max-w-4xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
	{#if $questionnaireStore.isLoading}
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
					/>
					<path
						class="opacity-75"
						fill="currentColor"
						d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
					/>
				</svg>
				<p class="text-gray-600">Loading questionnaire...</p>
			</div>
		</div>
	{:else if $questionnaireStore.error}
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
					/>
				</svg>
				<div>
					<h3 class="text-red-800 font-semibold mb-1">Error</h3>
					<p class="text-red-700">{$questionnaireStore.error}</p>
				</div>
			</div>
		</div>
	{:else if $questionnaireStore.questionnaire}
		<div class="space-y-6">
			<!-- Header -->
			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h1 class="text-3xl font-bold text-gray-900 mb-2">
					{$questionnaireStore.questionnaire.title}
				</h1>
				{#if $questionnaireStore.questionnaire.description}
					<p class="text-gray-600">{$questionnaireStore.questionnaire.description}</p>
				{/if}

				{#if $isSubmitted}
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
								/>
							</svg>
							<span class="text-green-800 font-medium">This questionnaire has been submitted</span>
						</div>
					</div>
				{/if}

				{#if $questionnaireStore.lastSaved && !$isSubmitted}
					<p class="mt-4 text-sm text-gray-500">
						Last saved: {$questionnaireStore.lastSaved.toLocaleTimeString()}
					</p>
				{/if}
			</div>

			<!-- Questions for current step -->
			<div class="space-y-4">
				{#each $questionsForCurrentStep as question (question.id)}
					<QuestionRenderer
						{question}
						value={$questionnaireStore.answers[question.id] || ''}
						disabled={$isSubmitted}
						onchange={(value) => handleAnswerChange(question.id, value)}
					/>
				{/each}
			</div>

			<!-- Navigation -->
			{#if !$isSubmitted}
				<StepNavigation
					currentStep={$questionnaireStore.currentStep}
					totalSteps={$totalSteps}
					canNavigateNext={$canNavigateNext}
					isLastStep={$isLastStep}
					isSaving={$questionnaireStore.isSaving}
					onPrevious={handlePrevious}
					onNext={handleNext}
					onSubmit={handleSubmit}
				/>
			{/if}
		</div>
	{:else}
		<div class="bg-yellow-50 border border-yellow-200 rounded-lg p-6">
			<p class="text-yellow-800">No active pre-test questionnaire found.</p>
		</div>
	{/if}
</div>

<SubmissionConfirmation show={showSubmissionModal} {submittedAt} />

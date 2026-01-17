<script lang="ts">
	import { onMount } from 'svelte';
	import { studentApi, type QuestionnaireWithAnswers } from '$lib/api/student';
	import QuestionRenderer from '$lib/components/QuestionRenderer.svelte';
	import StepNavigation from '$lib/components/StepNavigation.svelte';
	import SubmissionConfirmation from '$lib/components/SubmissionConfirmation.svelte';

	let questionnaire = $state<QuestionnaireWithAnswers | null>(null);
	let answers = $state<Record<string, string>>({});
	let currentStep = $state(1);
	let isLoading = $state(true);
	let isSaving = $state(false);
	let error = $state<string | null>(null);
	let lastSaved = $state<Date | null>(null);
	let showSubmissionModal = $state(false);
	let submittedAt = $state<string | undefined>(undefined);

	let autoSaveTimeout: ReturnType<typeof setTimeout> | null = null;
	let stepStartTime: Date | null = null;

	const questionsForCurrentStep = $derived(
		questionnaire ? questionnaire.questions.filter(q => q.step === currentStep) : []
	);

	const totalSteps = $derived(questionnaire?.totalSteps ?? 0);

	const canNavigateNext = $derived(() => {
		const currentQuestions = questionsForCurrentStep;
		const requiredQuestions = currentQuestions.filter(q => q.isRequired);
		return requiredQuestions.every(q => answers[q.id] && answers[q.id].trim() !== '');
	});

	const isLastStep = $derived(currentStep === totalSteps);
	const isSubmitted = $derived(questionnaire?.isSubmitted ?? false);

	onMount(async () => {
		await loadQuestionnaire();

		if (!isSubmitted && questionnaire) {
			stepStartTime = new Date();
			await studentApi.recordStepTiming(currentStep, true);
		}
	});

	async function loadQuestionnaire() {
		isLoading = true;
		error = null;

		try {
			const data = await studentApi.getPosttest();
			questionnaire = data;

			const answerMap: Record<string, string> = {};
			data.answers.forEach(a => {
				answerMap[a.questionId] = a.value ?? '';
			});
			answers = answerMap;
		} catch (err) {
			if (err instanceof Error && err.message.includes('403')) {
				error = 'Post-test is not currently available or you have not completed the pre-test.';
			} else {
				error = err instanceof Error ? err.message : 'Failed to load questionnaire';
			}
		} finally {
			isLoading = false;
		}
	}

	function handleAnswerChange(questionId: string, value: string) {
		if (isSubmitted) return;

		answers = { ...answers, [questionId]: value };

		if (autoSaveTimeout) {
			clearTimeout(autoSaveTimeout);
		}

		autoSaveTimeout = setTimeout(() => {
			saveAnswers();
		}, 1500);
	}

	async function saveAnswers() {
		if (isSubmitted || isSaving) return;

		isSaving = true;
		error = null;

		try {
			await studentApi.savePosttestAnswers(answers);
			lastSaved = new Date();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to save answers';
		} finally {
			isSaving = false;
		}
	}

	async function handleNext() {
		if (!canNavigateNext() || isLastStep) return;

		if (stepStartTime) {
			await studentApi.recordStepTiming(currentStep, false);
		}

		await saveAnswers();

		currentStep += 1;

		stepStartTime = new Date();
		await studentApi.recordStepTiming(currentStep, true);
	}

	async function handlePrevious() {
		if (currentStep <= 1) return;

		if (stepStartTime) {
			await studentApi.recordStepTiming(currentStep, false);
		}

		currentStep -= 1;

		stepStartTime = new Date();
		await studentApi.recordStepTiming(currentStep, true);
	}

	async function handleSubmit() {
		if (isSubmitted) return;

		if (stepStartTime) {
			await studentApi.recordStepTiming(currentStep, false);
		}

		await saveAnswers();

		isSaving = true;
		error = null;

		try {
			const result = await studentApi.submitPosttest();
			submittedAt = result.submittedAt;
			showSubmissionModal = true;
			await loadQuestionnaire();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to submit questionnaire';
		} finally {
			isSaving = false;
		}
	}
</script>

<div class="max-w-4xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
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
				<p class="text-gray-600">Loading questionnaire...</p>
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
	{:else if questionnaire}
		<div class="space-y-6">
			<!-- Header -->
			<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
				<h1 class="text-3xl font-bold text-gray-900 mb-2">
					{questionnaire.title}
				</h1>
				{#if questionnaire.description}
					<p class="text-gray-600">{questionnaire.description}</p>
				{/if}

				{#if isSubmitted}
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
							<span class="text-green-800 font-medium">This questionnaire has been submitted</span>
						</div>
					</div>
				{/if}

				{#if lastSaved && !isSubmitted}
					<p class="mt-4 text-sm text-gray-500">
						Last saved: {lastSaved.toLocaleTimeString()}
					</p>
				{/if}
			</div>

			<!-- Questions for current step -->
			<div class="space-y-4">
				{#each questionsForCurrentStep as question (question.id)}
					<QuestionRenderer
						{question}
						value={answers[question.id] || ''}
						disabled={isSubmitted}
						onchange={(value) => handleAnswerChange(question.id, value)}
					/>
				{/each}
			</div>

			<!-- Navigation -->
			{#if !isSubmitted}
				<StepNavigation
					{currentStep}
					{totalSteps}
					canNavigateNext={canNavigateNext()}
					{isLastStep}
					{isSaving}
					onPrevious={handlePrevious}
					onNext={handleNext}
					onSubmit={handleSubmit}
				/>
			{/if}
		</div>
	{:else}
		<div class="bg-yellow-50 border border-yellow-200 rounded-lg p-6">
			<p class="text-yellow-800">No active post-test questionnaire found.</p>
		</div>
	{/if}
</div>

<SubmissionConfirmation show={showSubmissionModal} {submittedAt} />

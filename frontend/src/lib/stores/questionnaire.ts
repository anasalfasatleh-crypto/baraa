import { writable, derived, get } from 'svelte/store';
import { studentApi, type QuestionnaireWithAnswers, type Question } from '$lib/api/student';

interface QuestionnaireState {
	questionnaire: QuestionnaireWithAnswers | null;
	answers: Record<string, string>;
	currentStep: number;
	isLoading: boolean;
	isSaving: boolean;
	error: string | null;
	lastSaved: Date | null;
}

function createQuestionnaireStore() {
	const state = writable<QuestionnaireState>({
		questionnaire: null,
		answers: {},
		currentStep: 1,
		isLoading: false,
		isSaving: false,
		error: null,
		lastSaved: null
	});

	let autoSaveTimeout: ReturnType<typeof setTimeout> | null = null;
	let stepStartTime: Date | null = null;

	// Derived stores
	const questionsForCurrentStep = derived(state, ($state) => {
		if (!$state.questionnaire) return [];
		return $state.questionnaire.questions.filter(q => q.step === $state.currentStep);
	});

	const totalSteps = derived(state, ($state) => {
		return $state.questionnaire?.totalSteps ?? 0;
	});

	const canNavigateNext = derived([state, questionsForCurrentStep], ([$state, $questions]) => {
		const requiredQuestions = $questions.filter(q => q.isRequired);
		return requiredQuestions.every(q => $state.answers[q.id] && $state.answers[q.id].trim() !== '');
	});

	const isLastStep = derived([state, totalSteps], ([$state, $totalSteps]) => {
		return $state.currentStep === $totalSteps;
	});

	const isSubmitted = derived(state, ($state) => {
		return $state.questionnaire?.isSubmitted ?? false;
	});

	async function loadQuestionnaire() {
		state.update(s => ({ ...s, isLoading: true, error: null }));

		try {
			const data = await studentApi.getPretest();

			// Load existing answers
			const answerMap: Record<string, string> = {};
			data.answers.forEach(a => {
				answerMap[a.questionId] = a.value;
			});

			state.update(s => ({
				...s,
				questionnaire: data,
				answers: answerMap,
				isLoading: false
			}));
		} catch (err) {
			state.update(s => ({
				...s,
				error: err instanceof Error ? err.message : 'Failed to load questionnaire',
				isLoading: false
			}));
		}
	}

	function setAnswer(questionId: string, value: string) {
		const currentState = get(state);
		if (currentState.questionnaire?.isSubmitted) {
			return; // Cannot modify submitted questionnaire
		}

		state.update(s => ({
			...s,
			answers: { ...s.answers, [questionId]: value }
		}));

		// Trigger auto-save with debounce
		if (autoSaveTimeout) {
			clearTimeout(autoSaveTimeout);
		}

		autoSaveTimeout = setTimeout(() => {
			saveAnswers();
		}, 1500); // Save 1.5 seconds after user stops typing
	}

	async function saveAnswers() {
		const currentState = get(state);
		if (currentState.questionnaire?.isSubmitted || currentState.isSaving) {
			return;
		}

		state.update(s => ({ ...s, isSaving: true, error: null }));

		try {
			await studentApi.savePretestAnswers(currentState.answers);
			state.update(s => ({ ...s, lastSaved: new Date(), isSaving: false }));
		} catch (err) {
			state.update(s => ({
				...s,
				error: err instanceof Error ? err.message : 'Failed to save answers',
				isSaving: false
			}));
		}
	}

	async function nextStep() {
		const currentState = get(state);
		const $canNavigateNext = get(canNavigateNext);
		const $isLastStep = get(isLastStep);

		if (!$canNavigateNext || $isLastStep) {
			return;
		}

		// Record step end time
		if (stepStartTime) {
			await studentApi.recordStepTiming(currentState.currentStep, false);
		}

		// Force save before moving to next step
		await saveAnswers();

		state.update(s => ({ ...s, currentStep: s.currentStep + 1 }));

		// Record step start time
		stepStartTime = new Date();
		await studentApi.recordStepTiming(currentState.currentStep + 1, true);
	}

	async function previousStep() {
		const currentState = get(state);

		if (currentState.currentStep <= 1) {
			return;
		}

		// Record step end time
		if (stepStartTime) {
			await studentApi.recordStepTiming(currentState.currentStep, false);
		}

		state.update(s => ({ ...s, currentStep: s.currentStep - 1 }));

		// Record step start time
		stepStartTime = new Date();
		await studentApi.recordStepTiming(currentState.currentStep - 1, true);
	}

	async function goToStep(step: number) {
		const currentState = get(state);
		const $totalSteps = get(totalSteps);

		if (step < 1 || step > $totalSteps || step === currentState.currentStep) {
			return;
		}

		// Record step end time
		if (stepStartTime) {
			await studentApi.recordStepTiming(currentState.currentStep, false);
		}

		state.update(s => ({ ...s, currentStep: step }));

		// Record step start time
		stepStartTime = new Date();
		await studentApi.recordStepTiming(step, true);
	}

	async function startQuestionnaire() {
		const currentState = get(state);
		stepStartTime = new Date();
		await studentApi.recordStepTiming(currentState.currentStep, true);
	}

	async function submitQuestionnaire(): Promise<boolean> {
		const currentState = get(state);
		if (currentState.questionnaire?.isSubmitted) {
			return false;
		}

		// Record final step end time
		if (stepStartTime) {
			await studentApi.recordStepTiming(currentState.currentStep, false);
		}

		// Force save all answers before submission
		await saveAnswers();

		state.update(s => ({ ...s, isSaving: true, error: null }));

		try {
			await studentApi.submitPretest();

			// Reload questionnaire to get updated submission status
			await loadQuestionnaire();

			return true;
		} catch (err) {
			state.update(s => ({
				...s,
				error: err instanceof Error ? err.message : 'Failed to submit questionnaire',
				isSaving: false
			}));
			return false;
		}
	}

	function reset() {
		state.set({
			questionnaire: null,
			answers: {},
			currentStep: 1,
			isLoading: false,
			isSaving: false,
			error: null,
			lastSaved: null
		});
		stepStartTime = null;

		if (autoSaveTimeout) {
			clearTimeout(autoSaveTimeout);
			autoSaveTimeout = null;
		}
	}

	return {
		subscribe: state.subscribe,
		questionsForCurrentStep,
		totalSteps,
		canNavigateNext,
		isLastStep,
		isSubmitted,
		loadQuestionnaire,
		setAnswer,
		saveAnswers,
		nextStep,
		previousStep,
		goToStep,
		startQuestionnaire,
		submitQuestionnaire,
		reset
	};
}

export const questionnaireStore = createQuestionnaireStore();

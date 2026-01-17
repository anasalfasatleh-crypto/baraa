<script lang="ts">
	import {
		adminApi,
		type QuestionnaireDetail,
		type QuestionUpdateItem,
		type UpdateQuestionnaireRequest,
		type UpdateQuestionsRequest
	} from '$lib/api/admin';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import { Button } from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import * as Alert from '$lib/components/ui/alert';
	import { Input } from '$lib/components/ui/input';
	import { Label } from '$lib/components/ui/label';
	import { Textarea } from '$lib/components/ui/textarea';
	import { Checkbox } from '$lib/components/ui/checkbox';
	import * as Select from '$lib/components/ui/select';
	import QuestionEditor from '$lib/components/builder/QuestionEditor.svelte';
	import {
		ArrowLeft,
		AlertCircle,
		CheckCircle2,
		Loader2,
		Plus,
		HelpCircle
	} from '@lucide/svelte';

	const id = $derived($page.params.id);

	let questionnaire = $state<QuestionnaireDetail | null>(null);
	let questions = $state<QuestionUpdateItem[]>([]);
	let loading = $state(true);
	let saving = $state(false);
	let error = $state<string | null>(null);
	let successMessage = $state<string | null>(null);

	// Questionnaire metadata
	let title = $state('');
	let description = $state('');
	let type = $state<'Pretest' | 'Posttest'>('Pretest');
	let isActive = $state(false);

	const typeOptions = [
		{ value: 'Pretest', label: 'Pre-test' },
		{ value: 'Posttest', label: 'Post-test' }
	];

	const selectedTypeLabel = $derived(typeOptions.find((t) => t.value === type)?.label ?? 'Select type...');

	onMount(async () => {
		await loadQuestionnaire();
	});

	async function loadQuestionnaire() {
		if (!id) {
			error = 'Invalid questionnaire ID';
			loading = false;
			return;
		}
		loading = true;
		error = null;
		try {
			questionnaire = await adminApi.getQuestionnaire(id);
			title = questionnaire.title;
			description = questionnaire.description || '';
			type = questionnaire.type;
			isActive = questionnaire.isActive;
			questions = questionnaire.questions.map((q) => ({
				id: q.id,
				text: q.text,
				type: q.type,
				options: q.options,
				orderIndex: q.orderIndex,
				step: q.step,
				isRequired: q.isRequired,
				minValue: q.minValue,
				maxValue: q.maxValue,
				minLabel: q.minLabel,
				maxLabel: q.maxLabel
			}));
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load questionnaire';
		} finally {
			loading = false;
		}
	}

	async function handleSaveMetadata() {
		if (!id) return;
		saving = true;
		error = null;
		successMessage = null;
		try {
			const request: UpdateQuestionnaireRequest = {
				title,
				description: description || undefined,
				type,
				isActive
			};
			await adminApi.updateQuestionnaire(id, request);
			successMessage = 'Questionnaire metadata saved successfully';
			await loadQuestionnaire();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to save questionnaire';
		} finally {
			saving = false;
		}
	}

	async function handleSaveQuestions() {
		if (!id) return;
		saving = true;
		error = null;
		successMessage = null;

		// Validate questions
		const emptyQuestions = questions.filter((q) => !q.text.trim());
		if (emptyQuestions.length > 0) {
			error = 'All questions must have text';
			saving = false;
			return;
		}

		// Validate questions that need options
		const questionsNeedingOptions = questions.filter(
			(q) =>
				(q.type === 'MultipleChoice' || q.type === 'DropdownGrade') &&
				(!q.options || q.options.length === 0)
		);
		if (questionsNeedingOptions.length > 0) {
			error = 'Multiple choice and dropdown questions must have at least one option';
			saving = false;
			return;
		}

		try {
			const request: UpdateQuestionsRequest = {
				questions
			};
			await adminApi.updateQuestions(id, request);
			successMessage = 'Questions saved successfully';
			await loadQuestionnaire();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to save questions';
		} finally {
			saving = false;
		}
	}

	function addQuestion() {
		const newQuestion: QuestionUpdateItem = {
			text: '',
			type: 'LikertScale',
			orderIndex: questions.length + 1,
			step: 1,
			isRequired: true
		};
		questions = [...questions, newQuestion];
	}

	function updateQuestion(index: number, updated: QuestionUpdateItem) {
		questions = questions.map((q, i) => (i === index ? updated : q));
	}

	function deleteQuestion(index: number) {
		questions = questions.filter((_, i) => i !== index);
		// Reorder
		questions = questions.map((q, i) => ({ ...q, orderIndex: i + 1 }));
	}

	function moveQuestionUp(index: number) {
		if (index === 0) return;
		const newQuestions = [...questions];
		[newQuestions[index - 1], newQuestions[index]] = [newQuestions[index], newQuestions[index - 1]];
		questions = newQuestions.map((q, i) => ({ ...q, orderIndex: i + 1 }));
	}

	function moveQuestionDown(index: number) {
		if (index === questions.length - 1) return;
		const newQuestions = [...questions];
		[newQuestions[index], newQuestions[index + 1]] = [newQuestions[index + 1], newQuestions[index]];
		questions = newQuestions.map((q, i) => ({ ...q, orderIndex: i + 1 }));
	}

	function handleCancel() {
		goto('/admin/questionnaires');
	}
</script>

<div class="space-y-6">
	<!-- Header -->
	<div>
		<Button variant="ghost" onclick={handleCancel} class="mb-4 -ml-2">
			<ArrowLeft class="mr-2 h-4 w-4" />
			Back to Questionnaires
		</Button>
		<div class="flex items-center justify-between">
			<div>
				<h1 class="text-3xl font-bold tracking-tight">Edit Questionnaire</h1>
				{#if questionnaire}
					<p class="text-muted-foreground">
						{questionnaire.questions.length} questions
					</p>
				{/if}
			</div>
		</div>
	</div>

	<!-- Success Message -->
	{#if successMessage}
		<Alert.Root>
			<CheckCircle2 class="h-4 w-4" />
			<Alert.Title>Success</Alert.Title>
			<Alert.Description class="flex items-center justify-between">
				<span>{successMessage}</span>
				<Button variant="ghost" size="sm" onclick={() => (successMessage = null)}>
					Dismiss
				</Button>
			</Alert.Description>
		</Alert.Root>
	{/if}

	<!-- Error Message -->
	{#if error}
		<Alert.Root variant="destructive">
			<AlertCircle class="h-4 w-4" />
			<Alert.Title>Error</Alert.Title>
			<Alert.Description>{error}</Alert.Description>
		</Alert.Root>
	{/if}

	{#if loading}
		<Card.Root>
			<Card.Content class="flex flex-col items-center justify-center py-12">
				<Loader2 class="h-8 w-8 animate-spin text-primary" />
				<p class="mt-2 text-sm text-muted-foreground">Loading questionnaire...</p>
			</Card.Content>
		</Card.Root>
	{:else}
		<!-- Questionnaire Metadata -->
		<Card.Root>
			<Card.Header>
				<Card.Title>Questionnaire Details</Card.Title>
			</Card.Header>
			<Card.Content class="space-y-4">
				<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
					<div class="space-y-2">
						<Label for="title">
							Title <span class="text-destructive">*</span>
						</Label>
						<Input id="title" bind:value={title} disabled={saving} />
					</div>
					<div class="space-y-2">
						<Label>
							Type <span class="text-destructive">*</span>
						</Label>
						<Select.Root
							type="single"
							value={type}
							onValueChange={(v) => {
								if (v) type = v as 'Pretest' | 'Posttest';
							}}
							disabled={saving}
						>
							<Select.Trigger class="w-full">
								{selectedTypeLabel}
							</Select.Trigger>
							<Select.Content>
								{#each typeOptions as option}
									<Select.Item value={option.value} label={option.label}>
										{option.label}
									</Select.Item>
								{/each}
							</Select.Content>
						</Select.Root>
					</div>
				</div>

				<div class="space-y-2">
					<Label for="description">Description</Label>
					<Textarea id="description" bind:value={description} rows={2} disabled={saving} />
				</div>

				<div class="flex items-center space-x-2">
					<Checkbox id="isActive" bind:checked={isActive} disabled={saving} />
					<Label for="isActive" class="text-sm font-normal">
						Active (students can access this questionnaire)
					</Label>
				</div>

				<div class="flex justify-end pt-4 border-t">
					<Button onclick={handleSaveMetadata} disabled={saving}>
						{#if saving}
							<Loader2 class="mr-2 h-4 w-4 animate-spin" />
							Saving...
						{:else}
							Save Details
						{/if}
					</Button>
				</div>
			</Card.Content>
		</Card.Root>

		<!-- Questions Section -->
		<Card.Root>
			<Card.Header class="flex flex-row items-center justify-between space-y-0">
				<Card.Title>Questions</Card.Title>
				<Button onclick={addQuestion} disabled={saving} size="sm">
					<Plus class="mr-2 h-4 w-4" />
					Add Question
				</Button>
			</Card.Header>
			<Card.Content>
				{#if questions.length === 0}
					<div class="flex flex-col items-center justify-center py-12">
						<HelpCircle class="h-12 w-12 text-muted-foreground" />
						<h3 class="mt-4 text-lg font-semibold">No questions yet</h3>
						<p class="mt-1 text-sm text-muted-foreground">
							Get started by adding your first question.
						</p>
						<Button class="mt-6" onclick={addQuestion} disabled={saving}>
							<Plus class="mr-2 h-4 w-4" />
							Add First Question
						</Button>
					</div>
				{:else}
					<div class="space-y-4">
						{#each questions as question, index}
							<QuestionEditor
								{question}
								{index}
								totalQuestions={questions.length}
								onUpdate={(updated) => updateQuestion(index, updated)}
								onDelete={() => deleteQuestion(index)}
								onMoveUp={() => moveQuestionUp(index)}
								onMoveDown={() => moveQuestionDown(index)}
								disabled={saving}
							/>
						{/each}
					</div>

					<div class="flex justify-end gap-3 pt-6 border-t mt-6">
						<Button variant="outline" onclick={handleCancel} disabled={saving}>
							Cancel
						</Button>
						<Button onclick={handleSaveQuestions} disabled={saving}>
							{#if saving}
								<Loader2 class="mr-2 h-4 w-4 animate-spin" />
								Saving...
							{:else}
								Save Questions
							{/if}
						</Button>
					</div>
				{/if}
			</Card.Content>
		</Card.Root>
	{/if}
</div>

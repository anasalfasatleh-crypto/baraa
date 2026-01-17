<script lang="ts">
	import type { QuestionUpdateItem } from '$lib/api/admin';
	import { Button } from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { Label } from '$lib/components/ui/label';
	import { Textarea } from '$lib/components/ui/textarea';
	import { Checkbox } from '$lib/components/ui/checkbox';
	import { Badge } from '$lib/components/ui/badge';
	import QuestionTypeSelector from './QuestionTypeSelector.svelte';
	import OptionsEditor from './OptionsEditor.svelte';
	import { Trash2, ChevronUp, ChevronDown } from '@lucide/svelte';

	interface Props {
		question: QuestionUpdateItem;
		index: number;
		totalQuestions: number;
		onUpdate: (question: QuestionUpdateItem) => void;
		onDelete: () => void;
		onMoveUp: () => void;
		onMoveDown: () => void;
		disabled?: boolean;
	}

	let {
		question,
		index,
		totalQuestions,
		onUpdate,
		onDelete,
		onMoveUp,
		onMoveDown,
		disabled = false
	}: Props = $props();

	function updateField<K extends keyof QuestionUpdateItem>(
		field: K,
		value: QuestionUpdateItem[K]
	) {
		onUpdate({ ...question, [field]: value });
	}

	const needsOptions = $derived(
		question.type === 'MultipleChoice' || question.type === 'DropdownGrade'
	);

	const needsLikertSettings = $derived(question.type === 'LikertScale');
</script>

<Card.Root>
	<Card.Header class="pb-3">
		<div class="flex items-center justify-between">
			<div class="flex items-center gap-3">
				<Badge variant="secondary" class="h-8 w-8 rounded-full p-0 flex items-center justify-center">
					{question.orderIndex}
				</Badge>
				<div class="flex flex-col">
					<span class="text-sm font-medium">Question {question.orderIndex}</span>
					<span class="text-xs text-muted-foreground">Step {question.step}</span>
				</div>
			</div>
			<div class="flex items-center gap-1">
				<Button
					variant="ghost"
					size="icon"
					onclick={onMoveUp}
					disabled={index === 0 || disabled}
					title="Move up"
				>
					<ChevronUp class="h-4 w-4" />
				</Button>
				<Button
					variant="ghost"
					size="icon"
					onclick={onMoveDown}
					disabled={index === totalQuestions - 1 || disabled}
					title="Move down"
				>
					<ChevronDown class="h-4 w-4" />
				</Button>
				<Button
					variant="ghost"
					size="icon"
					onclick={onDelete}
					{disabled}
					class="text-destructive hover:text-destructive"
					title="Delete question"
				>
					<Trash2 class="h-4 w-4" />
				</Button>
			</div>
		</div>
	</Card.Header>
	<Card.Content class="space-y-4">
		<!-- Question Text -->
		<div class="space-y-2">
			<Label for="text-{question.orderIndex}">
				Question Text <span class="text-destructive">*</span>
			</Label>
			<Textarea
				id="text-{question.orderIndex}"
				value={question.text}
				oninput={(e) => updateField('text', e.currentTarget.value)}
				rows={2}
				{disabled}
				placeholder="Enter the question..."
			/>
		</div>

		<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
			<!-- Question Type -->
			<div class="space-y-2">
				<Label>Question Type <span class="text-destructive">*</span></Label>
				<QuestionTypeSelector
					value={question.type}
					onChange={(value) => updateField('type', value as QuestionUpdateItem['type'])}
					{disabled}
				/>
			</div>

			<!-- Required Toggle -->
			<div class="flex items-center space-x-2 pt-8">
				<Checkbox
					id="required-{question.orderIndex}"
					checked={question.isRequired}
					onCheckedChange={(checked) => updateField('isRequired', !!checked)}
					{disabled}
				/>
				<Label for="required-{question.orderIndex}" class="text-sm font-normal">
					Required question
				</Label>
			</div>
		</div>

		<!-- Likert Scale Settings -->
		{#if needsLikertSettings}
			<Card.Root class="bg-muted/50">
				<Card.Header class="pb-2">
					<Card.Title class="text-sm">Likert Scale Settings</Card.Title>
				</Card.Header>
				<Card.Content class="space-y-3">
					<div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
						<div class="space-y-2">
							<Label for="minValue-{question.orderIndex}">Minimum Value</Label>
							<Input
								type="number"
								id="minValue-{question.orderIndex}"
								value={question.minValue ?? 1}
								oninput={(e) => updateField('minValue', parseInt(e.currentTarget.value))}
								{disabled}
							/>
						</div>
						<div class="space-y-2">
							<Label for="maxValue-{question.orderIndex}">Maximum Value</Label>
							<Input
								type="number"
								id="maxValue-{question.orderIndex}"
								value={question.maxValue ?? 5}
								oninput={(e) => updateField('maxValue', parseInt(e.currentTarget.value))}
								{disabled}
							/>
						</div>
						<div class="space-y-2">
							<Label for="minLabel-{question.orderIndex}">Minimum Label</Label>
							<Input
								type="text"
								id="minLabel-{question.orderIndex}"
								value={question.minLabel ?? ''}
								oninput={(e) => updateField('minLabel', e.currentTarget.value)}
								placeholder="e.g., Strongly Disagree"
								{disabled}
							/>
						</div>
						<div class="space-y-2">
							<Label for="maxLabel-{question.orderIndex}">Maximum Label</Label>
							<Input
								type="text"
								id="maxLabel-{question.orderIndex}"
								value={question.maxLabel ?? ''}
								oninput={(e) => updateField('maxLabel', e.currentTarget.value)}
								placeholder="e.g., Strongly Agree"
								{disabled}
							/>
						</div>
					</div>
				</Card.Content>
			</Card.Root>
		{/if}

		<!-- Options for Multiple Choice/Dropdown -->
		{#if needsOptions}
			<Card.Root class="bg-muted/50">
				<Card.Content class="pt-4">
					<OptionsEditor
						options={question.options ?? []}
						onChange={(options) => updateField('options', options)}
						{disabled}
					/>
				</Card.Content>
			</Card.Root>
		{/if}

		<!-- Step Assignment -->
		<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
			<div class="space-y-2">
				<Label for="step-{question.orderIndex}">
					Step Number <span class="text-destructive">*</span>
				</Label>
				<Input
					type="number"
					id="step-{question.orderIndex}"
					value={question.step}
					oninput={(e) => updateField('step', parseInt(e.currentTarget.value))}
					min={1}
					{disabled}
				/>
				<p class="text-xs text-muted-foreground">Which page/step this question appears on</p>
			</div>
		</div>
	</Card.Content>
</Card.Root>

<script lang="ts">
	import * as Select from '$lib/components/ui/select';

	interface Props {
		value: string;
		onChange: (value: string) => void;
		disabled?: boolean;
	}

	let { value, onChange, disabled = false }: Props = $props();

	const questionTypes = [
		{ value: 'LikertScale', label: 'Likert Scale (1-5)', description: 'Rating scale from 1 to 5' },
		{
			value: 'TrueFalseDontKnow',
			label: "True/False/I Don't Know",
			description: 'Three-option choice question'
		},
		{
			value: 'MultipleChoice',
			label: 'Multiple Choice',
			description: 'Single answer from multiple options'
		},
		{
			value: 'DropdownGrade',
			label: 'Dropdown Grade',
			description: 'Grade selection dropdown'
		},
		{ value: 'ShortText', label: 'Short Text', description: 'Single-line text input' },
		{ value: 'LongText', label: 'Long Text', description: 'Multi-line text area' },
		{
			value: 'EvaluatorOnly',
			label: 'Evaluator Only',
			description: 'Manual scoring by evaluator'
		}
	];

	const selectedType = $derived(questionTypes.find((t) => t.value === value));
	const selectedLabel = $derived(selectedType?.label ?? 'Select question type...');
</script>

<div class="space-y-1">
	<Select.Root
		type="single"
		{value}
		onValueChange={(v) => {
			if (v) onChange(v);
		}}
		{disabled}
	>
		<Select.Trigger class="w-full">
			{selectedLabel}
		</Select.Trigger>
		<Select.Content>
			{#each questionTypes as type}
				<Select.Item value={type.value} label={type.label}>
					{type.label}
				</Select.Item>
			{/each}
		</Select.Content>
	</Select.Root>

	<!-- Show description for selected type -->
	{#if selectedType}
		<p class="text-sm text-muted-foreground">{selectedType.description}</p>
	{/if}
</div>

<script lang="ts">
	import type { Question } from '$lib/api/student';

	interface Props {
		question: Question;
		value: string;
		disabled?: boolean;
		onchange: (value: string) => void;
	}

	let { question, value = '', disabled = false, onchange }: Props = $props();

	const minValue = $derived(question.minValue ?? 1);
	const maxValue = $derived(question.maxValue ?? 5);
	const range = $derived(Array.from({ length: maxValue - minValue + 1 }, (_, i) => minValue + i));

	function handleChange(newValue: number) {
		if (!disabled) {
			onchange(newValue.toString());
		}
	}
</script>

<div class="space-y-4">
	<div class="flex justify-between items-center">
		<span class="text-sm text-gray-600">{question.minLabel || 'Strongly Disagree'}</span>
		<span class="text-sm text-gray-600">{question.maxLabel || 'Strongly Agree'}</span>
	</div>

	<div class="flex justify-between items-center gap-2">
		{#each range as option}
			<button
				type="button"
				class="flex-1 py-3 px-4 border-2 rounded-lg transition-all {value === option.toString()
					? 'border-indigo-600 bg-indigo-50 text-indigo-900 font-semibold'
					: 'border-gray-300 hover:border-indigo-400 hover:bg-gray-50'} disabled:opacity-50 disabled:cursor-not-allowed"
				onclick={() => handleChange(option)}
				{disabled}
			>
				{option}
			</button>
		{/each}
	</div>
</div>

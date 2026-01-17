<script lang="ts">
	import type { Question } from '$lib/api/student';
	import LikertScale from './questions/LikertScale.svelte';
	import TrueFalse from './questions/TrueFalse.svelte';
	import MultipleChoice from './questions/MultipleChoice.svelte';
	import Dropdown from './questions/Dropdown.svelte';
	import TextField from './questions/TextField.svelte';

	interface Props {
		question: Question;
		value: string;
		disabled?: boolean;
		onchange: (value: string) => void;
	}

	let { question, value = '', disabled = false, onchange }: Props = $props();
</script>

<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
	<div class="mb-4">
		<label class="block text-lg font-medium text-gray-900 mb-2">
			{question.text}
			{#if question.isRequired}
				<span class="text-red-500">*</span>
			{/if}
		</label>
	</div>

	{#if question.type === 'LikertScale'}
		<LikertScale {question} {value} {disabled} {onchange} />
	{:else if question.type === 'TrueFalse'}
		<TrueFalse {question} {value} {disabled} {onchange} />
	{:else if question.type === 'MultipleChoice'}
		<MultipleChoice {question} {value} {disabled} {onchange} />
	{:else if question.type === 'Dropdown'}
		<Dropdown {question} {value} {disabled} {onchange} />
	{:else if question.type === 'TextField'}
		<TextField {question} {value} {disabled} {onchange} />
	{/if}
</div>

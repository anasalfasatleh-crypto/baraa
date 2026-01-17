<script lang="ts">
	import type { Question } from '$lib/api/student';

	interface Props {
		question: Question;
		value: string;
		disabled?: boolean;
		onchange: (value: string) => void;
	}

	let { question, value = '', disabled = false, onchange }: Props = $props();

	const options = $derived(question.options ?? []);

	function handleChange(event: Event) {
		if (!disabled) {
			const target = event.target as HTMLSelectElement;
			onchange(target.value);
		}
	}
</script>

<select
	class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
	{value}
	onchange={handleChange}
	{disabled}
>
	<option value="">-- Select an option --</option>
	{#each options as option}
		<option value={option}>{option}</option>
	{/each}
</select>

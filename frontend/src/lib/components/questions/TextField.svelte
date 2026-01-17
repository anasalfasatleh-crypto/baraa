<script lang="ts">
	import type { Question } from '$lib/api/student';

	interface Props {
		question: Question;
		value: string;
		disabled?: boolean;
		onchange: (value: string) => void;
	}

	let { question, value = '', disabled = false, onchange }: Props = $props();

	function handleInput(event: Event) {
		if (!disabled) {
			const target = event.target as HTMLTextAreaElement;
			onchange(target.value);
		}
	}
</script>

<textarea
	class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed resize-y min-h-32"
	{value}
	oninput={handleInput}
	{disabled}
	placeholder="Enter your answer here..."
	rows="4"
></textarea>

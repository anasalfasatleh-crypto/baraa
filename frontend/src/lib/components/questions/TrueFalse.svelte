<script lang="ts">
	import type { Question } from '$lib/api/student';

	interface Props {
		question: Question;
		value: string;
		disabled?: boolean;
		onchange: (value: string) => void;
	}

	let { question, value = '', disabled = false, onchange }: Props = $props();

	function handleChange(newValue: string) {
		if (!disabled) {
			onchange(newValue);
		}
	}
</script>

<div class="flex gap-4">
	<button
		type="button"
		class="flex-1 py-3 px-6 border-2 rounded-lg transition-all {value === 'true'
			? 'border-green-600 bg-green-50 text-green-900 font-semibold'
			: 'border-gray-300 hover:border-green-400 hover:bg-gray-50'} disabled:opacity-50 disabled:cursor-not-allowed"
		onclick={() => handleChange('true')}
		{disabled}
	>
		True
	</button>

	<button
		type="button"
		class="flex-1 py-3 px-6 border-2 rounded-lg transition-all {value === 'false'
			? 'border-red-600 bg-red-50 text-red-900 font-semibold'
			: 'border-gray-300 hover:border-red-400 hover:bg-gray-50'} disabled:opacity-50 disabled:cursor-not-allowed"
		onclick={() => handleChange('false')}
		{disabled}
	>
		False
	</button>
</div>

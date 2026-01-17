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

	function handleChange(newValue: string) {
		if (!disabled) {
			onchange(newValue);
		}
	}
</script>

<div class="space-y-3">
	{#each options as option, index}
		<button
			type="button"
			class="w-full text-left py-3 px-4 border-2 rounded-lg transition-all {value === option
				? 'border-indigo-600 bg-indigo-50 text-indigo-900 font-semibold'
				: 'border-gray-300 hover:border-indigo-400 hover:bg-gray-50'} disabled:opacity-50 disabled:cursor-not-allowed"
			onclick={() => handleChange(option)}
			{disabled}
		>
			<div class="flex items-center gap-3">
				<span
					class="flex-shrink-0 w-6 h-6 rounded-full border-2 flex items-center justify-center {value ===
					option
						? 'border-indigo-600 bg-indigo-600'
						: 'border-gray-400'}"
				>
					{#if value === option}
						<svg
							class="w-4 h-4 text-white"
							fill="none"
							stroke="currentColor"
							viewBox="0 0 24 24"
						>
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M5 13l4 4L19 7"
							/>
						</svg>
					{/if}
				</span>
				<span>{option}</span>
			</div>
		</button>
	{/each}
</div>

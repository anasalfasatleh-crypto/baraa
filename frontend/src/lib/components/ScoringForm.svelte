<script lang="ts">
	import type { QuestionResponse } from '$lib/api/evaluator';

	interface Props {
		responses: QuestionResponse[];
		scores: Record<string, number>;
		disabled?: boolean;
		onscorechange: (questionId: string, score: number) => void;
	}

	let { responses, scores, disabled = false, onscorechange }: Props = $props();

	function handleScoreInput(questionId: string, value: string) {
		const numValue = parseFloat(value);
		if (!isNaN(numValue)) {
			onscorechange(questionId, numValue);
		}
	}
</script>

<div class="space-y-6">
	{#each responses as response (response.questionId)}
		<div class="bg-white rounded-lg border border-gray-200 p-5">
			<div class="mb-4">
				<div class="flex items-start justify-between mb-2">
					<h3 class="text-base font-medium text-gray-900">
						{response.questionText}
						{#if response.isRequired}
							<span class="text-red-500">*</span>
						{/if}
					</h3>
					{#if response.isScoreFinalized}
						<span
							class="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-green-100 text-green-800"
						>
							Finalized
						</span>
					{/if}
				</div>
				<p class="text-sm text-gray-500">
					Type: {response.questionType} | Step: {response.step}
				</p>
			</div>

			<div class="mb-4 bg-gray-50 rounded p-4">
				<label class="block text-sm font-medium text-gray-700 mb-1">Student Answer:</label>
				<p class="text-gray-900">
					{#if response.answer}
						{response.answer}
					{:else}
						<span class="text-gray-400 italic">No answer provided</span>
					{/if}
				</p>
			</div>

			<div>
				<label for="score-{response.questionId}" class="block text-sm font-medium text-gray-700 mb-2">
					Score:
				</label>
				<input
					id="score-{response.questionId}"
					type="number"
					step="0.01"
					min="0"
					max="100"
					value={scores[response.questionId] ?? ''}
					oninput={(e) => handleScoreInput(response.questionId, e.currentTarget.value)}
					disabled={disabled || response.isScoreFinalized}
					class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
					placeholder="Enter score (0-100)"
				/>
				{#if response.score !== undefined && response.score !== null}
					<p class="mt-1 text-sm text-gray-500">
						Current score: {response.score.toFixed(2)}
					</p>
				{/if}
			</div>
		</div>
	{/each}
</div>

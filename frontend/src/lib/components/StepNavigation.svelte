<script lang="ts">
	interface Props {
		currentStep: number;
		totalSteps: number;
		canNavigateNext: boolean;
		isLastStep: boolean;
		isSaving: boolean;
		onPrevious: () => void;
		onNext: () => void;
		onSubmit: () => void;
	}

	let {
		currentStep,
		totalSteps,
		canNavigateNext,
		isLastStep,
		isSaving,
		onPrevious,
		onNext,
		onSubmit
	}: Props = $props();
</script>

<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
	<!-- Progress bar -->
	<div class="mb-6">
		<div class="flex justify-between items-center mb-2">
			<span class="text-sm font-medium text-gray-700">
				Step {currentStep} of {totalSteps}
			</span>
			<span class="text-sm text-gray-500">
				{Math.round((currentStep / totalSteps) * 100)}% Complete
			</span>
		</div>
		<div class="w-full bg-gray-200 rounded-full h-2">
			<div
				class="bg-indigo-600 h-2 rounded-full transition-all duration-300"
				style="width: {(currentStep / totalSteps) * 100}%"
			></div>
		</div>
	</div>

	<!-- Step indicators -->
	<div class="flex justify-between items-center mb-6">
		{#each Array(totalSteps) as _, index}
			<div class="flex flex-col items-center">
				<div
					class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold transition-all {index +
						1 ===
					currentStep
						? 'bg-indigo-600 text-white'
						: index + 1 < currentStep
							? 'bg-green-500 text-white'
							: 'bg-gray-300 text-gray-600'}"
				>
					{#if index + 1 < currentStep}
						<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M5 13l4 4L19 7"
							/>
						</svg>
					{:else}
						{index + 1}
					{/if}
				</div>
			</div>
			{#if index < totalSteps - 1}
				<div
					class="flex-1 h-1 mx-2 rounded {index + 1 < currentStep ? 'bg-green-500' : 'bg-gray-300'}"
				></div>
			{/if}
		{/each}
	</div>

	<!-- Navigation buttons -->
	<div class="flex justify-between items-center gap-4">
		<button
			type="button"
			class="px-6 py-2 border-2 border-gray-300 rounded-lg font-medium text-gray-700 hover:bg-gray-50 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
			onclick={onPrevious}
			disabled={currentStep === 1 || isSaving}
		>
			Previous
		</button>

		<div class="text-sm text-gray-500">
			{#if isSaving}
				<span class="flex items-center gap-2">
					<svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
						<circle
							class="opacity-25"
							cx="12"
							cy="12"
							r="10"
							stroke="currentColor"
							stroke-width="4"
						/>
						<path
							class="opacity-75"
							fill="currentColor"
							d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
						/>
					</svg>
					Saving...
				</span>
			{/if}
		</div>

		{#if isLastStep}
			<button
				type="button"
				class="px-6 py-2 bg-green-600 text-white rounded-lg font-medium hover:bg-green-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
				onclick={onSubmit}
				disabled={!canNavigateNext || isSaving}
			>
				Submit
			</button>
		{:else}
			<button
				type="button"
				class="px-6 py-2 bg-indigo-600 text-white rounded-lg font-medium hover:bg-indigo-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
				onclick={onNext}
				disabled={!canNavigateNext || isSaving}
			>
				Next
			</button>
		{/if}
	</div>

	{#if !canNavigateNext}
		<p class="mt-4 text-sm text-red-600 text-center">
			Please answer all required questions before proceeding
		</p>
	{/if}
</div>

<script lang="ts">
	import { goto } from '$app/navigation';

	interface Props {
		show: boolean;
		submittedAt?: string;
		onclose?: () => void;
	}

	let { show, submittedAt, onclose }: Props = $props();

	function handleClose() {
		if (onclose) {
			onclose();
		} else {
			goto('/student');
		}
	}

	function formatDateTime(dateString: string | undefined) {
		if (!dateString) return '';
		const date = new Date(dateString);
		return date.toLocaleString('en-US', {
			year: 'numeric',
			month: 'long',
			day: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}
</script>

{#if show}
	<div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
		<div class="bg-white rounded-lg shadow-xl max-w-md w-full p-8 transition-all duration-200">
			<div class="text-center">
				<!-- Success icon -->
				<div class="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-green-100 mb-4">
					<svg
						class="h-10 w-10 text-green-600"
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
				</div>

				<!-- Title -->
				<h3 class="text-2xl font-bold text-gray-900 mb-2">Submission Successful!</h3>

				<!-- Message -->
				<p class="text-gray-600 mb-4">
					Your pre-test has been submitted successfully. Thank you for completing the assessment.
				</p>

				{#if submittedAt}
					<p class="text-sm text-gray-500 mb-6">
						Submitted at: {formatDateTime(submittedAt)}
					</p>
				{/if}

				<!-- Action button -->
				<button
					type="button"
					class="w-full px-6 py-3 bg-indigo-600 text-white rounded-lg font-medium hover:bg-indigo-700 transition-all"
					onclick={handleClose}
				>
					Return to Dashboard
				</button>
			</div>
		</div>
	</div>
{/if}

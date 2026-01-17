<script lang="ts">
	import { authApi } from '$lib/api/auth';

	let currentPassword = $state('');
	let newPassword = $state('');
	let confirmPassword = $state('');
	let error = $state('');
	let success = $state(false);
	let loading = $state(false);

	let { onSuccess, onCancel } = $props<{
		onSuccess?: () => void;
		onCancel?: () => void;
	}>();

	async function handleSubmit() {
		error = '';
		success = false;

		// Validation
		if (newPassword.length < 8) {
			error = 'New password must be at least 8 characters';
			return;
		}

		if (newPassword !== confirmPassword) {
			error = 'Passwords do not match';
			return;
		}

		loading = true;

		try {
			await authApi.changePassword({
				currentPassword,
				newPassword
			});

			success = true;
			currentPassword = '';
			newPassword = '';
			confirmPassword = '';

			// Call success callback if provided
			if (onSuccess) {
				setTimeout(onSuccess, 1500);
			}
		} catch (err: unknown) {
			error = err instanceof Error ? err.message : 'Failed to change password';
		} finally {
			loading = false;
		}
	}

	function handleCancel() {
		if (onCancel) {
			onCancel();
		}
	}
</script>

<div class="bg-white shadow sm:rounded-lg">
	<div class="px-4 py-5 sm:p-6">
		<h3 class="text-lg leading-6 font-medium text-gray-900">Change Password</h3>
		<div class="mt-2 max-w-xl text-sm text-gray-500">
			<p>Update your password to keep your account secure.</p>
		</div>

		<form class="mt-5 sm:flex sm:flex-col sm:gap-4" onsubmit={e => { e.preventDefault(); handleSubmit(); }}>
			{#if error}
				<div class="rounded-md bg-red-50 p-4 mb-4">
					<p class="text-sm text-red-800">{error}</p>
				</div>
			{/if}

			{#if success}
				<div class="rounded-md bg-green-50 p-4 mb-4">
					<p class="text-sm text-green-800">Password changed successfully!</p>
				</div>
			{/if}

			<div>
				<label for="current-password" class="block text-sm font-medium text-gray-700">
					Current Password
				</label>
				<input
					type="password"
					id="current-password"
					name="current-password"
					autocomplete="current-password"
					required
					class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
					bind:value={currentPassword}
					disabled={loading}
				/>
			</div>

			<div>
				<label for="new-password" class="block text-sm font-medium text-gray-700">
					New Password
				</label>
				<input
					type="password"
					id="new-password"
					name="new-password"
					autocomplete="new-password"
					required
					minlength="8"
					class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
					bind:value={newPassword}
					disabled={loading}
				/>
				<p class="mt-1 text-xs text-gray-500">Must be at least 8 characters</p>
			</div>

			<div>
				<label for="confirm-password" class="block text-sm font-medium text-gray-700">
					Confirm New Password
				</label>
				<input
					type="password"
					id="confirm-password"
					name="confirm-password"
					autocomplete="new-password"
					required
					minlength="8"
					class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
					bind:value={confirmPassword}
					disabled={loading}
				/>
			</div>

			<div class="mt-4 flex gap-3">
				<button
					type="submit"
					class="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
					disabled={loading}
				>
					{loading ? 'Changing...' : 'Change Password'}
				</button>

				{#if onCancel}
					<button
						type="button"
						class="inline-flex items-center justify-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
						onclick={handleCancel}
						disabled={loading}
					>
						Cancel
					</button>
				{/if}
			</div>
		</form>
	</div>
</div>

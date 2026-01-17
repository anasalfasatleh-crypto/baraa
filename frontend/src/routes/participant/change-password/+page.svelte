<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { participantApi } from '$lib/services/participantApi';
	import { participantAuth } from '$lib/stores/participantAuth';
	import * as Button from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import * as Input from '$lib/components/ui/input';
	import * as Label from '$lib/components/ui/label';
	import * as Alert from '$lib/components/ui/alert';

	let currentPassword = $state('');
	let newPassword = $state('');
	let confirmPassword = $state('');
	let error = $state('');
	let success = $state(false);
	let loading = $state(false);
	let isRequired = $state(false);
	let authLoading = $state(true);

	// Validation errors
	let currentPasswordError = $state('');
	let newPasswordError = $state('');
	let confirmPasswordError = $state('');

	onMount(async () => {
		await participantAuth.init();

		const unsubscribe = participantAuth.subscribe((state) => {
			authLoading = state.isLoading;
			isRequired = state.mustChangePassword;

			if (!state.isLoading && !state.isAuthenticated) {
				goto('/participant/login');
			}
		});

		return () => {
			unsubscribe();
		};
	});

	function validateForm(): boolean {
		let isValid = true;
		currentPasswordError = '';
		newPasswordError = '';
		confirmPasswordError = '';

		if (!currentPassword) {
			currentPasswordError = 'Current password is required';
			isValid = false;
		}

		if (!newPassword) {
			newPasswordError = 'New password is required';
			isValid = false;
		} else if (newPassword.length < 8) {
			newPasswordError = 'Password must be at least 8 characters';
			isValid = false;
		}

		if (newPassword !== confirmPassword) {
			confirmPasswordError = 'Passwords do not match';
			isValid = false;
		}

		if (currentPassword && newPassword && currentPassword === newPassword) {
			newPasswordError = 'New password must be different from current password';
			isValid = false;
		}

		return isValid;
	}

	async function handleChangePassword() {
		if (!validateForm()) {
			return;
		}

		error = '';
		loading = true;

		try {
			await participantApi.changePassword(currentPassword, newPassword);

			// Clear the must change password flag
			participantAuth.clearMustChangePassword();

			success = true;

			// Redirect to dashboard after a short delay
			setTimeout(() => {
				goto('/participant');
			}, 2000);
		} catch (err: unknown) {
			if (err && typeof err === 'object' && 'status' in err) {
				const apiError = err as { status: number; error?: string; message?: string };
				if (apiError.status === 401) {
					error = 'Current password is incorrect';
				} else if (apiError.status === 400) {
					error = apiError.message || 'Please check your input and try again.';
				} else {
					error = apiError.message || 'Failed to change password. Please try again.';
				}
			} else {
				error = 'Failed to change password. Please try again.';
			}
		} finally {
			loading = false;
		}
	}

	function goBack() {
		if (!isRequired) {
			goto('/participant');
		}
	}
</script>

<div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-teal-50 to-cyan-100 px-4 sm:px-6 lg:px-8">
	<div class="max-w-md w-full space-y-8">
		<div class="text-center">
			<h2 class="mt-6 text-3xl font-bold tracking-tight text-teal-900">
				Change Password
			</h2>
			{#if isRequired}
				<p class="mt-2 text-sm text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-4 py-2 inline-block">
					Password change is required before continuing
				</p>
			{:else}
				<p class="mt-2 text-sm text-teal-700">
					Update your account password
				</p>
			{/if}
		</div>

		{#if authLoading}
			<div class="flex items-center justify-center h-32">
				<div class="animate-spin rounded-full h-8 w-8 border-b-2 border-teal-600"></div>
			</div>
		{:else if success}
			<Card.Root class="p-8 border-teal-200 bg-white">
				<div class="text-center space-y-6">
					<div class="mx-auto w-16 h-16 bg-teal-100 rounded-full flex items-center justify-center">
						<svg class="w-8 h-8 text-teal-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
						</svg>
					</div>

					<div>
						<h3 class="text-xl font-semibold text-gray-900">Password Changed!</h3>
						<p class="mt-2 text-sm text-gray-600">Your password has been updated successfully.</p>
						<p class="mt-1 text-sm text-teal-600">Redirecting to dashboard...</p>
					</div>
				</div>
			</Card.Root>
		{:else}
			<Card.Root class="p-8 border-teal-200 bg-white">
				<form class="space-y-6" onsubmit={e => { e.preventDefault(); handleChangePassword(); }}>
					{#if error}
						<Alert.Root variant="destructive">
							<p class="text-sm">{error}</p>
						</Alert.Root>
					{/if}

					<div class="space-y-4">
						<div class="space-y-2">
							<Label.Root for="currentPassword">
								{isRequired ? 'Temporary Password' : 'Current Password'}
							</Label.Root>
							<Input.Root
								id="currentPassword"
								name="currentPassword"
								type="password"
								autocomplete="current-password"
								required
								placeholder={isRequired ? 'Enter temporary password' : 'Enter current password'}
								bind:value={currentPassword}
								disabled={loading}
								class={currentPasswordError ? 'border-red-500' : ''}
							/>
							{#if currentPasswordError}
								<p class="text-sm text-red-500">{currentPasswordError}</p>
							{/if}
						</div>

						<div class="space-y-2">
							<Label.Root for="newPassword">New Password</Label.Root>
							<Input.Root
								id="newPassword"
								name="newPassword"
								type="password"
								autocomplete="new-password"
								required
								placeholder="Minimum 8 characters"
								bind:value={newPassword}
								disabled={loading}
								class={newPasswordError ? 'border-red-500' : ''}
							/>
							{#if newPasswordError}
								<p class="text-sm text-red-500">{newPasswordError}</p>
							{/if}
						</div>

						<div class="space-y-2">
							<Label.Root for="confirmPassword">Confirm New Password</Label.Root>
							<Input.Root
								id="confirmPassword"
								name="confirmPassword"
								type="password"
								autocomplete="new-password"
								required
								placeholder="Re-enter new password"
								bind:value={confirmPassword}
								disabled={loading}
								class={confirmPasswordError ? 'border-red-500' : ''}
							/>
							{#if confirmPasswordError}
								<p class="text-sm text-red-500">{confirmPasswordError}</p>
							{/if}
						</div>
					</div>

					<div class="flex gap-3">
						{#if !isRequired}
							<Button.Root
								type="button"
								variant="outline"
								class="flex-1 border-teal-300"
								onclick={goBack}
								disabled={loading}
							>
								Cancel
							</Button.Root>
						{/if}
						<Button.Root
							type="submit"
							class={`${isRequired ? 'w-full' : 'flex-1'} bg-teal-600 hover:bg-teal-700`}
							disabled={loading}
						>
							{loading ? 'Changing...' : 'Change Password'}
						</Button.Root>
					</div>
				</form>
			</Card.Root>
		{/if}
	</div>
</div>

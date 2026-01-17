<script lang="ts">
	import { goto } from '$app/navigation';
	import { participantApi } from '$lib/services/participantApi';
	import { participantAuth } from '$lib/stores/participantAuth';
	import * as Button from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import * as Input from '$lib/components/ui/input';
	import * as Label from '$lib/components/ui/label';
	import * as Alert from '$lib/components/ui/alert';

	let loginIdentifier = $state('');
	let password = $state('');
	let confirmPassword = $state('');
	let phoneNumber = $state('');
	let error = $state('');
	let loading = $state(false);
	let registrationSuccess = $state(false);
	let assignedCode = $state('');

	// Validation errors
	let loginIdentifierError = $state('');
	let passwordError = $state('');
	let confirmPasswordError = $state('');

	function validateForm(): boolean {
		let isValid = true;
		loginIdentifierError = '';
		passwordError = '';
		confirmPasswordError = '';

		// Validate login identifier (username or email)
		if (!loginIdentifier.trim()) {
			loginIdentifierError = 'Username or email is required';
			isValid = false;
		} else if (loginIdentifier.includes('@')) {
			// Email format validation
			const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
			if (!emailRegex.test(loginIdentifier)) {
				loginIdentifierError = 'Please enter a valid email address';
				isValid = false;
			}
		} else if (loginIdentifier.length < 3 || loginIdentifier.length > 50) {
			loginIdentifierError = 'Username must be between 3 and 50 characters';
			isValid = false;
		}

		// Validate password
		if (!password) {
			passwordError = 'Password is required';
			isValid = false;
		} else if (password.length < 8) {
			passwordError = 'Password must be at least 8 characters';
			isValid = false;
		}

		// Validate confirm password
		if (password !== confirmPassword) {
			confirmPasswordError = 'Passwords do not match';
			isValid = false;
		}

		return isValid;
	}

	async function handleRegister() {
		if (!validateForm()) {
			return;
		}

		error = '';
		loading = true;

		try {
			const response = await participantApi.register({
				loginIdentifier: loginIdentifier.trim(),
				password,
				phoneNumber: phoneNumber.trim() || undefined
			});

			// Store the assigned code for display
			assignedCode = response.code;
			registrationSuccess = true;

			// Update auth store
			participantAuth.setRegistered(response.accessToken, response.refreshToken, {
				id: response.id,
				code: response.code,
				loginIdentifier: response.loginIdentifier
			});
		} catch (err: unknown) {
			if (err && typeof err === 'object' && 'status' in err) {
				const apiError = err as { status: number; error?: string; message?: string };
				if (apiError.status === 409) {
					error = 'This username or email is already registered. Please try a different one.';
				} else if (apiError.status === 400) {
					error = apiError.message || 'Please check your input and try again.';
				} else {
					error = apiError.message || 'Registration failed. Please try again.';
				}
			} else {
				error = 'Registration failed. Please try again.';
			}
		} finally {
			loading = false;
		}
	}

	function goToDashboard() {
		goto('/participant');
	}

	function goToLogin() {
		goto('/participant/login');
	}
</script>

<div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-teal-50 to-cyan-100 px-4 sm:px-6 lg:px-8">
	<div class="max-w-md w-full space-y-8">
		<div class="text-center">
			<h2 class="mt-6 text-3xl font-bold tracking-tight text-teal-900">
				Participant Registration
			</h2>
			<p class="mt-2 text-sm text-teal-700">
				Create your participant account
			</p>
		</div>

		{#if registrationSuccess}
			<!-- Success State -->
			<Card.Root class="p-8 border-teal-200 bg-white">
				<div class="text-center space-y-6">
					<div class="mx-auto w-16 h-16 bg-teal-100 rounded-full flex items-center justify-center">
						<svg class="w-8 h-8 text-teal-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
						</svg>
					</div>

					<div>
						<h3 class="text-xl font-semibold text-gray-900">Registration Successful!</h3>
						<p class="mt-2 text-sm text-gray-600">Your account has been created successfully.</p>
					</div>

					<div class="bg-teal-50 border border-teal-200 rounded-lg p-4">
						<p class="text-sm text-teal-700 mb-2">Your Participant Code:</p>
						<p class="text-4xl font-bold text-teal-900">{assignedCode}</p>
						<p class="text-xs text-teal-600 mt-2">Please save this code for your records</p>
					</div>

					<Button.Root onclick={goToDashboard} class="w-full bg-teal-600 hover:bg-teal-700">
						Go to Dashboard
					</Button.Root>
				</div>
			</Card.Root>
		{:else}
			<!-- Registration Form -->
			<Card.Root class="p-8 border-teal-200 bg-white">
				<form class="space-y-6" onsubmit={e => { e.preventDefault(); handleRegister(); }}>
					{#if error}
						<Alert.Root variant="destructive">
							<p class="text-sm">{error}</p>
						</Alert.Root>
					{/if}

					<div class="space-y-4">
						<div class="space-y-2">
							<Label.Root for="loginIdentifier">Username or Email</Label.Root>
							<Input.Root
								id="loginIdentifier"
								name="loginIdentifier"
								type="text"
								autocomplete="username"
								required
								placeholder="Enter username or email"
								bind:value={loginIdentifier}
								disabled={loading}
								class={loginIdentifierError ? 'border-red-500' : ''}
							/>
							{#if loginIdentifierError}
								<p class="text-sm text-red-500">{loginIdentifierError}</p>
							{/if}
						</div>

						<div class="space-y-2">
							<Label.Root for="password">Password</Label.Root>
							<Input.Root
								id="password"
								name="password"
								type="password"
								autocomplete="new-password"
								required
								placeholder="Minimum 8 characters"
								bind:value={password}
								disabled={loading}
								class={passwordError ? 'border-red-500' : ''}
							/>
							{#if passwordError}
								<p class="text-sm text-red-500">{passwordError}</p>
							{/if}
						</div>

						<div class="space-y-2">
							<Label.Root for="confirmPassword">Confirm Password</Label.Root>
							<Input.Root
								id="confirmPassword"
								name="confirmPassword"
								type="password"
								autocomplete="new-password"
								required
								placeholder="Re-enter your password"
								bind:value={confirmPassword}
								disabled={loading}
								class={confirmPasswordError ? 'border-red-500' : ''}
							/>
							{#if confirmPasswordError}
								<p class="text-sm text-red-500">{confirmPasswordError}</p>
							{/if}
						</div>

						<div class="space-y-2">
							<Label.Root for="phoneNumber">Phone Number (Optional)</Label.Root>
							<Input.Root
								id="phoneNumber"
								name="phoneNumber"
								type="tel"
								autocomplete="tel"
								placeholder="+1234567890"
								bind:value={phoneNumber}
								disabled={loading}
							/>
						</div>
					</div>

					<Button.Root type="submit" class="w-full bg-teal-600 hover:bg-teal-700" disabled={loading}>
						{loading ? 'Creating Account...' : 'Register'}
					</Button.Root>
				</form>

				<div class="mt-6 text-center">
					<p class="text-sm text-gray-600">
						Already have an account?{' '}
						<button
							type="button"
							onclick={goToLogin}
							class="font-medium text-teal-600 hover:text-teal-500"
						>
							Sign in
						</button>
					</p>
				</div>
			</Card.Root>
		{/if}

		<p class="text-center text-xs text-teal-700">
			You will receive a unique participant code upon registration
		</p>
	</div>
</div>

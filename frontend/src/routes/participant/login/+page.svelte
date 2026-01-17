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
	let error = $state('');
	let loading = $state(false);

	// Lockout state
	let isLocked = $state(false);
	let lockoutSeconds = $state(0);
	let lockoutInterval: ReturnType<typeof setInterval> | null = null;

	function startLockoutCountdown(seconds: number) {
		isLocked = true;
		lockoutSeconds = seconds;

		if (lockoutInterval) {
			clearInterval(lockoutInterval);
		}

		lockoutInterval = setInterval(() => {
			lockoutSeconds--;
			if (lockoutSeconds <= 0) {
				isLocked = false;
				if (lockoutInterval) {
					clearInterval(lockoutInterval);
					lockoutInterval = null;
				}
			}
		}, 1000);
	}

	async function handleLogin() {
		if (isLocked) {
			return;
		}

		error = '';
		loading = true;

		try {
			const response = await participantApi.login({
				loginIdentifier: loginIdentifier.trim(),
				password
			});

			// Update auth store
			participantAuth.setAuthenticated(response);

			// Check if password change is required
			if (response.mustChangePassword) {
				goto('/participant/change-password');
			} else {
				goto('/participant');
			}
		} catch (err: unknown) {
			if (err && typeof err === 'object' && 'status' in err) {
				const apiError = err as { status: number; error?: string; message?: string; remainingSeconds?: number };
				if (apiError.status === 423) {
					// Account locked
					const lockedError = apiError as { remainingSeconds: number };
					startLockoutCountdown(lockedError.remainingSeconds || 60);
					error = '';
				} else if (apiError.status === 401) {
					// Generic error message - don't reveal which field is wrong
					error = 'Invalid credentials. Please check your username/email and password.';
				} else {
					error = apiError.message || 'Login failed. Please try again.';
				}
			} else {
				error = 'Login failed. Please try again.';
			}
		} finally {
			loading = false;
		}
	}

	function goToRegister() {
		goto('/participant/register');
	}

	// Cleanup on destroy
	$effect(() => {
		return () => {
			if (lockoutInterval) {
				clearInterval(lockoutInterval);
			}
		};
	});
</script>

<div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-teal-50 to-cyan-100 px-4 sm:px-6 lg:px-8">
	<div class="max-w-md w-full space-y-8">
		<div class="text-center">
			<h2 class="mt-6 text-3xl font-bold tracking-tight text-teal-900">
				Participant Login
			</h2>
			<p class="mt-2 text-sm text-teal-700">
				Sign in to your participant account
			</p>
		</div>

		<Card.Root class="p-8 border-teal-200 bg-white">
			<form class="space-y-6" onsubmit={e => { e.preventDefault(); handleLogin(); }}>
				{#if error}
					<Alert.Root variant="destructive">
						<p class="text-sm">{error}</p>
					</Alert.Root>
				{/if}

				{#if isLocked}
					<Alert.Root class="bg-amber-50 border-amber-200">
						<div class="flex items-center gap-2">
							<svg class="w-5 h-5 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
							</svg>
							<div>
								<p class="text-sm font-medium text-amber-800">Account Temporarily Locked</p>
								<p class="text-sm text-amber-700">
									Too many failed attempts. Try again in <span class="font-bold">{lockoutSeconds}</span> seconds.
								</p>
							</div>
						</div>
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
							disabled={loading || isLocked}
						/>
					</div>
					<div class="space-y-2">
						<Label.Root for="password">Password</Label.Root>
						<Input.Root
							id="password"
							name="password"
							type="password"
							autocomplete="current-password"
							required
							placeholder="Enter your password"
							bind:value={password}
							disabled={loading || isLocked}
						/>
					</div>
				</div>

				<Button.Root
					type="submit"
					class="w-full bg-teal-600 hover:bg-teal-700"
					disabled={loading || isLocked}
				>
					{#if loading}
						Signing in...
					{:else if isLocked}
						Locked ({lockoutSeconds}s)
					{:else}
						Sign in
					{/if}
				</Button.Root>
			</form>

			<div class="mt-6 text-center">
				<p class="text-sm text-gray-600">
					Don't have an account?{' '}
					<button
						type="button"
						onclick={goToRegister}
						class="font-medium text-teal-600 hover:text-teal-500"
					>
						Register
					</button>
				</p>
			</div>
		</Card.Root>

		<p class="text-center text-xs text-teal-700">
			Use your username or email to sign in
		</p>
	</div>
</div>

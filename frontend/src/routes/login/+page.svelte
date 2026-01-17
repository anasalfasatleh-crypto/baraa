<script lang="ts">
	import { authApi } from '$lib/api/auth';
	import { goto } from '$app/navigation';
	import { auth } from '$lib/stores/auth';
	import * as Button from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import * as Input from '$lib/components/ui/input';
	import * as Label from '$lib/components/ui/label';
	import * as Alert from '$lib/components/ui/alert';

	let email = $state('');
	let password = $state('');
	let error = $state('');
	let loading = $state(false);

	async function handleLogin() {
		error = '';
		loading = true;

		try {
			await authApi.login({ email, password });

			// Redirect based on role
			const user = $auth.user;
			if (user) {
				switch (user.role) {
					case 'Admin':
						goto('/admin/dashboard');
						break;
					case 'Evaluator':
						goto('/evaluator');
						break;
					case 'Student':
						goto('/student');
						break;
				}
			}
		} catch (err: unknown) {
			error = err instanceof Error ? err.message : 'Login failed';
		} finally {
			loading = false;
		}
	}
</script>

<div class="min-h-screen flex items-center justify-center bg-background px-4 sm:px-6 lg:px-8">
	<div class="max-w-md w-full space-y-8">
		<div class="text-center">
			<h2 class="mt-6 text-3xl font-bold tracking-tight">
				Research Data Collection Platform
			</h2>
			<p class="mt-2 text-sm text-muted-foreground">
				Sign in to your account
			</p>
		</div>

		<Card.Root class="p-8">
			<form class="space-y-6" onsubmit={e => { e.preventDefault(); handleLogin(); }}>
				{#if error}
					<Alert.Root variant="destructive">
						<p class="text-sm">{error}</p>
					</Alert.Root>
				{/if}

				<div class="space-y-4">
					<div class="space-y-2">
						<Label.Root for="email">Email address</Label.Root>
						<Input.Root
							id="email"
							name="email"
							type="email"
							autocomplete="email"
							required
							placeholder="Email address"
							bind:value={email}
							disabled={loading}
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
							placeholder="Password"
							bind:value={password}
							disabled={loading}
						/>
					</div>
				</div>

				<Button.Root type="submit" class="w-full" disabled={loading}>
					{loading ? 'Signing in...' : 'Sign in'}
				</Button.Root>
			</form>
		</Card.Root>

		<p class="text-center text-xs text-muted-foreground">
			Demo Credentials: admin@research.edu / admin123
		</p>
	</div>
</div>

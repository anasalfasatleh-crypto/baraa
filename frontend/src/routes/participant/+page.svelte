<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { participantAuth, isParticipantAuthenticated, mustChangePassword } from '$lib/stores/participantAuth';
	import * as Button from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';

	let loading = $state(true);
	let participant = $state<{ code: string; loginIdentifier: string; phoneNumber: string | null; createdAt: string; lastLoginAt: string | null } | null>(null);

	onMount(async () => {
		// Initialize auth state
		await participantAuth.init();

		// Subscribe to auth state
		const unsubscribe = participantAuth.subscribe((state) => {
			loading = state.isLoading;
			participant = state.participant;

			if (!state.isLoading) {
				if (!state.isAuthenticated) {
					goto('/participant/login');
				} else if (state.mustChangePassword) {
					goto('/participant/change-password');
				}
			}
		});

		return () => {
			unsubscribe();
		};
	});

	async function handleLogout() {
		await participantAuth.logout();
		goto('/participant/login');
	}

	function formatDate(dateString: string | null): string {
		if (!dateString) return 'Never';
		return new Date(dateString).toLocaleDateString('en-US', {
			year: 'numeric',
			month: 'long',
			day: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}
</script>

<div class="min-h-screen bg-gradient-to-br from-teal-50 to-cyan-100">
	<!-- Header -->
	<header class="bg-white shadow-sm border-b border-teal-200">
		<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
			<div class="flex justify-between items-center h-16">
				<div class="flex items-center gap-2">
					<div class="w-8 h-8 bg-teal-600 rounded-lg flex items-center justify-center">
						<span class="text-white font-bold text-sm">P</span>
					</div>
					<span class="font-semibold text-teal-900">Participant Portal</span>
				</div>
				<Button.Root variant="outline" onclick={handleLogout} class="border-teal-300 text-teal-700 hover:bg-teal-50">
					Sign Out
				</Button.Root>
			</div>
		</div>
	</header>

	<!-- Main Content -->
	<main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
		{#if loading}
			<div class="flex items-center justify-center h-64">
				<div class="animate-spin rounded-full h-12 w-12 border-b-2 border-teal-600"></div>
			</div>
		{:else if participant}
			<div class="space-y-8">
				<!-- Welcome Section -->
				<div class="text-center">
					<h1 class="text-3xl font-bold text-teal-900">Welcome, Participant</h1>
					<p class="mt-2 text-teal-700">Your participant dashboard</p>
				</div>

				<!-- Participant Code Card -->
				<Card.Root class="max-w-md mx-auto border-teal-200 bg-white overflow-hidden">
					<div class="bg-teal-600 px-6 py-4">
						<h2 class="text-lg font-medium text-white">Your Participant Code</h2>
					</div>
					<div class="p-8 text-center">
						<div class="bg-teal-50 border-2 border-teal-200 rounded-xl p-6 inline-block">
							<p class="text-6xl font-bold text-teal-900 tracking-wider">{participant.code}</p>
						</div>
						<p class="mt-4 text-sm text-gray-600">
							Use this code when required for research activities
						</p>
					</div>
				</Card.Root>

				<!-- Profile Information -->
				<Card.Root class="max-w-md mx-auto border-teal-200 bg-white">
					<Card.Header>
						<Card.Title class="text-teal-900">Profile Information</Card.Title>
					</Card.Header>
					<Card.Content class="space-y-4">
						<div class="flex justify-between items-center py-2 border-b border-gray-100">
							<span class="text-gray-600">Login Identifier</span>
							<span class="font-medium text-gray-900">{participant.loginIdentifier}</span>
						</div>
						{#if participant.phoneNumber}
							<div class="flex justify-between items-center py-2 border-b border-gray-100">
								<span class="text-gray-600">Phone Number</span>
								<span class="font-medium text-gray-900">{participant.phoneNumber}</span>
							</div>
						{/if}
						<div class="flex justify-between items-center py-2 border-b border-gray-100">
							<span class="text-gray-600">Member Since</span>
							<span class="font-medium text-gray-900">{formatDate(participant.createdAt)}</span>
						</div>
						<div class="flex justify-between items-center py-2">
							<span class="text-gray-600">Last Login</span>
							<span class="font-medium text-gray-900">{formatDate(participant.lastLoginAt)}</span>
						</div>
					</Card.Content>
				</Card.Root>

				<!-- Quick Actions -->
				<Card.Root class="max-w-md mx-auto border-teal-200 bg-white">
					<Card.Header>
						<Card.Title class="text-teal-900">Quick Actions</Card.Title>
					</Card.Header>
					<Card.Content class="space-y-3">
						<Button.Root
							variant="outline"
							class="w-full justify-start border-teal-200 hover:bg-teal-50"
							onclick={() => goto('/participant/change-password')}
						>
							<svg class="w-5 h-5 mr-2 text-teal-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
							</svg>
							Change Password
						</Button.Root>
					</Card.Content>
				</Card.Root>
			</div>
		{/if}
	</main>
</div>

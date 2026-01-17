<script lang="ts">
	import { onMount } from 'svelte';
	import { participantApi, type ParticipantSummary, type ParticipantDetails, type ParticipantListResponse } from '$lib/services/participantApi';
	import * as Button from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import * as Input from '$lib/components/ui/input';
	import * as Table from '$lib/components/ui/table';
	import * as Alert from '$lib/components/ui/alert';

	let participants = $state<ParticipantSummary[]>([]);
	let loading = $state(true);
	let error = $state('');
	let searchQuery = $state('');
	let searchTimeout: ReturnType<typeof setTimeout> | null = null;

	// Pagination
	let currentPage = $state(1);
	let pageSize = $state(20);
	let totalCount = $state(0);
	let totalPages = $state(0);

	// Selected participant for details/reset
	let selectedParticipant = $state<ParticipantDetails | null>(null);
	let showDetails = $state(false);
	let detailsLoading = $state(false);

	// Password reset
	let showResetModal = $state(false);
	let resetLoading = $state(false);
	let tempPassword = $state('');
	let resetSuccess = $state(false);
	let copySuccess = $state(false);

	onMount(async () => {
		await loadParticipants();
	});

	async function loadParticipants() {
		loading = true;
		error = '';

		try {
			const response: ParticipantListResponse = await participantApi.listParticipants(currentPage, pageSize, searchQuery || undefined);
			participants = response.items;
			totalCount = response.totalCount;
			totalPages = response.totalPages;
		} catch (err) {
			error = 'Failed to load participants';
			console.error(err);
		} finally {
			loading = false;
		}
	}

	function handleSearch() {
		if (searchTimeout) {
			clearTimeout(searchTimeout);
		}

		searchTimeout = setTimeout(() => {
			currentPage = 1;
			loadParticipants();
		}, 300);
	}

	function goToPage(page: number) {
		if (page >= 1 && page <= totalPages) {
			currentPage = page;
			loadParticipants();
		}
	}

	async function viewDetails(participant: ParticipantSummary) {
		showDetails = true;
		detailsLoading = true;
		selectedParticipant = null;

		try {
			const details = await participantApi.getParticipantDetails(participant.id);
			selectedParticipant = details;
		} catch (err) {
			error = 'Failed to load participant details';
			showDetails = false;
		} finally {
			detailsLoading = false;
		}
	}

	function closeDetails() {
		showDetails = false;
		selectedParticipant = null;
	}

	function openResetModal(participant: ParticipantDetails) {
		selectedParticipant = participant;
		showResetModal = true;
		tempPassword = '';
		resetSuccess = false;
		copySuccess = false;
	}

	function closeResetModal() {
		showResetModal = false;
		tempPassword = '';
		resetSuccess = false;
	}

	async function resetPassword() {
		if (!selectedParticipant) return;

		resetLoading = true;
		error = '';

		try {
			const response = await participantApi.resetParticipantPassword(selectedParticipant.id);
			tempPassword = response.temporaryPassword;
			resetSuccess = true;
		} catch (err) {
			error = 'Failed to reset password';
		} finally {
			resetLoading = false;
		}
	}

	async function copyToClipboard() {
		try {
			await navigator.clipboard.writeText(tempPassword);
			copySuccess = true;
			setTimeout(() => {
				copySuccess = false;
			}, 2000);
		} catch (err) {
			console.error('Failed to copy:', err);
		}
	}

	function formatDate(dateString: string | null): string {
		if (!dateString) return 'Never';
		return new Date(dateString).toLocaleDateString('en-US', {
			year: 'numeric',
			month: 'short',
			day: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}
</script>

<div class="p-6 space-y-6">
	<div class="flex justify-between items-center">
		<div>
			<h1 class="text-2xl font-bold text-gray-900">Participant Management</h1>
			<p class="text-gray-600">Manage participant accounts and passwords</p>
		</div>
	</div>

	{#if error}
		<Alert.Root variant="destructive">
			<p class="text-sm">{error}</p>
		</Alert.Root>
	{/if}

	<!-- Search -->
	<Card.Root>
		<Card.Content class="pt-6">
			<div class="flex gap-4">
				<div class="flex-1">
					<Input.Root
						type="text"
						placeholder="Search by code, username/email, or phone..."
						bind:value={searchQuery}
						oninput={handleSearch}
					/>
				</div>
				<Button.Root variant="outline" onclick={loadParticipants}>
					Refresh
				</Button.Root>
			</div>
		</Card.Content>
	</Card.Root>

	<!-- Participants Table -->
	<Card.Root>
		<Card.Content class="p-0">
			{#if loading}
				<div class="flex items-center justify-center h-64">
					<div class="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
				</div>
			{:else if participants.length === 0}
				<div class="flex flex-col items-center justify-center h-64 text-gray-500">
					<svg class="w-12 h-12 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
					</svg>
					<p>No participants found</p>
				</div>
			{:else}
				<Table.Root>
					<Table.Header>
						<Table.Row>
							<Table.Head class="w-24">Code</Table.Head>
							<Table.Head>Login Identifier</Table.Head>
							<Table.Head>Created</Table.Head>
							<Table.Head class="text-right">Actions</Table.Head>
						</Table.Row>
					</Table.Header>
					<Table.Body>
						{#each participants as participant}
							<Table.Row>
								<Table.Cell class="font-mono font-bold text-teal-700">{participant.code}</Table.Cell>
								<Table.Cell>{participant.loginIdentifier}</Table.Cell>
								<Table.Cell>{formatDate(participant.createdAt)}</Table.Cell>
								<Table.Cell class="text-right">
									<Button.Root
										variant="outline"
										size="sm"
										onclick={() => viewDetails(participant)}
									>
										View Details
									</Button.Root>
								</Table.Cell>
							</Table.Row>
						{/each}
					</Table.Body>
				</Table.Root>

				<!-- Pagination -->
				{#if totalPages > 1}
					<div class="flex items-center justify-between px-4 py-3 border-t">
						<div class="text-sm text-gray-600">
							Showing {(currentPage - 1) * pageSize + 1} to {Math.min(currentPage * pageSize, totalCount)} of {totalCount} participants
						</div>
						<div class="flex gap-2">
							<Button.Root
								variant="outline"
								size="sm"
								disabled={currentPage === 1}
								onclick={() => goToPage(currentPage - 1)}
							>
								Previous
							</Button.Root>
							<span class="flex items-center px-3 text-sm">
								Page {currentPage} of {totalPages}
							</span>
							<Button.Root
								variant="outline"
								size="sm"
								disabled={currentPage === totalPages}
								onclick={() => goToPage(currentPage + 1)}
							>
								Next
							</Button.Root>
						</div>
					</div>
				{/if}
			{/if}
		</Card.Content>
	</Card.Root>
</div>

<!-- Details Modal -->
{#if showDetails}
	<div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onclick={closeDetails}>
		<div class="bg-white rounded-lg shadow-xl max-w-md w-full mx-4" onclick={e => e.stopPropagation()}>
			{#if detailsLoading}
				<div class="flex items-center justify-center h-64">
					<div class="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
				</div>
			{:else if selectedParticipant}
				<div class="p-6 border-b">
					<div class="flex items-center justify-between">
						<h2 class="text-xl font-semibold">Participant Details</h2>
						<button onclick={closeDetails} class="text-gray-400 hover:text-gray-600">
							<svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
							</svg>
						</button>
					</div>
				</div>

				<div class="p-6 space-y-4">
					<div class="bg-teal-50 border border-teal-200 rounded-lg p-4 text-center">
						<p class="text-sm text-teal-700 mb-1">Participant Code</p>
						<p class="text-3xl font-bold text-teal-900">{selectedParticipant.code}</p>
					</div>

					<div class="space-y-3">
						<div class="flex justify-between">
							<span class="text-gray-600">Login Identifier:</span>
							<span class="font-medium">{selectedParticipant.loginIdentifier}</span>
						</div>
						{#if selectedParticipant.phoneNumber}
							<div class="flex justify-between">
								<span class="text-gray-600">Phone Number:</span>
								<span class="font-medium">{selectedParticipant.phoneNumber}</span>
							</div>
						{/if}
						<div class="flex justify-between">
							<span class="text-gray-600">Created:</span>
							<span class="font-medium">{formatDate(selectedParticipant.createdAt)}</span>
						</div>
						<div class="flex justify-between">
							<span class="text-gray-600">Last Login:</span>
							<span class="font-medium">{formatDate(selectedParticipant.lastLoginAt)}</span>
						</div>
						<div class="flex justify-between">
							<span class="text-gray-600">Failed Attempts:</span>
							<span class="font-medium">{selectedParticipant.failedLoginAttempts}</span>
						</div>
						<div class="flex justify-between">
							<span class="text-gray-600">Account Status:</span>
							{#if selectedParticipant.isLocked}
								<span class="text-red-600 font-medium">Locked</span>
							{:else}
								<span class="text-green-600 font-medium">Active</span>
							{/if}
						</div>
						{#if selectedParticipant.mustChangePassword}
							<div class="bg-amber-50 border border-amber-200 rounded p-2 text-sm text-amber-800">
								Password change required on next login
							</div>
						{/if}
					</div>
				</div>

				<div class="p-6 border-t bg-gray-50 rounded-b-lg">
					<Button.Root
						class="w-full bg-amber-600 hover:bg-amber-700"
						onclick={() => openResetModal(selectedParticipant!)}
					>
						Reset Password
					</Button.Root>
				</div>
			{/if}
		</div>
	</div>
{/if}

<!-- Reset Password Modal -->
{#if showResetModal}
	<div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onclick={closeResetModal}>
		<div class="bg-white rounded-lg shadow-xl max-w-md w-full mx-4" onclick={e => e.stopPropagation()}>
			<div class="p-6 border-b">
				<h2 class="text-xl font-semibold">Reset Password</h2>
			</div>

			{#if resetSuccess}
				<div class="p-6 space-y-4">
					<div class="bg-green-50 border border-green-200 rounded-lg p-4 text-center">
						<svg class="w-12 h-12 text-green-600 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
						</svg>
						<p class="text-green-800 font-medium">Password Reset Successful!</p>
					</div>

					<div class="bg-gray-100 border rounded-lg p-4">
						<p class="text-sm text-gray-600 mb-2">Temporary Password:</p>
						<div class="flex items-center gap-2">
							<code class="flex-1 text-lg font-mono bg-white px-3 py-2 rounded border">
								{tempPassword}
							</code>
							<Button.Root
								variant="outline"
								size="sm"
								onclick={copyToClipboard}
							>
								{copySuccess ? 'Copied!' : 'Copy'}
							</Button.Root>
						</div>
					</div>

					<p class="text-sm text-gray-600">
						Share this temporary password with the participant. They will be required to change it on their next login.
					</p>
				</div>

				<div class="p-6 border-t bg-gray-50 rounded-b-lg">
					<Button.Root class="w-full" onclick={closeResetModal}>
						Done
					</Button.Root>
				</div>
			{:else}
				<div class="p-6 space-y-4">
					<p class="text-gray-600">
						Are you sure you want to reset the password for participant <strong class="text-teal-700">{selectedParticipant?.code}</strong>?
					</p>
					<p class="text-sm text-gray-500">
						A new temporary password will be generated. The participant will need to change it on their next login.
					</p>
				</div>

				<div class="p-6 border-t bg-gray-50 rounded-b-lg flex gap-3">
					<Button.Root variant="outline" class="flex-1" onclick={closeResetModal} disabled={resetLoading}>
						Cancel
					</Button.Root>
					<Button.Root class="flex-1 bg-amber-600 hover:bg-amber-700" onclick={resetPassword} disabled={resetLoading}>
						{resetLoading ? 'Resetting...' : 'Reset Password'}
					</Button.Root>
				</div>
			{/if}
		</div>
	</div>
{/if}

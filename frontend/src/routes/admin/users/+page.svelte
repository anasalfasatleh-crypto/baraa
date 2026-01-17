<script lang="ts">
	import { onMount } from 'svelte';
	import { adminApi, type AdminUser, type CreateUserRequest } from '$lib/api/admin';
	import UserTable from '$lib/components/UserTable.svelte';
	import CreateUserForm from '$lib/components/CreateUserForm.svelte';
	import * as Button from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import * as Alert from '$lib/components/ui/alert';
	import * as Select from '$lib/components/ui/select';
	import * as Label from '$lib/components/ui/label';

	let users = $state<AdminUser[]>([]);
	let isLoading = $state(true);
	let error = $state<string | null>(null);
	let successMessage = $state<string | null>(null);
	let showCreateForm = $state(false);
	let isCreating = $state(false);

	let roleFilter = $state('');
	let statusFilter = $state('');

	onMount(async () => {
		await loadUsers();
	});

	async function loadUsers() {
		isLoading = true;
		error = null;

		try {
			users = await adminApi.getUsers(
				roleFilter || undefined,
				statusFilter || undefined
			);
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load users';
		} finally {
			isLoading = false;
		}
	}

	async function handleCreateUser(user: CreateUserRequest) {
		isCreating = true;
		error = null;
		successMessage = null;

		try {
			await adminApi.createUser(user);
			successMessage = 'User created successfully';
			showCreateForm = false;
			await loadUsers();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to create user';
		} finally {
			isCreating = false;
		}
	}

	async function handleActivateUser(id: string) {
		try {
			await adminApi.activateUser(id);
			successMessage = 'User activated successfully';
			await loadUsers();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to activate user';
		}
	}

	async function handleDeactivateUser(id: string) {
		try {
			await adminApi.deactivateUser(id);
			successMessage = 'User deactivated successfully';
			await loadUsers();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to deactivate user';
		}
	}

	async function handleDeleteUser(id: string) {
		if (!confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
			return;
		}

		try {
			await adminApi.deleteUser(id);
			successMessage = 'User deleted successfully';
			await loadUsers();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to delete user';
		}
	}
</script>

<div class="max-w-7xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
	<div class="mb-8">
		<div class="flex justify-between items-center">
			<div>
				<h1 class="text-3xl font-bold mb-2">User Management</h1>
				<p class="text-muted-foreground">Create and manage user accounts.</p>
			</div>
			<Button.Root onclick={() => (showCreateForm = !showCreateForm)}>
				{showCreateForm ? 'Cancel' : 'Create User'}
			</Button.Root>
		</div>
	</div>

	{#if successMessage}
		<Alert.Root class="mb-6 border-green-500 bg-green-50 text-green-900">
			<div class="flex items-center">
				<svg
					class="h-5 w-5 mr-2"
					fill="none"
					stroke="currentColor"
					viewBox="0 0 24 24"
				>
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M5 13l4 4L19 7"
					></path>
				</svg>
				<span>{successMessage}</span>
			</div>
		</Alert.Root>
	{/if}

	{#if error}
		<Alert.Root variant="destructive" class="mb-6">
			<div class="flex items-start">
				<svg
					class="h-5 w-5 mr-2 shrink-0"
					fill="none"
					stroke="currentColor"
					viewBox="0 0 24 24"
				>
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
					></path>
				</svg>
				<span>{error}</span>
			</div>
		</Alert.Root>
	{/if}

	{#if showCreateForm}
		<Card.Root class="mb-6 p-6">
			<h2 class="text-lg font-semibold mb-4">Create New User</h2>
			<CreateUserForm
				onSubmit={handleCreateUser}
				onCancel={() => (showCreateForm = false)}
				isSubmitting={isCreating}
			/>
		</Card.Root>
	{/if}

	<!-- Filters -->
	<Card.Root class="mb-6 p-4">
		<div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
			<div>
				<Label.Root for="roleFilter" class="mb-2">Filter by Role</Label.Root>
				<Select.Root bind:value={roleFilter as any}>
					<Select.Trigger id="roleFilter" onchange={loadUsers}>
						{roleFilter || 'All Roles'}
					</Select.Trigger>
					<Select.Content>
						<Select.Item value="">All Roles</Select.Item>
						<Select.Item value="Admin">Admin</Select.Item>
						<Select.Item value="Evaluator">Evaluator</Select.Item>
						<Select.Item value="Student">Student</Select.Item>
					</Select.Content>
				</Select.Root>
			</div>
			<div>
				<Label.Root for="statusFilter" class="mb-2">Filter by Status</Label.Root>
				<Select.Root bind:value={statusFilter as any}>
					<Select.Trigger id="statusFilter" onchange={loadUsers}>
						{statusFilter || 'All Statuses'}
					</Select.Trigger>
					<Select.Content>
						<Select.Item value="">All Statuses</Select.Item>
						<Select.Item value="Active">Active</Select.Item>
						<Select.Item value="Inactive">Inactive</Select.Item>
					</Select.Content>
				</Select.Root>
			</div>
			<div class="flex items-end">
				<Button.Root
					variant="outline"
					class="w-full"
					onclick={() => {
						roleFilter = '';
						statusFilter = '';
						loadUsers();
					}}
				>
					Clear Filters
				</Button.Root>
			</div>
		</div>
	</Card.Root>

	{#if isLoading}
		<Card.Root class="flex items-center justify-center py-12">
			<svg
				class="animate-spin h-12 w-12 text-primary"
				fill="none"
				viewBox="0 0 24 24"
			>
				<circle
					class="opacity-25"
					cx="12"
					cy="12"
					r="10"
					stroke="currentColor"
					stroke-width="4"
				></circle>
				<path
					class="opacity-75"
					fill="currentColor"
					d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
				></path>
			</svg>
		</Card.Root>
	{:else if users.length === 0}
		<Alert.Root class="text-center border-yellow-500 bg-yellow-50 text-yellow-900">
			<p class="font-medium">No users found</p>
			<p class="text-sm mt-1">Try adjusting your filters or create a new user.</p>
		</Alert.Root>
	{:else}
		<Card.Root>
			<UserTable
				{users}
				onActivate={handleActivateUser}
				onDeactivate={handleDeactivateUser}
				onDelete={handleDeleteUser}
			/>
		</Card.Root>
	{/if}
</div>

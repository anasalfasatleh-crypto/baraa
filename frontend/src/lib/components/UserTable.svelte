<script lang="ts">
	import type { AdminUser } from '$lib/api/admin';
	import { goto } from '$app/navigation';
	import * as Table from '$lib/components/ui/table';
	import { Badge } from '$lib/components/ui/badge';
	import * as Button from '$lib/components/ui/button';

	interface Props {
		users: AdminUser[];
		onActivate?: (id: string) => void;
		onDeactivate?: (id: string) => void;
		onDelete?: (id: string) => void;
	}

	let { users, onActivate, onDeactivate, onDelete }: Props = $props();

	function handleViewUser(id: string) {
		goto(`/admin/users/${id}`);
	}

	function getRoleBadgeVariant(
		role: string
	): 'default' | 'secondary' | 'outline' | 'destructive' {
		if (role === 'Admin') return 'destructive';
		if (role === 'Evaluator') return 'default';
		return 'secondary';
	}

	function getStatusBadgeVariant(
		status: string
	): 'default' | 'secondary' | 'outline' | 'destructive' {
		return status === 'Active' ? 'default' : 'destructive';
	}
</script>

<Table.Root>
	<Table.Header>
		<Table.Row>
			<Table.Head>Name</Table.Head>
			<Table.Head>Email</Table.Head>
			<Table.Head>Role</Table.Head>
			<Table.Head>Status</Table.Head>
			<Table.Head>Hospital</Table.Head>
			<Table.Head class="text-right">Actions</Table.Head>
		</Table.Row>
	</Table.Header>
	<Table.Body>
		{#each users as user (user.id)}
			<Table.Row>
				<Table.Cell>
					<div class="font-medium">{user.name}</div>
					{#if user.gender}
						<div class="text-sm text-muted-foreground">{user.gender}</div>
					{/if}
				</Table.Cell>
				<Table.Cell>
					<div class="text-sm">{user.email}</div>
				</Table.Cell>
				<Table.Cell>
					<Badge variant={getRoleBadgeVariant(user.role)}>
						{user.role}
					</Badge>
				</Table.Cell>
				<Table.Cell>
					<Badge variant={getStatusBadgeVariant(user.status)}>
						{user.status}
					</Badge>
				</Table.Cell>
				<Table.Cell class="text-muted-foreground">
					{user.hospital || '-'}
				</Table.Cell>
				<Table.Cell class="text-right">
					<div class="flex justify-end gap-2">
						<Button.Root variant="ghost" size="sm" onclick={() => handleViewUser(user.id)}>
							View
						</Button.Root>
						{#if user.status === 'Active' && onDeactivate}
							<Button.Root variant="outline" size="sm" onclick={() => onDeactivate(user.id)}>
								Deactivate
							</Button.Root>
						{:else if user.status === 'Inactive' && onActivate}
							<Button.Root variant="outline" size="sm" onclick={() => onActivate(user.id)}>
								Activate
							</Button.Root>
						{/if}
						{#if onDelete}
							<Button.Root variant="destructive" size="sm" onclick={() => onDelete(user.id)}>
								Delete
							</Button.Root>
						{/if}
					</div>
				</Table.Cell>
			</Table.Row>
		{/each}
	</Table.Body>
</Table.Root>

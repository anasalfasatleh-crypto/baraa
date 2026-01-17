<script lang="ts">
	import type { CreateUserRequest } from '$lib/api/admin';
	import * as Button from '$lib/components/ui/button';
	import * as Input from '$lib/components/ui/input';
	import * as Select from '$lib/components/ui/select';
	import * as Label from '$lib/components/ui/label';

	interface Props {
		onSubmit: (user: CreateUserRequest) => void;
		onCancel: () => void;
		isSubmitting?: boolean;
	}

	let { onSubmit, onCancel, isSubmitting = false }: Props = $props();

	let email = $state('');
	let password = $state('');
	let name = $state('');
	let role = $state('Student');
	let hospital = $state('');
	let gender = $state('');

	function handleSubmit(e: Event) {
		e.preventDefault();

		const user: CreateUserRequest = {
			email,
			password,
			name,
			role,
			hospital: hospital || undefined,
			gender: gender || undefined
		};

		onSubmit(user);
	}
</script>

<form onsubmit={handleSubmit} class="space-y-6">
	<div class="grid grid-cols-1 gap-6 sm:grid-cols-2">
		<!-- Name -->
		<div class="space-y-2">
			<Label.Root for="name">
				Name <span class="text-destructive">*</span>
			</Label.Root>
			<Input.Root id="name" type="text" bind:value={name} required disabled={isSubmitting} />
		</div>

		<!-- Email -->
		<div class="space-y-2">
			<Label.Root for="email">
				Email <span class="text-destructive">*</span>
			</Label.Root>
			<Input.Root id="email" type="email" bind:value={email} required disabled={isSubmitting} />
		</div>

		<!-- Password -->
		<div class="space-y-2">
			<Label.Root for="password">
				Password <span class="text-destructive">*</span>
			</Label.Root>
			<Input.Root
				id="password"
				type="password"
				bind:value={password}
				required
				disabled={isSubmitting}
			/>
		</div>

		<!-- Role -->
		<div class="space-y-2">
			<Label.Root for="role">
				Role <span class="text-destructive">*</span>
			</Label.Root>
			<Select.Root bind:value={role as any} disabled={isSubmitting}>
				<Select.Trigger id="role">{role || 'Select role'}</Select.Trigger>
				<Select.Content>
					<Select.Item value="Student">Student</Select.Item>
					<Select.Item value="Evaluator">Evaluator</Select.Item>
					<Select.Item value="Admin">Admin</Select.Item>
				</Select.Content>
			</Select.Root>
		</div>

		<!-- Hospital -->
		<div class="space-y-2">
			<Label.Root for="hospital">Hospital</Label.Root>
			<Input.Root id="hospital" type="text" bind:value={hospital} disabled={isSubmitting} />
		</div>

		<!-- Gender -->
		<div class="space-y-2">
			<Label.Root for="gender">Gender</Label.Root>
			<Select.Root bind:value={gender as any} disabled={isSubmitting}>
				<Select.Trigger id="gender">{gender || 'Select gender'}</Select.Trigger>
				<Select.Content>
					<Select.Item value="">Not specified</Select.Item>
					<Select.Item value="Male">Male</Select.Item>
					<Select.Item value="Female">Female</Select.Item>
				</Select.Content>
			</Select.Root>
		</div>
	</div>

	<!-- Actions -->
	<div class="flex justify-end gap-3">
		<Button.Root type="button" variant="outline" onclick={onCancel} disabled={isSubmitting}>
			Cancel
		</Button.Root>
		<Button.Root type="submit" disabled={isSubmitting}>
			{isSubmitting ? 'Creating...' : 'Create User'}
		</Button.Root>
	</div>
</form>

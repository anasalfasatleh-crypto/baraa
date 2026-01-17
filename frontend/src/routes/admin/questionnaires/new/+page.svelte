<script lang="ts">
	import { adminApi, type CreateQuestionnaireRequest } from '$lib/api/admin';
	import { goto } from '$app/navigation';
	import { Button } from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import * as Alert from '$lib/components/ui/alert';
	import { Input } from '$lib/components/ui/input';
	import { Label } from '$lib/components/ui/label';
	import { Textarea } from '$lib/components/ui/textarea';
	import * as Select from '$lib/components/ui/select';
	import { ArrowLeft, AlertCircle, Loader2 } from '@lucide/svelte';

	let title = $state('');
	let description = $state('');
	let type = $state<'Pretest' | 'Posttest'>('Pretest');
	let isCreating = $state(false);
	let error = $state<string | null>(null);

	const typeOptions = [
		{ value: 'Pretest', label: 'Pre-test' },
		{ value: 'Posttest', label: 'Post-test' }
	];

	const selectedTypeLabel = $derived(typeOptions.find((t) => t.value === type)?.label ?? 'Select type...');

	async function handleSubmit(e: Event) {
		e.preventDefault();
		isCreating = true;
		error = null;

		try {
			const request: CreateQuestionnaireRequest = {
				title,
				description: description || undefined,
				type
			};

			const questionnaire = await adminApi.createQuestionnaire(request);
			goto(`/admin/questionnaires/${questionnaire.id}`);
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to create questionnaire';
			isCreating = false;
		}
	}

	function handleCancel() {
		goto('/admin/questionnaires');
	}
</script>

<div class="max-w-2xl space-y-6">
	<!-- Header -->
	<div>
		<Button variant="ghost" onclick={handleCancel} class="mb-4 -ml-2">
			<ArrowLeft class="mr-2 h-4 w-4" />
			Back to Questionnaires
		</Button>
		<h1 class="text-3xl font-bold tracking-tight">Create New Questionnaire</h1>
		<p class="text-muted-foreground">
			Create a new questionnaire. You can add questions after creating it.
		</p>
	</div>

	<!-- Error Message -->
	{#if error}
		<Alert.Root variant="destructive">
			<AlertCircle class="h-4 w-4" />
			<Alert.Title>Error</Alert.Title>
			<Alert.Description>{error}</Alert.Description>
		</Alert.Root>
	{/if}

	<!-- Form -->
	<Card.Root>
		<Card.Content class="pt-6">
			<form onsubmit={handleSubmit} class="space-y-6">
				<!-- Title -->
				<div class="space-y-2">
					<Label for="title">
						Title <span class="text-destructive">*</span>
					</Label>
					<Input
						id="title"
						bind:value={title}
						required
						disabled={isCreating}
						placeholder="e.g., Pre-test Knowledge Assessment"
					/>
				</div>

				<!-- Description -->
				<div class="space-y-2">
					<Label for="description">Description</Label>
					<Textarea
						id="description"
						bind:value={description}
						rows={3}
						disabled={isCreating}
						placeholder="Optional description of this questionnaire..."
					/>
				</div>

				<!-- Type -->
				<div class="space-y-2">
					<Label>
						Type <span class="text-destructive">*</span>
					</Label>
					<Select.Root
						type="single"
						value={type}
						onValueChange={(v) => {
							if (v) type = v as 'Pretest' | 'Posttest';
						}}
						disabled={isCreating}
					>
						<Select.Trigger class="w-full">
							{selectedTypeLabel}
						</Select.Trigger>
						<Select.Content>
							{#each typeOptions as option}
								<Select.Item value={option.value} label={option.label}>
									{option.label}
								</Select.Item>
							{/each}
						</Select.Content>
					</Select.Root>
					<p class="text-sm text-muted-foreground">
						Pre-tests are given before educational materials, post-tests after.
					</p>
				</div>

				<!-- Actions -->
				<div class="flex justify-end gap-3 pt-4 border-t">
					<Button
						type="button"
						variant="outline"
						onclick={handleCancel}
						disabled={isCreating}
					>
						Cancel
					</Button>
					<Button type="submit" disabled={isCreating}>
						{#if isCreating}
							<Loader2 class="mr-2 h-4 w-4 animate-spin" />
							Creating...
						{:else}
							Create Questionnaire
						{/if}
					</Button>
				</div>
			</form>
		</Card.Content>
	</Card.Root>
</div>

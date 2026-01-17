<script lang="ts">
	import { adminApi, type QuestionnaireListItem } from '$lib/api/admin';
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { Button } from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import * as Table from '$lib/components/ui/table';
	import * as Alert from '$lib/components/ui/alert';
	import { Badge } from '$lib/components/ui/badge';
	import { Plus, FileText, AlertCircle, CheckCircle2, Loader2 } from '@lucide/svelte';

	let questionnaires = $state<QuestionnaireListItem[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let successMessage = $state<string | null>(null);

	onMount(async () => {
		await loadQuestionnaires();
	});

	async function loadQuestionnaires() {
		loading = true;
		error = null;
		try {
			questionnaires = await adminApi.getQuestionnaires();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load questionnaires';
		} finally {
			loading = false;
		}
	}

	async function handleDelete(id: string, title: string) {
		if (!confirm(`Are you sure you want to delete "${title}"? This action cannot be undone.`)) {
			return;
		}

		try {
			await adminApi.deleteQuestionnaire(id);
			successMessage = `Questionnaire "${title}" deleted successfully`;
			await loadQuestionnaires();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to delete questionnaire';
		}
	}

	function handleCreate() {
		goto('/admin/questionnaires/new');
	}

	function handleEdit(id: string) {
		goto(`/admin/questionnaires/${id}`);
	}
</script>

<div class="space-y-6">
	<!-- Header -->
	<div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
		<div>
			<h1 class="text-3xl font-bold tracking-tight">Questionnaires</h1>
			<p class="text-muted-foreground">
				Create and manage pre-test and post-test questionnaires
			</p>
		</div>
		<Button onclick={handleCreate}>
			<Plus class="mr-2 h-4 w-4" />
			Create Questionnaire
		</Button>
	</div>

	<!-- Success Message -->
	{#if successMessage}
		<Alert.Root>
			<CheckCircle2 class="h-4 w-4" />
			<Alert.Title>Success</Alert.Title>
			<Alert.Description class="flex items-center justify-between">
				<span>{successMessage}</span>
				<Button variant="ghost" size="sm" onclick={() => (successMessage = null)}>
					Dismiss
				</Button>
			</Alert.Description>
		</Alert.Root>
	{/if}

	<!-- Error Message -->
	{#if error}
		<Alert.Root variant="destructive">
			<AlertCircle class="h-4 w-4" />
			<Alert.Title>Error</Alert.Title>
			<Alert.Description class="flex items-center justify-between">
				<span>{error}</span>
				<Button variant="ghost" size="sm" onclick={() => (error = null)}>
					Dismiss
				</Button>
			</Alert.Description>
		</Alert.Root>
	{/if}

	<!-- Questionnaires List -->
	<Card.Root>
		{#if loading}
			<Card.Content class="flex flex-col items-center justify-center py-12">
				<Loader2 class="h-8 w-8 animate-spin text-primary" />
				<p class="mt-2 text-sm text-muted-foreground">Loading questionnaires...</p>
			</Card.Content>
		{:else if questionnaires.length === 0}
			<Card.Content class="flex flex-col items-center justify-center py-12">
				<FileText class="h-12 w-12 text-muted-foreground" />
				<h3 class="mt-4 text-lg font-semibold">No questionnaires</h3>
				<p class="mt-1 text-sm text-muted-foreground">Get started by creating a new questionnaire.</p>
				<Button class="mt-6" onclick={handleCreate}>
					<Plus class="mr-2 h-4 w-4" />
					Create Questionnaire
				</Button>
			</Card.Content>
		{:else}
			<Table.Root>
				<Table.Header>
					<Table.Row>
						<Table.Head>Title</Table.Head>
						<Table.Head>Type</Table.Head>
						<Table.Head>Questions</Table.Head>
						<Table.Head>Status</Table.Head>
						<Table.Head>Last Updated</Table.Head>
						<Table.Head class="text-right">Actions</Table.Head>
					</Table.Row>
				</Table.Header>
				<Table.Body>
					{#each questionnaires as questionnaire}
						<Table.Row>
							<Table.Cell>
								<div class="font-medium">{questionnaire.title}</div>
								{#if questionnaire.description}
									<div class="text-sm text-muted-foreground">{questionnaire.description}</div>
								{/if}
							</Table.Cell>
							<Table.Cell>
								<Badge variant={questionnaire.type === 'Pretest' ? 'default' : 'secondary'}>
									{questionnaire.type}
								</Badge>
							</Table.Cell>
							<Table.Cell class="text-muted-foreground">
								{questionnaire.questionCount} questions
							</Table.Cell>
							<Table.Cell>
								<Badge variant={questionnaire.isActive ? 'default' : 'outline'}>
									{questionnaire.isActive ? 'Active' : 'Inactive'}
								</Badge>
							</Table.Cell>
							<Table.Cell class="text-muted-foreground">
								{new Date(questionnaire.updatedAt).toLocaleDateString()}
							</Table.Cell>
							<Table.Cell class="text-right">
								<div class="flex justify-end gap-2">
									<Button
										variant="ghost"
										size="sm"
										onclick={() => handleEdit(questionnaire.id)}
									>
										Edit
									</Button>
									<Button
										variant="ghost"
										size="sm"
										class="text-destructive hover:text-destructive"
										onclick={() => handleDelete(questionnaire.id, questionnaire.title)}
									>
										Delete
									</Button>
								</div>
							</Table.Cell>
						</Table.Row>
					{/each}
				</Table.Body>
			</Table.Root>
		{/if}
	</Card.Root>
</div>

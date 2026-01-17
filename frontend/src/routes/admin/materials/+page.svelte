<script lang="ts">
	import { onMount } from 'svelte';
	import { adminApi, type AdminMaterial, type MaterialType } from '$lib/api/admin';
	import MaterialForm from '$lib/components/MaterialForm.svelte';
	import * as Button from '$lib/components/ui/button';
	import * as Card from '$lib/components/ui/card';
	import * as Alert from '$lib/components/ui/alert';
	import * as Select from '$lib/components/ui/select';
	import * as Label from '$lib/components/ui/label';
	import * as Table from '$lib/components/ui/table';
	import { Badge } from '$lib/components/ui/badge';

	let materials = $state<AdminMaterial[]>([]);
	let isLoading = $state(true);
	let error = $state<string | null>(null);
	let successMessage = $state<string | null>(null);
	let typeFilter = $state<MaterialType | ''>('');

	// Upload state
	let showUploadForm = $state(false);
	let isUploading = $state(false);
	let uploadProgress = $state(0);

	// Edit state
	let editingMaterial = $state<AdminMaterial | null>(null);
	let isEditing = $state(false);

	onMount(async () => {
		await loadMaterials();
	});

	async function loadMaterials() {
		isLoading = true;
		error = null;

		try {
			materials = await adminApi.getMaterials(typeFilter || undefined);
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load materials';
		} finally {
			isLoading = false;
		}
	}

	async function handleUpload(data: {
		title: string;
		description: string | null;
		type: MaterialType;
		file?: File;
	}) {
		if (!data.file) {
			error = 'Please select a file to upload';
			return;
		}

		isUploading = true;
		uploadProgress = 0;
		error = null;
		successMessage = null;

		try {
			await adminApi.uploadMaterial(data.file, data.title, data.type, data.description, (progress) => {
				uploadProgress = progress;
			});
			successMessage = 'Material uploaded successfully';
			showUploadForm = false;
			uploadProgress = 0;
			await loadMaterials();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to upload material';
		} finally {
			isUploading = false;
		}
	}

	async function handleUpdate(data: {
		title: string;
		description: string | null;
		type: MaterialType;
		orderIndex?: number;
		isActive?: boolean;
	}) {
		if (!editingMaterial) return;

		isEditing = true;
		error = null;
		successMessage = null;

		try {
			await adminApi.updateMaterial(editingMaterial.id, {
				title: data.title,
				description: data.description,
				type: data.type,
				orderIndex: data.orderIndex ?? editingMaterial.orderIndex,
				isActive: data.isActive ?? editingMaterial.isActive
			});
			successMessage = 'Material updated successfully';
			editingMaterial = null;
			await loadMaterials();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to update material';
		} finally {
			isEditing = false;
		}
	}

	async function handleDelete(material: AdminMaterial) {
		if (!confirm(`Are you sure you want to delete "${material.title}"? This action cannot be undone.`)) {
			return;
		}

		error = null;
		successMessage = null;

		try {
			await adminApi.deleteMaterial(material.id);
			successMessage = 'Material deleted successfully';
			await loadMaterials();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to delete material';
		}
	}

	async function handleView(material: AdminMaterial) {
		try {
			const detail = await adminApi.getMaterialDetail(material.id);
			window.open(detail.signedUrl, '_blank');
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to get material URL';
		}
	}

	function formatFileSize(bytes: number | null): string {
		if (!bytes) return '-';
		if (bytes < 1024) return `${bytes} B`;
		if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
		if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
		return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`;
	}

	function formatDate(dateString: string): string {
		return new Date(dateString).toLocaleDateString('en-US', {
			year: 'numeric',
			month: 'short',
			day: 'numeric'
		});
	}

	function getTypeBadgeVariant(type: MaterialType): 'default' | 'secondary' | 'outline' {
		switch (type) {
			case 'Pdf':
				return 'default';
			case 'Video':
				return 'secondary';
			case 'Text':
				return 'outline';
			default:
				return 'outline';
		}
	}
</script>

<div class="max-w-7xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
	<div class="mb-8">
		<div class="flex justify-between items-center">
			<div>
				<h1 class="text-3xl font-bold mb-2">Materials Management</h1>
				<p class="text-muted-foreground">Upload and manage study materials for students.</p>
			</div>
			<Button.Root onclick={() => { showUploadForm = !showUploadForm; editingMaterial = null; }}>
				{showUploadForm ? 'Cancel' : 'Upload Material'}
			</Button.Root>
		</div>
	</div>

	{#if successMessage}
		<Alert.Root class="mb-6 border-green-500 bg-green-50 text-green-900">
			<div class="flex items-center">
				<svg class="h-5 w-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"
					></path>
				</svg>
				<span>{successMessage}</span>
			</div>
		</Alert.Root>
	{/if}

	{#if error}
		<Alert.Root variant="destructive" class="mb-6">
			<div class="flex items-start">
				<svg class="h-5 w-5 mr-2 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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

	{#if showUploadForm}
		<Card.Root class="mb-6 p-6">
			<h2 class="text-lg font-semibold mb-4">Upload New Material</h2>
			<MaterialForm
				onSubmit={handleUpload}
				onCancel={() => (showUploadForm = false)}
				isSubmitting={isUploading}
				uploadProgress={uploadProgress}
			/>
		</Card.Root>
	{/if}

	{#if editingMaterial}
		<Card.Root class="mb-6 p-6">
			<h2 class="text-lg font-semibold mb-4">Edit Material: {editingMaterial.title}</h2>
			<MaterialForm
				material={editingMaterial}
				onSubmit={handleUpdate}
				onCancel={() => (editingMaterial = null)}
				isSubmitting={isEditing}
			/>
		</Card.Root>
	{/if}

	<!-- Filters -->
	<Card.Root class="mb-6 p-4">
		<div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
			<div>
				<Label.Root for="typeFilter" class="mb-2">Filter by Type</Label.Root>
				<Select.Root bind:value={typeFilter as any}>
					<Select.Trigger id="typeFilter" onchange={loadMaterials}>
						{typeFilter || 'All Types'}
					</Select.Trigger>
					<Select.Content>
						<Select.Item value="">All Types</Select.Item>
						<Select.Item value="Pdf">PDF</Select.Item>
						<Select.Item value="Video">Video</Select.Item>
						<Select.Item value="Text">Text</Select.Item>
					</Select.Content>
				</Select.Root>
			</div>
			<div class="flex items-end">
				<Button.Root
					variant="outline"
					class="w-full"
					onclick={() => {
						typeFilter = '';
						loadMaterials();
					}}
				>
					Clear Filters
				</Button.Root>
			</div>
		</div>
	</Card.Root>

	{#if isLoading}
		<Card.Root class="flex items-center justify-center py-12">
			<svg class="animate-spin h-12 w-12 text-primary" fill="none" viewBox="0 0 24 24">
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
	{:else if materials.length === 0}
		<Alert.Root class="text-center border-yellow-500 bg-yellow-50 text-yellow-900">
			<p class="font-medium">No materials found</p>
			<p class="text-sm mt-1">Upload your first material to get started.</p>
		</Alert.Root>
	{:else}
		<Card.Root>
			<Table.Root>
				<Table.Header>
					<Table.Row>
						<Table.Head>Title</Table.Head>
						<Table.Head>Type</Table.Head>
						<Table.Head>Size</Table.Head>
						<Table.Head>Access Count</Table.Head>
						<Table.Head>Created</Table.Head>
						<Table.Head>Status</Table.Head>
						<Table.Head>Actions</Table.Head>
					</Table.Row>
				</Table.Header>
				<Table.Body>
					{#each materials as material}
						<Table.Row>
							<Table.Cell class="font-medium">{material.title}</Table.Cell>
							<Table.Cell>
								<Badge variant={getTypeBadgeVariant(material.type)}>
									{material.type}
								</Badge>
							</Table.Cell>
							<Table.Cell>{formatFileSize(material.fileSizeBytes)}</Table.Cell>
							<Table.Cell>{material.accessCount}</Table.Cell>
							<Table.Cell>{formatDate(material.createdAt)}</Table.Cell>
							<Table.Cell>
								{#if material.isActive}
									<Badge variant="default">Active</Badge>
								{:else}
									<Badge variant="secondary">Inactive</Badge>
								{/if}
							</Table.Cell>
							<Table.Cell>
								<div class="flex gap-2">
									<Button.Root variant="outline" size="sm" onclick={() => handleView(material)}>
										View
									</Button.Root>
									<Button.Root
										variant="outline"
										size="sm"
										onclick={() => {
											editingMaterial = material;
											showUploadForm = false;
										}}
									>
										Edit
									</Button.Root>
									<Button.Root
										variant="destructive"
										size="sm"
										onclick={() => handleDelete(material)}
									>
										Delete
									</Button.Root>
								</div>
							</Table.Cell>
						</Table.Row>
					{/each}
				</Table.Body>
			</Table.Root>
		</Card.Root>
	{/if}
</div>

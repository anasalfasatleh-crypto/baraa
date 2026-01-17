<script lang="ts">
	import * as Button from '$lib/components/ui/button';
	import * as Input from '$lib/components/ui/input';
	import * as Label from '$lib/components/ui/label';
	import * as Select from '$lib/components/ui/select';
	import type { AdminMaterial, MaterialType } from '$lib/api/admin';

	interface Props {
		material?: AdminMaterial;
		isSubmitting: boolean;
		uploadProgress?: number;
		onSubmit: (data: {
			title: string;
			description: string | null;
			type: MaterialType;
			file?: File;
			orderIndex?: number;
			isActive?: boolean;
		}) => void;
		onCancel: () => void;
	}

	let { material, isSubmitting, uploadProgress = 0, onSubmit, onCancel }: Props = $props();

	const isEditMode = $derived(!!material);

	let title = $state('');
	let description = $state('');
	let type = $state<MaterialType>('Pdf');
	let orderIndex = $state(0);
	let isActiveStr = $state('true');
	const isActive = $derived(isActiveStr === 'true');
	let file = $state<File | null>(null);
	let fileError = $state<string | null>(null);

	// Sync form state when material changes (for edit mode)
	$effect(() => {
		if (material) {
			title = material.title;
			description = material.description ?? '';
			type = material.type;
			orderIndex = material.orderIndex;
			isActiveStr = material.isActive === false ? 'false' : 'true';
		} else {
			title = '';
			description = '';
			type = 'Pdf';
			orderIndex = 0;
			isActiveStr = 'true';
		}
		file = null;
		fileError = null;
	});

	const allowedExtensions = ['.pdf', '.mp4', '.avi', '.mov', '.wmv', '.txt', '.doc', '.docx'];
	const maxFileSize = 1024 * 1024 * 1024; // 1GB

	function validateFile(selectedFile: File): boolean {
		const extension = '.' + selectedFile.name.split('.').pop()?.toLowerCase();

		if (!allowedExtensions.includes(extension)) {
			fileError = `File type not supported. Allowed: ${allowedExtensions.join(', ')}`;
			return false;
		}

		if (selectedFile.size > maxFileSize) {
			fileError = 'File size must not exceed 1GB';
			return false;
		}

		fileError = null;
		return true;
	}

	function handleFileChange(event: Event) {
		const input = event.target as HTMLInputElement;
		const selectedFile = input.files?.[0];

		if (selectedFile) {
			if (validateFile(selectedFile)) {
				file = selectedFile;

				// Auto-detect type from extension
				const extension = '.' + selectedFile.name.split('.').pop()?.toLowerCase();
				if (extension === '.pdf') {
					type = 'Pdf';
				} else if (['.mp4', '.avi', '.mov', '.wmv'].includes(extension)) {
					type = 'Video';
				} else if (['.txt', '.doc', '.docx'].includes(extension)) {
					type = 'Text';
				}
			} else {
				file = null;
				input.value = '';
			}
		}
	}

	function handleSubmit(e: Event) {
		e.preventDefault();

		if (!isEditMode && !file) {
			fileError = 'Please select a file to upload';
			return;
		}

		onSubmit({
			title,
			description: description || null,
			type,
			file: file || undefined,
			orderIndex: isEditMode ? orderIndex : undefined,
			isActive: isEditMode ? isActive : undefined
		});
	}

	function formatFileSize(bytes: number): string {
		if (bytes < 1024) return `${bytes} B`;
		if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
		if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
		return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`;
	}
</script>

<form onsubmit={handleSubmit} class="space-y-6">
	<div class="grid grid-cols-1 gap-6 sm:grid-cols-2">
		<div class="space-y-2">
			<Label.Root for="title">
				Title <span class="text-destructive">*</span>
			</Label.Root>
			<Input.Root
				id="title"
				type="text"
				bind:value={title}
				maxlength={200}
				required
				disabled={isSubmitting}
				placeholder="Enter material title"
			/>
		</div>

		<div class="space-y-2">
			<Label.Root for="type">
				Type <span class="text-destructive">*</span>
			</Label.Root>
			<Select.Root bind:value={type as any} disabled={isSubmitting}>
				<Select.Trigger id="type">{type || 'Select type'}</Select.Trigger>
				<Select.Content>
					<Select.Item value="Pdf">PDF</Select.Item>
					<Select.Item value="Video">Video</Select.Item>
					<Select.Item value="Text">Text</Select.Item>
				</Select.Content>
			</Select.Root>
		</div>
	</div>

	<div class="space-y-2">
		<Label.Root for="description">Description</Label.Root>
		<textarea
			id="description"
			bind:value={description}
			maxlength={1000}
			disabled={isSubmitting}
			placeholder="Enter material description (optional)"
			rows={3}
			class="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
		></textarea>
		<p class="text-xs text-muted-foreground">{description.length}/1000 characters</p>
	</div>

	{#if !isEditMode}
		<div class="space-y-2">
			<Label.Root for="file">
				File <span class="text-destructive">*</span>
			</Label.Root>
			<input
				id="file"
				type="file"
				onchange={handleFileChange}
				disabled={isSubmitting}
				accept=".pdf,.mp4,.avi,.mov,.wmv,.txt,.doc,.docx"
				class="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
			/>
			{#if fileError}
				<p class="text-sm text-destructive">{fileError}</p>
			{/if}
			{#if file}
				<p class="text-sm text-muted-foreground">
					Selected: {file.name} ({formatFileSize(file.size)})
				</p>
			{/if}
			<p class="text-xs text-muted-foreground">
				Supported formats: PDF, Video (MP4, AVI, MOV, WMV), Text (TXT, DOC, DOCX). Max size: 1GB.
			</p>
		</div>

		{#if isSubmitting && uploadProgress > 0}
			<div class="space-y-2">
				<div class="flex justify-between text-sm">
					<span>Uploading...</span>
					<span>{uploadProgress}%</span>
				</div>
				<div class="w-full bg-secondary rounded-full h-2">
					<div
						class="bg-primary h-2 rounded-full transition-all duration-300"
						style="width: {uploadProgress}%"
					></div>
				</div>
			</div>
		{/if}
	{:else}
		<div class="grid grid-cols-1 gap-6 sm:grid-cols-2">
			<div class="space-y-2">
				<Label.Root for="orderIndex">Order Index</Label.Root>
				<Input.Root
					id="orderIndex"
					type="number"
					bind:value={orderIndex}
					min={0}
					disabled={isSubmitting}
				/>
				<p class="text-xs text-muted-foreground">Lower numbers appear first</p>
			</div>

			<div class="space-y-2">
				<Label.Root for="isActive">Status</Label.Root>
				<Select.Root bind:value={isActiveStr as any} disabled={isSubmitting}>
					<Select.Trigger id="isActive">{isActive ? 'Active' : 'Inactive'}</Select.Trigger>
					<Select.Content>
						<Select.Item value="true">Active</Select.Item>
						<Select.Item value="false">Inactive</Select.Item>
					</Select.Content>
				</Select.Root>
			</div>
		</div>
	{/if}

	<div class="flex justify-end gap-3">
		<Button.Root type="button" variant="outline" onclick={onCancel} disabled={isSubmitting}>
			Cancel
		</Button.Root>
		<Button.Root type="submit" disabled={isSubmitting || (!isEditMode && !file)}>
			{#if isSubmitting}
				{isEditMode ? 'Saving...' : 'Uploading...'}
			{:else}
				{isEditMode ? 'Save Changes' : 'Upload Material'}
			{/if}
		</Button.Root>
	</div>
</form>

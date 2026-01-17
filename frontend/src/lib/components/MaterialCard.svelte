<script lang="ts">
	import { goto } from '$app/navigation';
	import type { Material } from '$lib/api/materials';

	interface Props {
		material: Material;
	}

	let { material }: Props = $props();

	function handleClick() {
		goto(`/student/materials/${material.id}`);
	}

	function getIconForType(type: string) {
		switch (type) {
			case 'Pdf':
				return '📄';
			case 'Video':
				return '🎥';
			case 'Text':
				return '📝';
			default:
				return '📁';
		}
	}

	function formatFileSize(bytes: number | null): string {
		if (!bytes) return 'Unknown size';
		const mb = bytes / (1024 * 1024);
		if (mb < 1) {
			const kb = bytes / 1024;
			return `${kb.toFixed(1)} KB`;
		}
		return `${mb.toFixed(1)} MB`;
	}
</script>

<button
	type="button"
	class="w-full bg-white rounded-lg shadow-sm border border-gray-200 p-6 hover:shadow-md hover:border-indigo-300 transition-all text-left"
	onclick={handleClick}
>
	<div class="flex items-start gap-4">
		<div class="text-4xl">{getIconForType(material.type)}</div>
		<div class="flex-1 min-w-0">
			<h3 class="text-lg font-semibold text-gray-900 mb-1">
				{material.title}
			</h3>
			{#if material.description}
				<p class="text-sm text-gray-600 line-clamp-2 mb-3">
					{material.description}
				</p>
			{/if}
			<div class="flex items-center gap-4 text-sm text-gray-500">
				<span class="flex items-center gap-1">
					<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"
						/>
					</svg>
					{material.type}
				</span>
				{#if material.fileSizeBytes}
					<span>{formatFileSize(material.fileSizeBytes)}</span>
				{/if}
				<span class="flex items-center gap-1">
					<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
						/>
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
						/>
					</svg>
					Viewed {material.accessCount} {material.accessCount === 1 ? 'time' : 'times'}
				</span>
			</div>
		</div>
		<div>
			<svg class="w-6 h-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path
					stroke-linecap="round"
					stroke-linejoin="round"
					stroke-width="2"
					d="M9 5l7 7-7 7"
				/>
			</svg>
		</div>
	</div>
</button>

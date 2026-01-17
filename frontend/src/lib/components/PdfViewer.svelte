<script lang="ts">
	interface Props {
		url: string;
		title: string;
	}

	let { url, title }: Props = $props();
</script>

<div class="w-full h-full min-h-[600px] bg-gray-50 rounded-lg overflow-hidden">
	<iframe
		src={url}
		{title}
		class="w-full h-full min-h-[600px]"
		style="border: none;"
	></iframe>
</div>

<p class="mt-4 text-sm text-gray-600 text-center">
	Having trouble viewing? <a href={url} target="_blank" rel="noopener noreferrer" class="text-indigo-600 hover:text-indigo-700 underline">Open in new tab</a>
</p>

<script lang="ts">
	interface Props {
		url: string;
		title: string;
		ontimeupdate?: (currentTime: number) => void;
		onended?: () => void;
	}

	let { url, title, ontimeupdate, onended }: Props = $props();

	let videoElement: HTMLVideoElement;

	function handleTimeUpdate() {
		if (ontimeupdate && videoElement) {
			ontimeupdate(videoElement.currentTime);
		}
	}

	function handleEnded() {
		if (onended) {
			onended();
		}
	}
</script>

<div class="w-full bg-black rounded-lg overflow-hidden">
	<video
		bind:this={videoElement}
		src={url}
		controls
		class="w-full"
		preload="metadata"
		ontimeupdate={handleTimeUpdate}
		onended={handleEnded}
	>
		<track kind="captions" />
		Your browser does not support the video tag.
	</video>
</div>

<p class="mt-2 text-sm text-gray-600">
	{title}
</p>

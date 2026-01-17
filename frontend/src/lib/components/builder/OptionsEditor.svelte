<script lang="ts">
	import { Button } from '$lib/components/ui/button';
	import { Input } from '$lib/components/ui/input';
	import { Label } from '$lib/components/ui/label';
	import * as Alert from '$lib/components/ui/alert';
	import { Plus, Trash2, AlertTriangle } from '@lucide/svelte';

	interface Props {
		options: string[];
		onChange: (options: string[]) => void;
		disabled?: boolean;
	}

	let { options, onChange, disabled = false }: Props = $props();

	function addOption() {
		onChange([...options, '']);
	}

	function removeOption(index: number) {
		const newOptions = options.filter((_, i) => i !== index);
		onChange(newOptions);
	}

	function updateOption(index: number, value: string) {
		const newOptions = [...options];
		newOptions[index] = value;
		onChange(newOptions);
	}

	const hasEmptyOptions = $derived(options.length > 0 && options.some((opt) => !opt.trim()));
</script>

<div class="space-y-3">
	<div class="flex items-center justify-between">
		<Label>Options</Label>
		<Button variant="secondary" size="sm" onclick={addOption} {disabled}>
			<Plus class="mr-2 h-4 w-4" />
			Add Option
		</Button>
	</div>

	{#if options.length === 0}
		<p class="text-sm text-muted-foreground italic">
			No options yet. Click "Add Option" to create one.
		</p>
	{:else}
		<div class="space-y-2">
			{#each options as option, index}
				<div class="flex items-center gap-2">
					<span class="text-sm text-muted-foreground w-8">#{index + 1}</span>
					<Input
						value={option}
						oninput={(e) => updateOption(index, e.currentTarget.value)}
						placeholder="Enter option text..."
						{disabled}
						class="flex-1"
					/>
					<Button
						variant="ghost"
						size="icon"
						onclick={() => removeOption(index)}
						{disabled}
						class="text-destructive hover:text-destructive"
					>
						<Trash2 class="h-4 w-4" />
					</Button>
				</div>
			{/each}
		</div>
	{/if}

	{#if hasEmptyOptions}
		<Alert.Root>
			<AlertTriangle class="h-4 w-4" />
			<Alert.Description>
				Some options are empty. Please fill them in or remove them.
			</Alert.Description>
		</Alert.Root>
	{/if}
</div>

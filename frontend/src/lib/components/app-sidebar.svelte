<script lang="ts">
	import * as Sidebar from '$lib/components/ui/sidebar';
	import { auth } from '$lib/stores/auth';
	import { page } from '$app/stores';
	import { authApi } from '$lib/api/auth';
	import { goto } from '$app/navigation';
	import {
		LayoutDashboard,
		Users,
		ClipboardList,
		Download,
		FileQuestion,
		BookOpen,
		FileCheck,
		LogOut
	} from '@lucide/svelte';

	async function handleLogout() {
		await authApi.logout();
		goto('/login');
	}

	const user = $derived($auth.user);

	// Role-based navigation items with icons
	const navigationItems = $derived(
		user
			? user.role === 'Admin'
				? [
						{ icon: LayoutDashboard, href: '/admin/dashboard', label: 'Dashboard' },
						{ icon: Users, href: '/admin/users', label: 'Users' },
						{ icon: ClipboardList, href: '/admin/questionnaires', label: 'Questionnaires' },
						{ icon: BookOpen, href: '/admin/materials', label: 'Materials' },
						{ icon: Download, href: '/admin/export', label: 'Export Data' }
				  ]
				: user.role === 'Evaluator'
				? [
						{ icon: LayoutDashboard, href: '/evaluator', label: 'Dashboard' },
						{ icon: Users, href: '/evaluator/students', label: 'Students' }
				  ]
				: user.role === 'Student'
				? [
						{ icon: LayoutDashboard, href: '/student', label: 'Dashboard' },
						{ icon: FileQuestion, href: '/student/pretest', label: 'Pre-Test' },
						{ icon: BookOpen, href: '/student/materials', label: 'Materials' },
						{ icon: FileCheck, href: '/student/posttest', label: 'Post-Test' }
				  ]
				: []
			: []
	);

	const currentPath = $derived($page.url.pathname);
</script>

<Sidebar.Root>
	<Sidebar.Header>
		<div class="flex items-center gap-2 px-4 py-4">
			<h2 class="text-lg font-semibold">Research Platform</h2>
		</div>
	</Sidebar.Header>

	<Sidebar.Content>
		<Sidebar.Group>
			<Sidebar.GroupLabel>Navigation</Sidebar.GroupLabel>
			<Sidebar.GroupContent>
				<Sidebar.Menu>
					{#each navigationItems as item}
						<Sidebar.MenuItem>
							<a href={item.href} class="block">
								<Sidebar.MenuButton isActive={currentPath === item.href}>
									<item.icon class="mr-2 h-4 w-4" />
									<span>{item.label}</span>
								</Sidebar.MenuButton>
							</a>
						</Sidebar.MenuItem>
					{/each}
				</Sidebar.Menu>
			</Sidebar.GroupContent>
		</Sidebar.Group>
	</Sidebar.Content>

	<Sidebar.Footer>
		<Sidebar.Group>
			<div class="px-4 py-2 text-sm text-muted-foreground border-t">
				<div class="font-medium">{user?.name}</div>
				<div class="text-xs">{user?.role}</div>
			</div>
			<Sidebar.Menu>
				<Sidebar.MenuItem>
					<button onclick={handleLogout} class="w-full">
						<Sidebar.MenuButton>
							<LogOut class="mr-2 h-4 w-4" />
							<span>Logout</span>
						</Sidebar.MenuButton>
					</button>
				</Sidebar.MenuItem>
			</Sidebar.Menu>
		</Sidebar.Group>
	</Sidebar.Footer>
</Sidebar.Root>

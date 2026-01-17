<script lang="ts">
	import type { DashboardMetrics } from '$lib/api/admin';

	interface Props {
		metrics: DashboardMetrics;
	}

	let { metrics }: Props = $props();
</script>

<div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
	<!-- Total Students -->
	<div class="bg-white overflow-hidden shadow rounded-lg">
		<div class="p-5">
			<div class="flex items-center">
				<div class="flex-shrink-0">
					<svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
						/>
					</svg>
				</div>
				<div class="ml-5 w-0 flex-1">
					<dl>
						<dt class="text-sm font-medium text-gray-500 truncate">Total Students</dt>
						<dd class="text-3xl font-semibold text-gray-900">{metrics.totalStudents}</dd>
					</dl>
				</div>
			</div>
		</div>
	</div>

	<!-- Total Evaluators -->
	<div class="bg-white overflow-hidden shadow rounded-lg">
		<div class="p-5">
			<div class="flex items-center">
				<div class="flex-shrink-0">
					<svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
						/>
					</svg>
				</div>
				<div class="ml-5 w-0 flex-1">
					<dl>
						<dt class="text-sm font-medium text-gray-500 truncate">Total Evaluators</dt>
						<dd class="text-3xl font-semibold text-gray-900">{metrics.totalEvaluators}</dd>
					</dl>
				</div>
			</div>
		</div>
	</div>

	<!-- Pre-test Completed -->
	<div class="bg-white overflow-hidden shadow rounded-lg">
		<div class="p-5">
			<div class="flex items-center">
				<div class="flex-shrink-0">
					<svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
						/>
					</svg>
				</div>
				<div class="ml-5 w-0 flex-1">
					<dl>
						<dt class="text-sm font-medium text-gray-500 truncate">Pre-test Completed</dt>
						<dd class="text-3xl font-semibold text-gray-900">{metrics.pretestCompletedCount}</dd>
					</dl>
				</div>
			</div>
		</div>
	</div>

	<!-- Post-test Completed -->
	<div class="bg-white overflow-hidden shadow rounded-lg">
		<div class="p-5">
			<div class="flex items-center">
				<div class="flex-shrink-0">
					<svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"
						/>
					</svg>
				</div>
				<div class="ml-5 w-0 flex-1">
					<dl>
						<dt class="text-sm font-medium text-gray-500 truncate">Post-test Completed</dt>
						<dd class="text-3xl font-semibold text-gray-900">{metrics.posttestCompletedCount}</dd>
					</dl>
				</div>
			</div>
		</div>
	</div>

	<!-- Materials Accessed -->
	<div class="bg-white overflow-hidden shadow rounded-lg">
		<div class="p-5">
			<div class="flex items-center">
				<div class="flex-shrink-0">
					<svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"
						/>
					</svg>
				</div>
				<div class="ml-5 w-0 flex-1">
					<dl>
						<dt class="text-sm font-medium text-gray-500 truncate">Materials Accessed</dt>
						<dd class="text-3xl font-semibold text-gray-900">{metrics.materialsAccessedCount}</dd>
					</dl>
				</div>
			</div>
		</div>
	</div>

	<!-- Evaluator Assignments -->
	<div class="bg-white overflow-hidden shadow rounded-lg">
		<div class="p-5">
			<div class="flex items-center">
				<div class="flex-shrink-0">
					<svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
						/>
					</svg>
				</div>
				<div class="ml-5 w-0 flex-1">
					<dl>
						<dt class="text-sm font-medium text-gray-500 truncate">Assignments</dt>
						<dd class="text-3xl font-semibold text-gray-900">{metrics.totalEvaluatorAssignments}</dd>
					</dl>
				</div>
			</div>
		</div>
	</div>

	<!-- Scoring Progress -->
	<div class="bg-white overflow-hidden shadow rounded-lg">
		<div class="p-5">
			<div class="flex items-center">
				<div class="flex-shrink-0">
					<svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
						/>
					</svg>
				</div>
				<div class="ml-5 w-0 flex-1">
					<dl>
						<dt class="text-sm font-medium text-gray-500 truncate">Scoring Progress</dt>
						<dd class="text-3xl font-semibold text-gray-900">
							{metrics.scoringCompletionRate.toFixed(1)}%
						</dd>
					</dl>
				</div>
			</div>
		</div>
	</div>

	<!-- Post-test Status -->
	<div class="bg-white overflow-hidden shadow rounded-lg">
		<div class="p-5">
			<div class="flex items-center">
				<div class="flex-shrink-0">
					<svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
						/>
					</svg>
				</div>
				<div class="ml-5 w-0 flex-1">
					<dl>
						<dt class="text-sm font-medium text-gray-500 truncate">Post-test Batch</dt>
						<dd class="flex items-center">
							<span
								class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {metrics.posttestBatchOpen
									? 'bg-green-100 text-green-800'
									: 'bg-red-100 text-red-800'}"
							>
								{metrics.posttestBatchOpen ? 'Open' : 'Closed'}
							</span>
						</dd>
					</dl>
				</div>
			</div>
		</div>
	</div>
</div>

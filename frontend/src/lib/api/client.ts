import { PUBLIC_API_URL } from '$env/static/public';
import { auth } from '$lib/stores/auth';
import { get } from 'svelte/store';

export class ApiError extends Error {
	constructor(
		public status: number,
		public message: string,
		public details?: unknown
	) {
		super(message);
		this.name = 'ApiError';
	}
}

interface RequestOptions extends RequestInit {
	requiresAuth?: boolean;
}

async function request<T>(
	endpoint: string,
	options: RequestOptions = {}
): Promise<T> {
	const { requiresAuth = true, ...fetchOptions } = options;

	const headers = new Headers(fetchOptions.headers);
	headers.set('Content-Type', 'application/json');

	if (requiresAuth) {
		const authState = get(auth);
		if (authState.accessToken) {
			headers.set('Authorization', `Bearer ${authState.accessToken}`);
		}
	}

	const url = `${PUBLIC_API_URL}${endpoint}`;

	try {
		const response = await fetch(url, {
			...fetchOptions,
			headers,
			credentials: 'include' // Include cookies for refresh token
		});

		if (!response.ok) {
			const error = await response.json().catch(() => ({ error: 'Request failed' }));
			throw new ApiError(response.status, error.message || error.error, error);
		}

		// Handle 204 No Content
		if (response.status === 204) {
			return null as T;
		}

		return await response.json();
	} catch (error) {
		if (error instanceof ApiError) {
			throw error;
		}
		throw new ApiError(0, 'Network error occurred');
	}
}

export const api = {
	get: <T>(endpoint: string, options?: RequestOptions) =>
		request<T>(endpoint, { ...options, method: 'GET' }),

	post: <T>(endpoint: string, data?: unknown, options?: RequestOptions) =>
		request<T>(endpoint, {
			...options,
			method: 'POST',
			body: data ? JSON.stringify(data) : undefined
		}),

	put: <T>(endpoint: string, data?: unknown, options?: RequestOptions) =>
		request<T>(endpoint, {
			...options,
			method: 'PUT',
			body: data ? JSON.stringify(data) : undefined
		}),

	patch: <T>(endpoint: string, data?: unknown, options?: RequestOptions) =>
		request<T>(endpoint, {
			...options,
			method: 'PATCH',
			body: data ? JSON.stringify(data) : undefined
		}),

	delete: <T>(endpoint: string, options?: RequestOptions) =>
		request<T>(endpoint, { ...options, method: 'DELETE' })
};

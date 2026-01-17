import type { Handle } from '@sveltejs/kit';
import { redirect } from '@sveltejs/kit';

export const handle: Handle = async ({ event, resolve }) => {
	// Public routes that don't require authentication
	const publicRoutes = ['/login', '/health'];

	// Check if current route is public
	const isPublicRoute = publicRoutes.some(route => event.url.pathname.startsWith(route));

	if (!isPublicRoute) {
		// Check for auth token in cookies or headers
		const authHeader = event.request.headers.get('authorization');
		const hasRefreshToken = event.cookies.get('refreshToken');

		// If no authentication tokens present, redirect to login
		if (!authHeader && !hasRefreshToken) {
			throw redirect(303, '/login');
		}
	}

	const response = await resolve(event);
	return response;
};

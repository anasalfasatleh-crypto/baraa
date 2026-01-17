import { api } from './client';
import { auth, type UserInfo } from '$lib/stores/auth';

export interface LoginRequest {
	email: string;
	password: string;
}

export interface LoginResponse {
	accessToken: string;
	expiresIn: number;
	user: UserInfo;
}

export interface ChangePasswordRequest {
	currentPassword: string;
	newPassword: string;
}

export const authApi = {
	async login(credentials: LoginRequest): Promise<LoginResponse> {
		const response = await api.post<LoginResponse>('/auth/login', credentials, {
			requiresAuth: false
		});

		// Update auth store
		auth.login(response.user, response.accessToken);

		return response;
	},

	async logout(): Promise<void> {
		try {
			await api.post('/auth/logout');
		} finally {
			auth.logout();
		}
	},

	async refreshToken(): Promise<void> {
		const response = await api.post<{ accessToken: string; expiresIn: number }>('/auth/refresh', undefined, {
			requiresAuth: false
		});

		auth.updateToken(response.accessToken);
	},

	async changePassword(data: ChangePasswordRequest): Promise<void> {
		await api.post('/auth/change-password', data);
	}
};

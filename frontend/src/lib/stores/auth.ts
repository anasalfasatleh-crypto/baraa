import { writable } from 'svelte/store';
import { browser } from '$app/environment';

export interface UserInfo {
	id: string;
	email: string;
	name: string;
	role: 'Admin' | 'Evaluator' | 'Student';
}

export interface AuthState {
	user: UserInfo | null;
	accessToken: string | null;
	isAuthenticated: boolean;
}

function createAuthStore() {
	const initialState: AuthState = {
		user: null,
		accessToken: null,
		isAuthenticated: false
	};

	// Load from localStorage if in browser
	if (browser) {
		const storedUser = localStorage.getItem('user');
		const storedToken = localStorage.getItem('accessToken');

		if (storedUser && storedToken) {
			initialState.user = JSON.parse(storedUser);
			initialState.accessToken = storedToken;
			initialState.isAuthenticated = true;
		}
	}

	const { subscribe, set, update } = writable<AuthState>(initialState);

	return {
		subscribe,
		login: (user: UserInfo, accessToken: string) => {
			if (browser) {
				localStorage.setItem('user', JSON.stringify(user));
				localStorage.setItem('accessToken', accessToken);
			}
			set({ user, accessToken, isAuthenticated: true });
		},
		logout: () => {
			if (browser) {
				localStorage.removeItem('user');
				localStorage.removeItem('accessToken');
			}
			set({ user: null, accessToken: null, isAuthenticated: false });
		},
		updateToken: (accessToken: string) => {
			if (browser) {
				localStorage.setItem('accessToken', accessToken);
			}
			update((state) => ({ ...state, accessToken }));
		}
	};
}

export const auth = createAuthStore();

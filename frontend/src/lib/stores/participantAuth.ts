import { writable, derived } from 'svelte/store';
import type { ParticipantProfile, ParticipantLoginResponse } from '$lib/services/participantApi';
import { participantApi } from '$lib/services/participantApi';

// ============================================================
// TYPES
// ============================================================

interface ParticipantAuthState {
	isAuthenticated: boolean;
	isLoading: boolean;
	participant: ParticipantProfile | null;
	mustChangePassword: boolean;
	error: string | null;
}

// ============================================================
// STORE
// ============================================================

const initialState: ParticipantAuthState = {
	isAuthenticated: false,
	isLoading: true,
	participant: null,
	mustChangePassword: false,
	error: null
};

function createParticipantAuthStore() {
	const { subscribe, set, update } = writable<ParticipantAuthState>(initialState);

	return {
		subscribe,

		/**
		 * Initialize auth state from session storage
		 */
		init: async () => {
			const accessToken = sessionStorage.getItem('participant_access_token');
			const refreshToken = sessionStorage.getItem('participant_refresh_token');

			if (!accessToken || !refreshToken) {
				set({ ...initialState, isLoading: false });
				return;
			}

			try {
				const profile = await participantApi.getProfile();
				update((state) => ({
					...state,
					isAuthenticated: true,
					isLoading: false,
					participant: profile,
					error: null
				}));
			} catch {
				// Token invalid, try to refresh
				try {
					const response = await participantApi.refreshToken(refreshToken);
					storeTokens(response.accessToken, response.refreshToken);
					update((state) => ({
						...state,
						isAuthenticated: true,
						isLoading: false,
						participant: response.participant,
						mustChangePassword: response.mustChangePassword,
						error: null
					}));
				} catch {
					// Refresh failed, clear auth
					clearTokens();
					set({ ...initialState, isLoading: false });
				}
			}
		},

		/**
		 * Handle successful login
		 */
		setAuthenticated: (response: ParticipantLoginResponse) => {
			storeTokens(response.accessToken, response.refreshToken);
			update((state) => ({
				...state,
				isAuthenticated: true,
				isLoading: false,
				participant: response.participant,
				mustChangePassword: response.mustChangePassword,
				error: null
			}));
		},

		/**
		 * Handle registration success (auto-login)
		 */
		setRegistered: (
			accessToken: string,
			refreshToken: string,
			participant: { id: string; code: string; loginIdentifier: string }
		) => {
			storeTokens(accessToken, refreshToken);
			update((state) => ({
				...state,
				isAuthenticated: true,
				isLoading: false,
				participant: {
					id: participant.id,
					code: participant.code,
					loginIdentifier: participant.loginIdentifier,
					phoneNumber: null,
					createdAt: new Date().toISOString(),
					lastLoginAt: null
				},
				mustChangePassword: false,
				error: null
			}));
		},

		/**
		 * Clear password change requirement after successful change
		 */
		clearMustChangePassword: () => {
			update((state) => ({
				...state,
				mustChangePassword: false
			}));
		},

		/**
		 * Logout and clear session
		 */
		logout: async () => {
			const refreshToken = sessionStorage.getItem('participant_refresh_token');
			if (refreshToken) {
				try {
					await participantApi.logout(refreshToken);
				} catch {
					// Ignore logout API errors
				}
			}
			clearTokens();
			set({ ...initialState, isLoading: false });
		},

		/**
		 * Set error message
		 */
		setError: (error: string) => {
			update((state) => ({
				...state,
				error,
				isLoading: false
			}));
		},

		/**
		 * Clear error
		 */
		clearError: () => {
			update((state) => ({
				...state,
				error: null
			}));
		},

		/**
		 * Set loading state
		 */
		setLoading: (isLoading: boolean) => {
			update((state) => ({
				...state,
				isLoading
			}));
		}
	};
}

// ============================================================
// HELPERS
// ============================================================

function storeTokens(accessToken: string, refreshToken: string) {
	sessionStorage.setItem('participant_access_token', accessToken);
	sessionStorage.setItem('participant_refresh_token', refreshToken);
}

function clearTokens() {
	sessionStorage.removeItem('participant_access_token');
	sessionStorage.removeItem('participant_refresh_token');
}

// ============================================================
// EXPORTS
// ============================================================

export const participantAuth = createParticipantAuthStore();

// Derived stores for convenience
export const isParticipantAuthenticated = derived(
	participantAuth,
	($auth) => $auth.isAuthenticated
);

export const participantCode = derived(
	participantAuth,
	($auth) => $auth.participant?.code ?? null
);

export const participantProfile = derived(participantAuth, ($auth) => $auth.participant);

export const mustChangePassword = derived(
	participantAuth,
	($auth) => $auth.mustChangePassword
);

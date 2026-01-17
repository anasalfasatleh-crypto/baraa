// Participant API Service
// Handles all API calls for participant registration, authentication, and profile

const API_BASE = '/api/v1';

// ============================================================
// TYPES
// ============================================================

export interface ParticipantRegisterRequest {
	loginIdentifier: string;
	password: string;
	phoneNumber?: string;
}

export interface ParticipantLoginRequest {
	loginIdentifier: string;
	password: string;
}

export interface ParticipantRegisterResponse {
	id: string;
	code: string;
	loginIdentifier: string;
	accessToken: string;
	refreshToken: string;
	expiresIn: number;
}

export interface ParticipantLoginResponse {
	accessToken: string;
	refreshToken: string;
	expiresIn: number;
	mustChangePassword: boolean;
	participant: ParticipantProfile;
}

export interface ParticipantProfile {
	id: string;
	code: string;
	loginIdentifier: string;
	phoneNumber: string | null;
	createdAt: string;
	lastLoginAt: string | null;
}

export interface ParticipantDetails {
	id: string;
	code: string;
	loginIdentifier: string;
	phoneNumber: string | null;
	createdAt: string;
	updatedAt: string;
	lastLoginAt: string | null;
	failedLoginAttempts: number;
	isLocked: boolean;
	lockoutEnd: string | null;
	mustChangePassword: boolean;
}

export interface ParticipantSummary {
	id: string;
	code: string;
	loginIdentifier: string;
	createdAt: string;
}

export interface ParticipantListResponse {
	items: ParticipantSummary[];
	totalCount: number;
	page: number;
	pageSize: number;
	totalPages: number;
}

export interface PasswordResetResponse {
	temporaryPassword: string;
	message: string;
}

export interface AccountLockedResponse {
	message: string;
	lockoutEnd: string;
	remainingSeconds: number;
}

export interface ApiError {
	error: string;
	message: string;
}

// ============================================================
// API CLIENT
// ============================================================

class ParticipantApiClient {
	private getAuthHeader(): HeadersInit {
		const token = sessionStorage.getItem('participant_access_token');
		return token ? { Authorization: `Bearer ${token}` } : {};
	}

	private async handleResponse<T>(response: Response): Promise<T> {
		if (!response.ok) {
			const error = await response.json().catch(() => ({ error: 'Unknown', message: 'An error occurred' }));
			throw { status: response.status, ...error };
		}
		return response.json();
	}

	// ============================================================
	// PUBLIC ENDPOINTS
	// ============================================================

	async register(request: ParticipantRegisterRequest): Promise<ParticipantRegisterResponse> {
		const response = await fetch(`${API_BASE}/participants/register`, {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify(request)
		});
		return this.handleResponse<ParticipantRegisterResponse>(response);
	}

	async login(request: ParticipantLoginRequest): Promise<ParticipantLoginResponse> {
		const response = await fetch(`${API_BASE}/participants/login`, {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify(request)
		});
		return this.handleResponse<ParticipantLoginResponse>(response);
	}

	async refreshToken(refreshToken: string): Promise<ParticipantLoginResponse> {
		const response = await fetch(`${API_BASE}/participants/refresh`, {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ refreshToken })
		});
		return this.handleResponse<ParticipantLoginResponse>(response);
	}

	// ============================================================
	// AUTHENTICATED PARTICIPANT ENDPOINTS
	// ============================================================

	async getProfile(): Promise<ParticipantProfile> {
		const response = await fetch(`${API_BASE}/participants/me`, {
			headers: { ...this.getAuthHeader() }
		});
		return this.handleResponse<ParticipantProfile>(response);
	}

	async changePassword(currentPassword: string, newPassword: string): Promise<void> {
		const response = await fetch(`${API_BASE}/participants/me/change-password`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				...this.getAuthHeader()
			},
			body: JSON.stringify({ currentPassword, newPassword })
		});
		await this.handleResponse<{ message: string }>(response);
	}

	async logout(refreshToken: string): Promise<void> {
		await fetch(`${API_BASE}/participants/logout`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				...this.getAuthHeader()
			},
			body: JSON.stringify({ refreshToken })
		});
	}

	// ============================================================
	// ADMIN ENDPOINTS
	// ============================================================

	async listParticipants(page = 1, pageSize = 20, search?: string): Promise<ParticipantListResponse> {
		const params = new URLSearchParams({
			page: page.toString(),
			pageSize: pageSize.toString()
		});
		if (search) params.append('search', search);

		const response = await fetch(`${API_BASE}/admin/participants?${params}`, {
			headers: { ...this.getAuthHeader() }
		});
		return this.handleResponse<ParticipantListResponse>(response);
	}

	async searchParticipants(query: string): Promise<ParticipantSummary[]> {
		const response = await fetch(`${API_BASE}/admin/participants/search?q=${encodeURIComponent(query)}`, {
			headers: { ...this.getAuthHeader() }
		});
		return this.handleResponse<ParticipantSummary[]>(response);
	}

	async getParticipantDetails(id: string): Promise<ParticipantDetails> {
		const response = await fetch(`${API_BASE}/admin/participants/${id}`, {
			headers: { ...this.getAuthHeader() }
		});
		return this.handleResponse<ParticipantDetails>(response);
	}

	async resetParticipantPassword(id: string): Promise<PasswordResetResponse> {
		const response = await fetch(`${API_BASE}/admin/participants/${id}/reset-password`, {
			method: 'POST',
			headers: { ...this.getAuthHeader() }
		});
		return this.handleResponse<PasswordResetResponse>(response);
	}
}

export const participantApi = new ParticipantApiClient();

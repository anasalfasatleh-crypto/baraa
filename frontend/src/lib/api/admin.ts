import { api } from './client';
import { PUBLIC_API_URL } from '$env/static/public';
import { auth } from '$lib/stores/auth';
import { get } from 'svelte/store';

// Dashboard types
export interface DashboardMetrics {
	totalStudents: number;
	totalEvaluators: number;
	totalAdmins: number;
	pretestCompletedCount: number;
	posttestCompletedCount: number;
	materialsAccessedCount: number;
	totalEvaluatorAssignments: number;
	scoringCompletionRate: number;
	posttestBatchOpen: boolean;
	currentBatchName?: string;
	currentBatchOpenDate?: string;
	currentBatchCloseDate?: string;
}

// User types
export interface AdminUser {
	id: string;
	email: string;
	name: string;
	role: string;
	status: string;
	hospital?: string;
	gender?: string;
	createdAt: string;
	updatedAt: string;
}

export interface CreateUserRequest {
	email: string;
	password: string;
	name: string;
	role: string;
	hospital?: string;
	gender?: string;
}

export interface UpdateUserRequest {
	name?: string;
	role?: string;
	hospital?: string;
	gender?: string;
}

export interface ResetPasswordResponse {
	success: boolean;
	message: string;
	newPassword: string;
}

// Assignment types
export interface EvaluatorAssignment {
	id: string;
	evaluatorId: string;
	evaluatorName: string;
	evaluatorEmail: string;
	studentId: string;
	studentName: string;
	studentEmail: string;
	assignedAt: string;
}

export interface CreateAssignmentRequest {
	evaluatorId: string;
	studentId: string;
}

// Batch types
export interface PostTestBatch {
	id: string;
	name: string;
	description?: string;
	openDate: string;
	closeDate: string;
	isActive: boolean;
	createdAt: string;
}

export interface CreateBatchRequest {
	name: string;
	description?: string;
	openDate: string;
	closeDate: string;
}

// CSV Import types
export interface CsvImportResult {
	totalRows: number;
	successCount: number;
	errors: string[];
	generatedPasswords: UserPasswordInfo[];
}

export interface UserPasswordInfo {
	email: string;
	name: string;
	password: string;
}

// Questionnaire Builder types
export interface QuestionnaireListItem {
	id: string;
	title: string;
	description?: string;
	type: 'Pretest' | 'Posttest';
	isActive: boolean;
	questionCount: number;
	createdAt: string;
	updatedAt: string;
}

export interface QuestionBuilderDto {
	id: string;
	text: string;
	type: 'LikertScale' | 'TrueFalseDontKnow' | 'MultipleChoice' | 'DropdownGrade' | 'ShortText' | 'LongText' | 'EvaluatorOnly';
	options?: string[];
	orderIndex: number;
	step: number;
	isRequired: boolean;
	minValue?: number;
	maxValue?: number;
	minLabel?: string;
	maxLabel?: string;
}

export interface QuestionnaireDetail {
	id: string;
	title: string;
	description?: string;
	type: 'Pretest' | 'Posttest';
	isActive: boolean;
	questions: QuestionBuilderDto[];
	createdAt: string;
	updatedAt: string;
}

export interface CreateQuestionnaireRequest {
	title: string;
	description?: string;
	type: 'Pretest' | 'Posttest';
}

export interface UpdateQuestionnaireRequest {
	title: string;
	description?: string;
	type: 'Pretest' | 'Posttest';
	isActive: boolean;
}

export interface QuestionUpdateItem {
	id?: string;
	text: string;
	type: 'LikertScale' | 'TrueFalseDontKnow' | 'MultipleChoice' | 'DropdownGrade' | 'ShortText' | 'LongText' | 'EvaluatorOnly';
	options?: string[];
	orderIndex: number;
	step: number;
	isRequired: boolean;
	minValue?: number;
	maxValue?: number;
	minLabel?: string;
	maxLabel?: string;
}

export interface UpdateQuestionsRequest {
	questions: QuestionUpdateItem[];
}

// Material Management types
export type MaterialType = 'Pdf' | 'Video' | 'Text';

export interface AdminMaterial {
	id: string;
	title: string;
	description: string | null;
	type: MaterialType;
	fileExtension: string | null;
	fileSizeBytes: number | null;
	orderIndex: number;
	isActive: boolean;
	accessCount: number;
	createdAt: string;
	updatedAt: string;
}

export interface AdminMaterialDetail extends AdminMaterial {
	signedUrl: string;
}

export interface UpdateMaterialRequest {
	title: string;
	description: string | null;
	type: MaterialType;
	orderIndex: number;
	isActive: boolean;
}

export const adminApi = {
	// Dashboard
	async getDashboard(): Promise<DashboardMetrics> {
		return await api.get<DashboardMetrics>('/admin/dashboard');
	},

	// Users
	async getUsers(role?: string, status?: string): Promise<AdminUser[]> {
		const params = new URLSearchParams();
		if (role) params.append('role', role);
		if (status) params.append('status', status);
		const query = params.toString() ? `?${params.toString()}` : '';
		return await api.get<AdminUser[]>(`/admin/users${query}`);
	},

	async getUser(id: string): Promise<AdminUser> {
		return await api.get<AdminUser>(`/admin/users/${id}`);
	},

	async createUser(request: CreateUserRequest): Promise<AdminUser> {
		return await api.post<AdminUser>('/admin/users', request);
	},

	async updateUser(id: string, request: UpdateUserRequest): Promise<AdminUser> {
		return await api.put<AdminUser>(`/admin/users/${id}`, request);
	},

	async deleteUser(id: string): Promise<void> {
		return await api.delete<void>(`/admin/users/${id}`);
	},

	async activateUser(id: string): Promise<{ success: boolean; message: string }> {
		return await api.post<{ success: boolean; message: string }>(`/admin/users/${id}/activate`, {});
	},

	async deactivateUser(id: string): Promise<{ success: boolean; message: string }> {
		return await api.post<{ success: boolean; message: string }>(`/admin/users/${id}/deactivate`, {});
	},

	async resetPassword(id: string, newPassword: string): Promise<ResetPasswordResponse> {
		return await api.post<ResetPasswordResponse>(`/admin/users/${id}/reset-password`, { newPassword });
	},

	async importUsers(file: File): Promise<CsvImportResult> {
		const formData = new FormData();
		formData.append('file', file);

		const response = await fetch(`${PUBLIC_API_URL}/admin/users/import`, {
			method: 'POST',
			body: formData,
			credentials: 'include'
		});

		if (!response.ok) {
			const error = await response.json().catch(() => ({ error: 'Import failed' }));
			throw new Error(error.message || error.error);
		}

		return await response.json();
	},

	// Evaluator Assignments
	async getEvaluatorAssignments(): Promise<EvaluatorAssignment[]> {
		return await api.get<EvaluatorAssignment[]>('/admin/evaluator-assignments');
	},

	async createEvaluatorAssignment(request: CreateAssignmentRequest): Promise<EvaluatorAssignment> {
		return await api.post<EvaluatorAssignment>('/admin/evaluator-assignments', request);
	},

	async deleteEvaluatorAssignment(id: string): Promise<void> {
		return await api.delete<void>(`/admin/evaluator-assignments/${id}`);
	},

	// Post-test Batch
	async openPosttestBatch(request: CreateBatchRequest): Promise<PostTestBatch> {
		return await api.post<PostTestBatch>('/admin/posttest-batch/open', request);
	},

	async closePosttestBatch(id: string): Promise<{ success: boolean; message: string }> {
		return await api.post<{ success: boolean; message: string }>('/admin/posttest-batch/close', { id });
	},

	// Export
	async exportData(format: 'excel' | 'csv'): Promise<Blob> {
		const response = await fetch(`${PUBLIC_API_URL}/admin/export?format=${format}`, {
			method: 'GET',
			credentials: 'include'
		});

		if (!response.ok) {
			const error = await response.json().catch(() => ({ error: 'Export failed' }));
			throw new Error(error.message || error.error);
		}

		return await response.blob();
	},

	// Questionnaire Builder
	async getQuestionnaires(): Promise<QuestionnaireListItem[]> {
		return await api.get<QuestionnaireListItem[]>('/admin/questionnaires');
	},

	async getQuestionnaire(id: string): Promise<QuestionnaireDetail> {
		return await api.get<QuestionnaireDetail>(`/admin/questionnaires/${id}`);
	},

	async createQuestionnaire(request: CreateQuestionnaireRequest): Promise<QuestionnaireDetail> {
		return await api.post<QuestionnaireDetail>('/admin/questionnaires', request);
	},

	async updateQuestionnaire(id: string, request: UpdateQuestionnaireRequest): Promise<QuestionnaireDetail> {
		return await api.put<QuestionnaireDetail>(`/admin/questionnaires/${id}`, request);
	},

	async updateQuestions(id: string, request: UpdateQuestionsRequest): Promise<QuestionnaireDetail> {
		return await api.put<QuestionnaireDetail>(`/admin/questionnaires/${id}/questions`, request);
	},

	async deleteQuestionnaire(id: string): Promise<{ success: boolean; message: string }> {
		return await api.delete<{ success: boolean; message: string }>(`/admin/questionnaires/${id}`);
	},

	// Materials Management
	async getMaterials(type?: MaterialType, includeInactive: boolean = true): Promise<AdminMaterial[]> {
		const params = new URLSearchParams();
		if (type) params.append('type', type);
		params.append('includeInactive', String(includeInactive));
		const query = params.toString() ? `?${params.toString()}` : '';
		return await api.get<AdminMaterial[]>(`/admin/materials${query}`);
	},

	async uploadMaterial(
		file: File,
		title: string,
		type: MaterialType,
		description?: string | null,
		onProgress?: (percent: number) => void
	): Promise<AdminMaterial> {
		return new Promise((resolve, reject) => {
			const xhr = new XMLHttpRequest();
			const formData = new FormData();

			formData.append('file', file);
			formData.append('title', title);
			formData.append('type', type);
			if (description) formData.append('description', description);

			xhr.upload.onprogress = (e) => {
				if (e.lengthComputable && onProgress) {
					onProgress(Math.round((e.loaded / e.total) * 100));
				}
			};

			xhr.onload = () => {
				if (xhr.status >= 200 && xhr.status < 300) {
					try {
						resolve(JSON.parse(xhr.responseText));
					} catch {
						reject(new Error('Invalid response from server'));
					}
				} else {
					try {
						const error = JSON.parse(xhr.responseText);
						reject(new Error(error.error || error.message || 'Upload failed'));
					} catch {
						reject(new Error('Upload failed'));
					}
				}
			};

			xhr.onerror = () => reject(new Error('Network error during upload'));
			xhr.ontimeout = () => reject(new Error('Upload timed out'));

			xhr.open('POST', `${PUBLIC_API_URL}/admin/materials/upload`);
			xhr.timeout = 600000; // 10 minute timeout for large files
			xhr.withCredentials = true;

			// Get auth token from auth store
			const authState = get(auth);
			if (authState.accessToken) {
				xhr.setRequestHeader('Authorization', `Bearer ${authState.accessToken}`);
			}

			xhr.send(formData);
		});
	},

	async getMaterialDetail(id: string): Promise<AdminMaterialDetail> {
		return await api.get<AdminMaterialDetail>(`/admin/materials/${id}`);
	},

	async updateMaterial(id: string, request: UpdateMaterialRequest): Promise<AdminMaterial> {
		return await api.put<AdminMaterial>(`/admin/materials/${id}`, request);
	},

	async deleteMaterial(id: string): Promise<void> {
		return await api.delete<void>(`/admin/materials/${id}`);
	}
};

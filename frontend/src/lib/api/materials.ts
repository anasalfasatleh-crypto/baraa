import { api } from './client';

export interface Material {
	id: string;
	title: string;
	description: string | null;
	type: 'Pdf' | 'Video' | 'Text';
	fileExtension: string | null;
	fileSizeBytes: number | null;
	accessCount: number;
}

export interface MaterialDetail {
	id: string;
	title: string;
	description: string | null;
	type: 'Pdf' | 'Video' | 'Text';
	fileExtension: string | null;
	fileSizeBytes: number | null;
	signedUrl: string;
	accessCount: number;
}

export interface TrackAccessRequest {
	durationSeconds?: number;
	completed: boolean;
}

export const materialsApi = {
	async getMaterials(): Promise<Material[]> {
		return api.get<Material[]>('/student/materials');
	},

	async getMaterialDetail(id: string): Promise<MaterialDetail> {
		return api.get<MaterialDetail>(`/student/materials/${id}`);
	},

	async trackAccess(id: string, data: TrackAccessRequest): Promise<void> {
		await api.post<void>(`/student/materials/${id}/track`, data);
	}
};

import { api } from './client';

export interface AssignedStudent {
	id: string;
	name: string;
	email: string;
	hospital?: string;
	gender?: string;
}

export interface QuestionResponse {
	questionId: string;
	questionText: string;
	questionType: string;
	step: number;
	orderIndex: number;
	isRequired: boolean;
	answer?: string;
	score?: number;
	isScoreFinalized: boolean;
}

export interface StudentResponses {
	studentId: string;
	questionnaireId: string;
	questionnaireTitle: string;
	questionnaireType: string;
	submittedAt?: string;
	responses: QuestionResponse[];
}

export interface ScoreItem {
	questionId: string;
	score: number;
}

export interface SaveScoresRequest {
	questionnaireId: string;
	scores: ScoreItem[];
	finalize: boolean;
}

export interface SaveScoresResponse {
	success: boolean;
	message: string;
}

export const evaluatorApi = {
	async getAssignedStudents(): Promise<AssignedStudent[]> {
		return await api.get<AssignedStudent[]>('/evaluator/students');
	},

	async getStudentResponses(studentId: string, type: 'pretest' | 'posttest' = 'pretest'): Promise<StudentResponses> {
		return await api.get<StudentResponses>(`/evaluator/students/${studentId}/responses?type=${type}`);
	},

	async saveScores(studentId: string, request: SaveScoresRequest): Promise<SaveScoresResponse> {
		return await api.post<SaveScoresResponse>(`/evaluator/students/${studentId}/scores`, request);
	}
};

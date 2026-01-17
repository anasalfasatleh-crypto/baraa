import { api } from './client';

export interface StudentStatus {
	pretestCompleted: boolean;
	pretestCompletedAt: string | null;
	posttestCompleted: boolean;
	posttestCompletedAt: string | null;
	materialsAccessed: number;
}

export interface Question {
	id: string;
	text: string;
	type: 'LikertScale' | 'TrueFalse' | 'MultipleChoice' | 'Dropdown' | 'TextField';
	options: string[] | null;
	orderIndex: number;
	step: number;
	isRequired: boolean;
	minValue: number | null;
	maxValue: number | null;
	minLabel: string | null;
	maxLabel: string | null;
}

export interface Answer {
	questionId: string;
	value: string;
}

export interface QuestionnaireWithAnswers {
	id: string;
	title: string;
	description: string | null;
	type: 'Pretest' | 'Posttest';
	questions: Question[];
	answers: Answer[];
	totalSteps: number;
	isSubmitted: boolean;
}

export interface SaveAnswersRequest {
	answers: Record<string, string>;
}

export interface SaveAnswersResponse {
	success: boolean;
	message: string;
}

export interface SubmissionResponse {
	success: boolean;
	message: string;
	submittedAt: string;
}

export interface StepTimingRequest {
	step: number;
	isStart: boolean;
}

export const studentApi = {
	async getStatus(): Promise<StudentStatus> {
		return api.get<StudentStatus>('/student/status');
	},

	async getPretest(): Promise<QuestionnaireWithAnswers> {
		return api.get<QuestionnaireWithAnswers>('/student/pretest');
	},

	async savePretestAnswers(answers: Record<string, string>): Promise<SaveAnswersResponse> {
		return api.post<SaveAnswersResponse>('/student/pretest/save', {
			answers
		});
	},

	async submitPretest(): Promise<SubmissionResponse> {
		return api.post<SubmissionResponse>('/student/pretest/submit');
	},

	async recordStepTiming(step: number, isStart: boolean): Promise<void> {
		await api.post<void>('/student/step-timing', {
			step,
			isStart
		});
	},

	async getPosttest(): Promise<QuestionnaireWithAnswers> {
		return api.get<QuestionnaireWithAnswers>('/student/posttest');
	},

	async savePosttestAnswers(answers: Record<string, string>): Promise<SaveAnswersResponse> {
		return api.post<SaveAnswersResponse>('/student/posttest/save', {
			answers
		});
	},

	async submitPosttest(): Promise<SubmissionResponse> {
		return api.post<SubmissionResponse>('/student/posttest/submit');
	}
};

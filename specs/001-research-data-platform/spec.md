# Feature Specification: Research Data Collection Platform

**Feature Branch**: `001-research-data-platform`
**Created**: 2026-01-10
**Status**: Draft
**Input**: Research Data Collection Platform for quasi-experimental academic study measuring the effect of psychoeducational ICU delirium training on ICU/CCU nurses

## Overview

This platform supports an academic research study designed to measure the effectiveness of psychoeducational training on ICU delirium for nurses working in ICU/CCU departments. The platform enables structured data collection through pre/post-test questionnaires, provides access to educational materials, supports manual evaluation scoring, and exports data for statistical analysis.

**Core Objective**: Collect scientific data before and after education for statistical analysis in a controlled, auditable manner.

---

## Clarifications

### Session 2026-01-10

- Q: What triggers score finalization for evaluators? → A: Administrator manually finalizes each student's scores (individual action)
- Q: Should pre-test scores be auto-calculated like post-test? → A: Yes, auto-calculate pre-test scores using same method as post-test
- Q: How should bulk user creation be supported? → A: CSV/Excel file import for bulk user creation
- Q: When can evaluators begin scoring students? → A: Evaluators can score only after post-test submission

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Student Completes Pre-Test Assessment (Priority: P1)

A nurse (student) logs into the platform with credentials provided by the administrator. Upon first login, they are immediately presented with the pre-test questionnaire. The student navigates through multi-step questionnaire sections, answering various question types (Likert scale, true/false, multiple choice). They can move freely between steps, with their progress automatically saved when switching sections. Once all questions are answered, they submit the pre-test, which locks and cannot be edited afterward.

**Why this priority**: The pre-test is the foundational data collection point for the study. Without pre-test completion, the research cannot establish baseline measurements. This is the critical first interaction for every participant.

**Independent Test**: Can be fully tested by creating a student account, logging in, completing a multi-step questionnaire, and verifying submission lock. Delivers immediate research value by capturing baseline data.

**Acceptance Scenarios**:

1. **Given** a student with valid credentials, **When** they log in for the first time, **Then** the pre-test questionnaire is displayed automatically
2. **Given** a student on step 2 of the pre-test, **When** they navigate to step 4, **Then** their step 2 answers are auto-saved
3. **Given** a student who has answered all questions, **When** they submit the pre-test, **Then** the submission is locked and a confirmation with timestamp is displayed
4. **Given** a student who has submitted the pre-test, **When** they attempt to access the pre-test again, **Then** they see their submission confirmation (read-only) and cannot edit answers

---

### User Story 2 - Student Accesses Educational Materials (Priority: P2)

After completing the pre-test, the student gains unlimited access to educational materials about ICU delirium. Materials include PDFs, videos, and text sections. The student can view any material multiple times. The system tracks their engagement (number of opens, time spent, first/last access) without restricting access.

**Why this priority**: Educational intervention is the core of the psychoeducational study. Students must have unrestricted access to learning materials for the training to be effective.

**Independent Test**: Can be tested by uploading materials, having a student access them multiple times, and verifying access tracking. Delivers value by enabling the educational intervention.

**Acceptance Scenarios**:

1. **Given** a student who completed the pre-test, **When** they navigate to educational materials, **Then** they see all available PDFs, videos, and text content
2. **Given** a student viewing a video, **When** they close the video after 5 minutes, **Then** the system records the access duration
3. **Given** a student who has accessed a PDF three times, **When** an admin views their record, **Then** the access count shows 3 with first and last access timestamps

---

### User Story 3 - Student Completes Post-Test Assessment (Priority: P3)

After a defined period (typically 3 months), the administrator opens the post-test batch for all students. Students who completed the pre-test can now access and complete the post-test questionnaire. The post-test follows the same multi-step, auto-save pattern as the pre-test but is only available during the open batch period.

**Why this priority**: Post-test data enables comparison with pre-test scores, which is essential for measuring training effectiveness. This completes the student's research participation.

**Independent Test**: Can be tested by opening a post-test batch, having a student complete it, and verifying submission and scoring. Delivers value by providing comparative research data.

**Acceptance Scenarios**:

1. **Given** a student who completed pre-test and the post-test batch is closed, **When** they try to access post-test, **Then** they see a message that post-test is not yet available
2. **Given** a student who completed pre-test and the post-test batch is open, **When** they access the post-test, **Then** they can complete and submit the questionnaire
3. **Given** a student who submitted the post-test, **When** they try to access it again, **Then** they see submission confirmation (read-only)

---

### User Story 4 - Evaluator Scores Student Responses (Priority: P4)

An evaluator logs in and sees only their assigned students. For each student, the evaluator can enter manual scores on designated scoring fields. The system auto-calculates total evaluator scores. Evaluators can edit scores until finalization. Evaluators cannot see personal demographic data beyond what is necessary for identification.

**Why this priority**: Evaluator scoring provides expert assessment data that complements student self-reported answers, adding validity to the research findings.

**Independent Test**: Can be tested by assigning students to an evaluator, having the evaluator enter scores, and verifying score calculations. Delivers value by capturing expert assessments.

**Acceptance Scenarios**:

1. **Given** an evaluator with 5 assigned students, **When** they log in, **Then** they see only those 5 students in their list
2. **Given** an evaluator viewing a student's responses, **When** they enter scores for each item, **Then** the system auto-calculates the total evaluator score
3. **Given** an evaluator who entered scores, **When** the scores are not finalized, **Then** the evaluator can edit their scores
4. **Given** an evaluator, **When** they view student information, **Then** they see only minimal identification data (no sensitive demographics)

---

### User Story 5 - Administrator Manages Users and Study Flow (Priority: P5)

An administrator creates student and evaluator accounts, assigns evaluators to students, and controls the study flow. The admin can activate/deactivate users, reset passwords, and open/close the post-test batch for all students. The admin has access to a dashboard showing study progress metrics.

**Why this priority**: Administrative control is essential for running the study, but the platform can function for data collection with manual workarounds if admin features are limited.

**Independent Test**: Can be tested by creating users, managing their status, and controlling batch access. Delivers value by enabling study management.

**Acceptance Scenarios**:

1. **Given** an admin, **When** they create a new student account, **Then** the student receives credentials and can log in
2. **Given** an admin viewing inactive users, **When** they activate a user, **Then** that user can log in to the platform
3. **Given** an admin, **When** they click "Open Post-Test Batch", **Then** all eligible students can access the post-test
4. **Given** an admin, **When** they view the dashboard, **Then** they see counts for: students registered, pre-test completed, material accesses, post-test completed, evaluations completed

---

### User Story 6 - Administrator Exports Research Data (Priority: P6)

The administrator can export all collected data (student responses, timing data, evaluator scores, combined scores) in formats suitable for statistical analysis software (Excel/CSV). The export includes all relevant fields needed for SPSS analysis.

**Why this priority**: Data export is the ultimate deliverable of the platform—enabling the research analysis. However, it is dependent on data being collected first.

**Independent Test**: Can be tested by generating sample data and exporting to Excel/CSV, then verifying the file can be imported into statistical software.

**Acceptance Scenarios**:

1. **Given** an admin with completed study data, **When** they click Export to Excel, **Then** a downloadable file is generated containing all student responses, scores, and metadata
2. **Given** an exported file, **When** imported into SPSS, **Then** all fields are properly structured for analysis

---

### User Story 7 - Administrator Configures Questionnaires (Priority: P7)

The administrator can create and edit questionnaires through a form builder. Supported question types include: Likert scale (1-5), True/False/I Don't Know, Multiple choice (single answer), Dropdown grading, Text fields, and Evaluator-only manual scoring fields. Questions are organized into steps for multi-step navigation.

**Why this priority**: Questionnaire configuration is needed before data collection, but an initial questionnaire can be pre-configured for launch.

**Independent Test**: Can be tested by building a questionnaire with various question types and verifying student can complete it.

**Acceptance Scenarios**:

1. **Given** an admin in the form builder, **When** they add a Likert scale question, **Then** it appears with 1-5 options for students
2. **Given** an admin building a questionnaire, **When** they organize questions into steps, **Then** students see those steps in the multi-step navigation
3. **Given** a question marked as evaluator-only, **When** a student views the questionnaire, **Then** that question is not visible to them

---

### Edge Cases

- What happens when a student's session expires mid-questionnaire? → Auto-saved answers are preserved; student continues from last saved state upon re-login
- How does the system handle a student trying to submit with unanswered required questions? → Validation prevents submission; student is directed to incomplete questions
- What if an evaluator is deactivated with unfinalized scores? → Scores remain in system; admin can reassign students to another evaluator
- What happens if a student never completes the pre-test? → They cannot access educational materials or post-test; admin can view incomplete status
- How are concurrent edits handled if evaluator has multiple browser tabs? → Last save wins; user is warned if editing stale data

---

## Requirements *(mandatory)*

### Functional Requirements

**Authentication & Access Control**

- **FR-001**: System MUST support three distinct user roles: Administrator, Evaluator, and Student
- **FR-002**: System MUST authenticate users with secure credentials (username/password)
- **FR-003**: System MUST enforce role-based access control restricting features by user role
- **FR-004**: Users MUST be able to change their password after initial login
- **FR-005**: Administrators MUST be able to reset passwords for any user
- **FR-006**: Administrators MUST be able to activate and deactivate user accounts
- **FR-007**: Deactivated users MUST be prevented from logging in

**Questionnaire System**

- **FR-008**: System MUST support multi-step questionnaires with free navigation between steps
- **FR-009**: System MUST auto-save student answers when navigating between questionnaire steps
- **FR-010**: System MUST support these question types: Likert scale (1-5), True/False/I Don't Know, Multiple choice (single answer), Dropdown grading, Text field (optional), Evaluator-only scoring field
- **FR-011**: System MUST validate required questions before allowing questionnaire submission
- **FR-012**: System MUST lock submitted questionnaires preventing any edits
- **FR-013**: System MUST display submission confirmation with timestamp after successful submission
- **FR-014**: System MUST track time spent on each questionnaire step

**Pre-Test Specific**

- **FR-015**: Pre-test MUST be automatically presented to students on first login
- **FR-016**: Students MUST complete pre-test exactly once
- **FR-017**: Pre-test completion MUST be required before accessing educational materials

**Post-Test Specific**

- **FR-018**: Post-test MUST only be accessible when administrator opens the batch
- **FR-019**: Administrators MUST be able to open and close post-test access for all students simultaneously
- **FR-020**: Students MUST complete post-test exactly once (when available)

**Educational Materials**

- **FR-021**: Administrators MUST be able to upload educational materials (PDFs, videos, text)
- **FR-022**: Students MUST have unlimited access to educational materials after pre-test completion
- **FR-023**: System MUST track material engagement: number of opens, time spent, first/last access timestamps

**Evaluator Scoring**

- **FR-024**: Evaluators MUST only see students assigned to them
- **FR-024a**: Evaluators MUST only be able to score students who have submitted their post-test
- **FR-025**: Evaluators MUST be able to enter manual scores for designated scoring fields
- **FR-026**: System MUST auto-calculate total evaluator score from individual item scores
- **FR-027**: Evaluators MUST be able to edit scores until administrator finalizes that student's scores
- **FR-027a**: Administrators MUST be able to finalize individual student scores, after which evaluator edits are locked
- **FR-028**: Evaluators MUST NOT see student demographic data beyond minimal identification

**Scoring & Calculations**

- **FR-029**: System MUST calculate pre-test and post-test scores from student answers using the same scoring method
- **FR-030**: System MUST calculate combined scores from post-test and evaluator scores
- **FR-031**: System MUST store all score components separately for analysis

**Administrative Functions**

- **FR-032**: Administrators MUST be able to create user accounts (students, evaluators)
- **FR-032a**: Administrators MUST be able to bulk-import users via CSV/Excel file upload
- **FR-033**: Administrators MUST be able to assign evaluators to students
- **FR-034**: Administrators MUST be able to view all student responses and progress
- **FR-035**: Administrators MUST be able to view all evaluator scores
- **FR-036**: Administrators MUST be able to create and edit questionnaires via form builder

**Data Export**

- **FR-037**: Administrators MUST be able to export all data to Excel format
- **FR-038**: Administrators MUST be able to export all data to CSV format
- **FR-039**: Exports MUST include: student responses, timing data, evaluator scores, combined scores, and relevant metadata

**Dashboard & Analytics**

- **FR-040**: Administrator dashboard MUST display: students registered, pre-tests completed, material access counts, post-tests completed, evaluator scoring completion
- **FR-041**: Dashboard MUST include comparative charts: pre vs post score averages, completion percentages
- **FR-042**: Dashboard MUST support filtering by hospital, gender, and experience level

**User Interface**

- **FR-043**: Platform MUST provide mobile-first responsive design
- **FR-044**: Platform MUST function properly on mobile devices and tablets

**Security & Audit**

- **FR-045**: System MUST maintain audit logs for login attempts, scoring actions, and administrative activities
- **FR-046**: System MUST use secure session management
- **FR-047**: All file uploads MUST be validated for type and size

---

### Key Entities

- **User**: Represents administrators, evaluators, and students. Contains identity, contact info, hospital/department affiliation, role, and account status.

- **Questionnaire**: A collection of questions organized into steps. Has type (pre-test/post-test) and active status.

- **Question**: Individual assessment item within a questionnaire. Has question type, text, options, step assignment, and optionally evaluator-only flag.

- **Answer**: A student's response to a question. Linked to user and question, includes answer value and submission timestamp.

- **Step Timing**: Tracks time spent by each student on each questionnaire step.

- **Material**: Educational content (PDF, video, text) uploaded by administrators.

- **Material Access**: Records each time a student opens a material, tracking duration and timestamps.

- **Evaluator Score**: Manual score entered by evaluator for specific questions/students. Linked to evaluator, student, and question.

- **Combined Score**: Aggregated scores per student including post-test score, evaluator score, and combined total.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

**Data Collection Quality**

- **SC-001**: 95% of registered students successfully complete the pre-test questionnaire
- **SC-002**: 90% of students who complete pre-test also complete post-test (when available)
- **SC-003**: 100% of student responses are accurately captured and exportable

**User Experience**

- **SC-004**: Students can complete the pre-test questionnaire in under 30 minutes
- **SC-005**: Students can navigate between questionnaire steps with no data loss
- **SC-006**: Evaluators can score a student's responses in under 10 minutes
- **SC-007**: Platform is fully usable on mobile devices without horizontal scrolling

**Administrative Efficiency**

- **SC-008**: Administrators can create 50 student accounts in under 15 minutes
- **SC-009**: Data export generates complete dataset in under 60 seconds
- **SC-010**: Exported data imports successfully into SPSS without manual reformatting

**System Reliability**

- **SC-011**: System maintains 99% uptime during the study period
- **SC-012**: No data loss occurs due to session timeout or network interruption (auto-save works)
- **SC-013**: System supports at least 100 concurrent users without performance degradation

**Research Integrity**

- **SC-014**: All questionnaire submissions are timestamped with server time
- **SC-015**: Submitted questionnaires cannot be modified by anyone after submission
- **SC-016**: Complete audit trail exists for all evaluator scoring activities

---

## Assumptions

1. **Study Timeline**: The study runs for approximately 3 months between pre-test and post-test
2. **User Base**: Expected maximum of 200 students, 10 evaluators, and 5 administrators
3. **Language**: Platform will be in English (single language)
4. **Browser Support**: Modern browsers (Chrome, Firefox, Safari, Edge - latest 2 versions)
5. **Internet Access**: All users have reliable internet connectivity
6. **File Sizes**: Video files will not exceed 500MB; PDFs will not exceed 50MB
7. **Questionnaire Complexity**: Pre-test and post-test will have similar structure (same sections/steps)
8. **Scoring Formula**: Combined score calculation formula will be defined by research team before development
9. **Evaluator Assignment**: Each student is assigned to exactly one evaluator; evaluators may have multiple students
10. **Password Policy**: Standard password requirements (minimum 8 characters, complexity rules)

---

## Out of Scope

- Email notification system
- In-app messaging between users
- Multiple evaluator layers/review chains
- Automatic statistical analysis or report generation
- Mobile native applications
- Multi-language support
- Anonymous/self-registration (all accounts created by admin)
- Integration with external learning management systems
- Real-time collaboration features

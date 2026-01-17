# Data Model: Research Data Collection Platform

**Branch**: `001-research-data-platform` | **Date**: 2026-01-10

## Entity Relationship Overview

```
┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│    User     │────<│ EvaluatorStudent │>────│    User     │
│  (Student)  │     │   (Assignment)   │     │ (Evaluator) │
└──────┬──────┘     └──────────────────┘     └──────┬──────┘
       │                                            │
       │ 1:N                                        │ 1:N
       ▼                                            ▼
┌─────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   Answer    │────>│    Question      │<────│ EvaluatorScore  │
└──────┬──────┘     └────────┬─────────┘     └─────────────────┘
       │                     │
       │                     │ N:1
       │                     ▼
       │            ┌──────────────────┐
       │            │  Questionnaire   │
       │            └──────────────────┘
       │
       │ 1:1
       ▼
┌─────────────┐
│ StepTiming  │
└─────────────┘

┌─────────────┐     ┌──────────────────┐
│  Material   │<────│  MaterialAccess  │────>│ User (Student)
└─────────────┘     └──────────────────┘
```

---

## Entities

### User

Represents all system users: administrators, evaluators, and students.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| email | string(255) | UNIQUE, NOT NULL | Login identifier |
| password_hash | string(255) | NOT NULL | bcrypt hashed password |
| name | string(100) | NOT NULL | Display name |
| university_id | string(50) | NULL | Student/staff ID number |
| phone | string(20) | NULL | Contact phone |
| hospital | string(100) | NULL | Hospital affiliation |
| department | string(100) | NULL | Department (ICU/CCU) |
| gender | enum | NULL | male, female, other |
| experience_years | int | NULL | Years of nursing experience |
| role | enum | NOT NULL | admin, evaluator, student |
| status | enum | NOT NULL, DEFAULT active | active, inactive |
| created_at | timestamp | NOT NULL | Account creation time |
| updated_at | timestamp | NOT NULL | Last modification time |
| last_login_at | timestamp | NULL | Most recent login |

**Indexes**:
- `idx_user_email` on email (unique)
- `idx_user_role_status` on (role, status)
- `idx_user_hospital` on hospital

**Validation Rules**:
- Email must be valid format
- Password minimum 8 characters before hashing
- Role cannot be changed after creation (admin operation only)

---

### Questionnaire

Container for pre-test or post-test question sets.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| title | string(200) | NOT NULL | Questionnaire name |
| type | enum | NOT NULL | pretest, posttest |
| is_active | boolean | NOT NULL, DEFAULT true | Whether questionnaire is in use |
| total_steps | int | NOT NULL | Number of navigation steps |
| created_at | timestamp | NOT NULL | Creation time |
| updated_at | timestamp | NOT NULL | Last modification |

**Business Rules**:
- Only one active questionnaire per type at a time
- Cannot modify questionnaire after any submissions exist

---

### Question

Individual question within a questionnaire.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| questionnaire_id | UUID | FK, NOT NULL | Parent questionnaire |
| step_number | int | NOT NULL | Which step (1-indexed) |
| order_in_step | int | NOT NULL | Display order within step |
| question_type | enum | NOT NULL | likert, truefalse, multiplechoice, dropdown, text, evaluator_scoring |
| text | text | NOT NULL | Question text |
| options_json | jsonb | NULL | Options for choice questions |
| is_required | boolean | NOT NULL, DEFAULT true | Must be answered |
| is_evaluator_only | boolean | NOT NULL, DEFAULT false | Hidden from students |
| score_weight | decimal(5,2) | DEFAULT 1.0 | Weight for scoring |
| created_at | timestamp | NOT NULL | Creation time |

**options_json Format**:
```json
{
  "choices": [
    {"value": "1", "label": "Strongly Disagree"},
    {"value": "2", "label": "Disagree"},
    {"value": "3", "label": "Neutral"},
    {"value": "4", "label": "Agree"},
    {"value": "5", "label": "Strongly Agree"}
  ],
  "correct_value": "5"  // optional, for auto-scoring
}
```

**Indexes**:
- `idx_question_questionnaire_step` on (questionnaire_id, step_number, order_in_step)

**Validation Rules**:
- step_number must be between 1 and questionnaire.total_steps
- options_json required for likert, truefalse, multiplechoice, dropdown types
- evaluator_scoring type implies is_evaluator_only = true

---

### Answer

Student response to a question.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| user_id | UUID | FK, NOT NULL | Student who answered |
| question_id | UUID | FK, NOT NULL | Question being answered |
| answer_value | text | NULL | The answer (nullable for optional questions) |
| saved_at | timestamp | NOT NULL | Last auto-save time |
| submitted_at | timestamp | NULL | Final submission time (locks record) |

**Indexes**:
- `idx_answer_user_question` on (user_id, question_id) UNIQUE
- `idx_answer_submitted` on submitted_at WHERE submitted_at IS NOT NULL

**State Transitions**:
```
[Not Exists] --save--> [Draft: submitted_at=NULL]
[Draft] --save--> [Draft] (update answer_value)
[Draft] --submit--> [Submitted: submitted_at=NOW()]
[Submitted] --X--> (immutable, no transitions)
```

**Validation Rules**:
- answer_value format must match question_type (e.g., "1"-"5" for likert)
- Cannot update if submitted_at is not null (enforced by trigger)

---

### StepTiming

Tracks time spent on each questionnaire step.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| user_id | UUID | FK, NOT NULL | Student |
| questionnaire_id | UUID | FK, NOT NULL | Which questionnaire |
| step_number | int | NOT NULL | Step being tracked |
| started_at | timestamp | NOT NULL | When step was opened |
| ended_at | timestamp | NULL | When step was left |
| duration_seconds | int | GENERATED | Computed from start/end |

**Indexes**:
- `idx_steptiming_user_questionnaire` on (user_id, questionnaire_id, step_number)

---

### Material

Educational content uploaded by administrators.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| title | string(200) | NOT NULL | Material title |
| description | text | NULL | Description/summary |
| type | enum | NOT NULL | pdf, video, text |
| file_path | string(500) | NULL | S3 key for pdf/video |
| text_content | text | NULL | Content for text type |
| file_size_bytes | bigint | NULL | File size for pdf/video |
| order_index | int | NOT NULL, DEFAULT 0 | Display order |
| is_active | boolean | NOT NULL, DEFAULT true | Whether visible to students |
| created_at | timestamp | NOT NULL | Upload time |
| updated_at | timestamp | NOT NULL | Last modification |

**Validation Rules**:
- file_path required if type is pdf or video
- text_content required if type is text
- file_size_bytes must be <= 500MB for video, <= 50MB for pdf

---

### MaterialAccess

Records student engagement with educational materials.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| user_id | UUID | FK, NOT NULL | Student |
| material_id | UUID | FK, NOT NULL | Material accessed |
| opened_at | timestamp | NOT NULL | When access started |
| closed_at | timestamp | NULL | When access ended |
| duration_seconds | int | NULL | Time spent (computed) |

**Indexes**:
- `idx_materialaccess_user_material` on (user_id, material_id)
- `idx_materialaccess_opened` on opened_at

---

### EvaluatorAssignment

Links evaluators to their assigned students.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| evaluator_id | UUID | FK, NOT NULL | Evaluator user |
| student_id | UUID | FK, NOT NULL | Student user |
| assigned_at | timestamp | NOT NULL | When assigned |

**Indexes**:
- `idx_evaluator_assignment_unique` on (evaluator_id, student_id) UNIQUE
- `idx_evaluator_assignment_student` on student_id

**Validation Rules**:
- evaluator_id must reference user with role = evaluator
- student_id must reference user with role = student
- One student can have only one evaluator (enforced by unique constraint on student_id)

---

### EvaluatorScore

Manual scores entered by evaluators for specific questions.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| evaluator_id | UUID | FK, NOT NULL | Evaluator who scored |
| student_id | UUID | FK, NOT NULL | Student being scored |
| question_id | UUID | FK, NOT NULL | Question being scored |
| score | decimal(5,2) | NOT NULL | Assigned score |
| notes | text | NULL | Evaluator comments |
| saved_at | timestamp | NOT NULL | Last save time |
| finalized_at | timestamp | NULL | When admin finalized |

**Indexes**:
- `idx_evaluatorscore_unique` on (evaluator_id, student_id, question_id) UNIQUE
- `idx_evaluatorscore_student` on student_id

**State Transitions**:
```
[Not Exists] --save--> [Draft: finalized_at=NULL]
[Draft] --save--> [Draft] (update score)
[Draft] --admin_finalize--> [Finalized: finalized_at=NOW()]
[Finalized] --X--> (immutable)
```

**Validation Rules**:
- question_id must reference question with is_evaluator_only = true
- Cannot update if finalized_at is not null

---

### Score

Calculated scores for a student's questionnaire submission.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| user_id | UUID | FK, NOT NULL | Student |
| questionnaire_id | UUID | FK, NOT NULL | Which questionnaire |
| raw_score | decimal(10,2) | NOT NULL | Sum of weighted answers |
| max_possible_score | decimal(10,2) | NOT NULL | Maximum achievable |
| percentage_score | decimal(5,2) | GENERATED | (raw/max)*100 |
| calculated_at | timestamp | NOT NULL | When calculated |

**Indexes**:
- `idx_score_user_questionnaire` on (user_id, questionnaire_id) UNIQUE

---

### CombinedScore

Final aggregated score including evaluator assessment.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| student_id | UUID | FK, NOT NULL | Student |
| pretest_score | decimal(5,2) | NULL | Pre-test percentage |
| posttest_score | decimal(5,2) | NULL | Post-test percentage |
| evaluator_score | decimal(5,2) | NULL | Evaluator total percentage |
| combined_total | decimal(5,2) | NULL | Final combined score |
| calculated_at | timestamp | NOT NULL | When last calculated |
| finalized_at | timestamp | NULL | When admin finalized |

**Indexes**:
- `idx_combinedscore_student` on student_id UNIQUE

---

### PostTestBatch

Controls post-test availability window.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| opened_at | timestamp | NULL | When batch was opened |
| closed_at | timestamp | NULL | When batch was closed |
| opened_by | UUID | FK, NULL | Admin who opened |
| closed_by | UUID | FK, NULL | Admin who closed |

**Business Rules**:
- Only one batch record exists (singleton pattern)
- Post-test accessible when opened_at IS NOT NULL AND closed_at IS NULL

---

### AuditLog

Immutable record of system actions.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| user_id | UUID | FK, NULL | User who performed action |
| action | string(50) | NOT NULL | Action type |
| entity_type | string(50) | NOT NULL | Affected entity type |
| entity_id | UUID | NULL | Affected entity ID |
| old_values | jsonb | NULL | Previous state |
| new_values | jsonb | NULL | New state |
| ip_address | string(45) | NULL | Client IP |
| user_agent | string(500) | NULL | Client user agent |
| created_at | timestamp | NOT NULL | When action occurred |

**Action Types**:
- `login_success`, `login_failed`, `logout`
- `user_created`, `user_updated`, `user_deactivated`
- `answer_saved`, `questionnaire_submitted`
- `score_entered`, `score_finalized`
- `posttest_opened`, `posttest_closed`
- `data_exported`

**Indexes**:
- `idx_auditlog_user` on user_id
- `idx_auditlog_action` on action
- `idx_auditlog_created` on created_at

---

## Database Triggers

### prevent_answer_update_after_submit

```sql
CREATE OR REPLACE FUNCTION prevent_answer_update()
RETURNS TRIGGER AS $$
BEGIN
  IF OLD.submitted_at IS NOT NULL THEN
    RAISE EXCEPTION 'Cannot modify submitted answer';
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_prevent_answer_update
BEFORE UPDATE ON answers
FOR EACH ROW
EXECUTE FUNCTION prevent_answer_update();
```

### prevent_evaluatorscore_update_after_finalize

```sql
CREATE OR REPLACE FUNCTION prevent_score_update()
RETURNS TRIGGER AS $$
BEGIN
  IF OLD.finalized_at IS NOT NULL THEN
    RAISE EXCEPTION 'Cannot modify finalized score';
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_prevent_score_update
BEFORE UPDATE ON evaluator_scores
FOR EACH ROW
EXECUTE FUNCTION prevent_score_update();
```

---

## Views

### v_student_progress

Aggregated view of student study progress.

```sql
CREATE VIEW v_student_progress AS
SELECT
  u.id as student_id,
  u.name,
  u.hospital,
  (SELECT COUNT(*) > 0 FROM answers a
   JOIN questions q ON a.question_id = q.id
   JOIN questionnaires qn ON q.questionnaire_id = qn.id
   WHERE a.user_id = u.id AND qn.type = 'pretest' AND a.submitted_at IS NOT NULL) as pretest_completed,
  (SELECT COUNT(*) FROM material_access ma WHERE ma.user_id = u.id) as material_access_count,
  (SELECT COUNT(*) > 0 FROM answers a
   JOIN questions q ON a.question_id = q.id
   JOIN questionnaires qn ON q.questionnaire_id = qn.id
   WHERE a.user_id = u.id AND qn.type = 'posttest' AND a.submitted_at IS NOT NULL) as posttest_completed,
  (SELECT COUNT(*) > 0 FROM evaluator_scores es
   WHERE es.student_id = u.id AND es.finalized_at IS NOT NULL) as evaluation_completed
FROM users u
WHERE u.role = 'student';
```

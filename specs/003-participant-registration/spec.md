# Feature Specification: Participant Registration System

**Feature Branch**: `003-participant-registration`
**Created**: 2026-01-17
**Status**: Draft
**Input**: User description: "Rename student to participants, add registration with separate login page, auto-generated codes (A1-A99, B1-B99), and admin password reset functionality"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Participant Self-Registration (Priority: P1)

A new participant visits the dedicated participant registration page (separate from the main user pages) and creates an account. The system automatically assigns them a unique participant code following the pattern A1-A99, then B1-B99, C1-C99, etc. The participant receives their code upon successful registration and can use it along with their username or email to log in.

**Why this priority**: This is the core functionality that enables participants to join the system. Without registration, no other participant features can be used.

**Independent Test**: Can be fully tested by navigating to the participant registration page, filling out the registration form, and verifying a unique code is assigned and the participant can log in.

**Acceptance Scenarios**:

1. **Given** a visitor is on the participant registration page, **When** they fill in valid registration details (username/email, password), **Then** the system creates their account and displays their auto-generated participant code
2. **Given** the system has 99 participants with codes A1-A99, **When** a new participant registers, **Then** they receive code B1
3. **Given** a participant has registered, **When** they navigate to the participant login page, **Then** they can log in using their username or email and password

---

### User Story 2 - Participant Login (Priority: P1)

A registered participant accesses the dedicated participant login page (separate from admin/user login) and authenticates using their username or email along with their password. Upon successful login, they are directed to the participant area.

**Why this priority**: Login is essential for participants to access the system after registration. Tied with registration as core functionality.

**Independent Test**: Can be fully tested by having a registered participant navigate to the participant login page and successfully authenticating with their credentials.

**Acceptance Scenarios**:

1. **Given** a registered participant is on the participant login page, **When** they enter their username and correct password, **Then** they are authenticated and redirected to the participant dashboard
2. **Given** a registered participant is on the participant login page, **When** they enter their email and correct password, **Then** they are authenticated and redirected to the participant dashboard
3. **Given** a visitor enters invalid credentials, **When** they attempt to log in, **Then** the system displays an appropriate error message without revealing which field was incorrect

---

### User Story 3 - Admin Password Reset for Participants (Priority: P2)

An administrator needs to help a participant who has forgotten their password. The admin navigates to the participant management section, locates the participant by their code, username, or email, and initiates a password reset. The system generates a temporary password that the admin can communicate to the participant.

**Why this priority**: Password reset is essential for participant support but depends on participant accounts existing first. Critical for ongoing operations.

**Independent Test**: Can be fully tested by an admin logging in, searching for a participant, initiating password reset, and verifying the participant can use the new credentials.

**Acceptance Scenarios**:

1. **Given** an admin is in the participant management section, **When** they search for a participant by code (e.g., "A1"), **Then** the system displays the participant's details
2. **Given** an admin has found a participant, **When** they click "Reset Password", **Then** the system generates a new temporary password and displays it to the admin
3. **Given** a participant's password has been reset by admin, **When** the participant logs in with the temporary password, **Then** they are prompted to create a new password

---

### User Story 4 - View Participant Code (Priority: P3)

A logged-in participant wants to view their assigned participant code. They can access their profile or dashboard to see their unique code displayed prominently.

**Why this priority**: Convenience feature for participants who need to reference their code. Not blocking any core functionality.

**Independent Test**: Can be fully tested by a participant logging in and viewing their profile/dashboard where their code is displayed.

**Acceptance Scenarios**:

1. **Given** a participant is logged in, **When** they view their profile or dashboard, **Then** their participant code is clearly displayed
2. **Given** a participant with code "B42" is logged in, **When** they view their code, **Then** it shows exactly "B42"

---

### Edge Cases

- What happens when all codes in a letter series are exhausted (e.g., A99 reached)?
  - System automatically moves to the next letter (B1, C1, etc.)
- What happens when a participant tries to register with an already-used email?
  - System displays an error indicating the email is already registered
- What happens when an admin tries to reset password for a non-existent participant?
  - System displays a "participant not found" message
- How does the system handle concurrent registrations?
  - System ensures unique code assignment through proper sequencing to prevent duplicates
- What happens when all codes through Z99 are exhausted?
  - System continues with AA1-AA99, AB1-AB99, etc.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a dedicated participant registration page separate from other user registration flows
- **FR-002**: System MUST provide a dedicated participant login page separate from admin/user login pages
- **FR-003**: System MUST automatically generate unique participant codes following the pattern: A1 through A99, then B1 through B99, continuing through the alphabet
- **FR-004**: System MUST use a single "username or email" field for login identification (participant enters either format in one field)
- **FR-005**: System MUST store participant credentials securely with password hashing
- **FR-006**: System MUST display the assigned participant code to the participant upon successful registration
- **FR-007**: Admin MUST be able to search for participants by code, username, or email
- **FR-008**: Admin MUST be able to initiate a password reset for any participant
- **FR-009**: System MUST generate a temporary password when admin resets a participant's password
- **FR-010**: System MUST prompt participants to change their password after using a temporary password
- **FR-011**: System MUST enforce minimum password requirements (at least 8 characters)
- **FR-012**: System MUST prevent duplicate login identifiers (username or email) across participant accounts
- **FR-013**: System MUST allow optional phone number field during registration
- **FR-014**: System MUST track the last assigned code to ensure proper sequential assignment
- **FR-015**: Participant pages and routes MUST be visually and functionally distinct from admin/user pages
- **FR-016**: System MUST temporarily lock participant account after 5 consecutive failed login attempts for 1 minute
- **FR-017**: System MUST maintain participant session for 24 hours or until browser closes, whichever comes first

### Key Entities

- **Participant**: Represents a registered participant in the system. Key attributes: participant code (unique, auto-generated), login identifier (unique, can be username or email format), phone number (optional), password (hashed), registration date, password reset flag (indicates if using temporary password), failed login attempts count, lockout expiry timestamp
- **Participant Code Sequence**: Tracks the current position in the code generation sequence to ensure unique, sequential assignment. Key attributes: current letter, current number
- **Password Reset Record**: Tracks admin-initiated password resets for audit purposes. Key attributes: participant reference, admin who initiated, timestamp, whether temporary password was used

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Participants can complete registration in under 2 minutes
- **SC-002**: Participants can successfully log in using their login identifier (username or email format)
- **SC-003**: 100% of registered participants receive a unique, sequentially assigned code
- **SC-004**: Admins can locate and reset a participant's password in under 1 minute
- **SC-005**: System supports at least 2,574 unique participant codes (A1-Z99) before requiring extended format
- **SC-006**: Participant login page is clearly distinguishable from admin/user login pages
- **SC-007**: Zero duplicate participant codes exist in the system at any time

## Clarifications

### Session 2026-01-17

- Q: What additional information should be collected during participant registration? → A: Minimal registration with single username-or-email field (used for login), password, and optional phone number
- Q: How should the system handle repeated failed login attempts? → A: Temporary lockout after 5 failed attempts with 1 minute cooldown
- Q: How long should a participant remain logged in before requiring re-authentication? → A: Standard session: 24 hours, or until browser closes

## Assumptions

- The system already has an admin user type with appropriate permissions
- There is an existing authentication infrastructure that can be extended for participants
- Participants are a distinct user type from existing "students" (terminology change from student to participant)
- Email verification is not required for participant registration (can be added if needed)
- Temporary passwords expire after first use (participant must set new password)
- The code format is fixed (letter + 1-2 digit number) and cannot be customized

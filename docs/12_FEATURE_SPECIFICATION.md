# 12. Feature Specification: Version 1.0 (Detailed Blueprint)

## 1. Organization Onboarding & Settings
- **Self-Service Registration**: Institute owner registers with Name, Mobile, Email, Institute Name, Address.
- **Verification**: OTP/Email verification triggers initial `OrgAdmin` credential setup.
- **Branding**: Institute Logo, Contact Numbers, Tagline, Address.
- **Academic Sessions**: Configurable academic years (e.g. `2026-27`), one set as active default.
- **Status & Control**: SuperAdmin can activate, suspend, or deactivate organizations.

---

## 2. Teacher & Staff Management
- **Account Creation**: OrgAdmin invites/registers teachers.
- **Profile Fields**: Full name, mobile, email, designation, qualification, subjects taught.
- **Batch & Subject Assignments**: Explicit mapping (`TeacherBatchSubject`) determining which batches and subjects a teacher is authorized to manage.
- **Dashboard**: Teachers see pending homework reviews, upcoming quizzes, scheduled events, and unread batch messages.
- **Status Control**: Deactivation without deleting historical submissions, quizzes, or grades.

---

## 3. Student Management
- **Direct Admission**: OrgAdmin / authorized staff admits students directly.
- **Profile Fields**: Full name, mobile (for login/OTP/SMS), parent contact, admission number, roll number, assigned batch(es).
- **Status**: `Active`, `Inactive`, `Alumni/Completed`.
- **Search & Filters**: Filter students by batch, academic session, payment status, and status.

---

## 4. Batch & Class Management
- **Atomic Unit**: Batches are the core grouping mechanism for teaching, tests, and communication.
- **Configuration**: Batch Name (e.g. `Class 10 - Science Morning`), Grade/Class, Academic Session, Room/Timing info.
- **Student Enrollment**: Add/remove students to/from batches.
- **Teacher Assignment**: Assign primary teachers and subject teachers per batch.

---

## 5. Fees Management
- **Fee Structures**: Configure monthly, quarterly, or yearly fee plans per batch/class.
- **Student-Specific Customization**: Support custom discounts or special fee rates per student.
- **Payment Logging**: Record payments with amount, payment date, mode (Cash, UPI, NetBanking, Cheque), and reference/transaction number.
- **Receipt Generation**: Generate downloadable PDF fee receipt with institute branding and receipt number.
- **Dues Tracking**: Instant summary of Total Expected, Total Collected, Pending, and Overdue fees.

---

## 6. Study Notes & Material
- **Upload Supported Files**: PDF documents and images (PNG, JPG).
- **Metadata**: Title, description, subject, batch association, tags.
- **Access Control**: Scoped strictly to students enrolled in the assigned batch.
- **Version Management**: Update/replace notes with audit history.

---

## 7. Homework & Task Management
- **Task Creation**: Title, instructions, target batch, subject, deadline/due date, attachment files.
- **Student Submission**: Students submit text responses and/or uploaded images/PDFs.
- **Teacher Review**: Teacher reviews submissions, marks as `Reviewed`/`Needs Revision`, assigns scores/feedback.
- **Status Tracking**: Visual breakdown of Submitted, Pending, and Late submissions.

---

## 8. Quiz & Test Management
- **Question Types**: Text questions and Image-based questions (MCQ / Objective format).
- **Scoring Rules**: Positive marks per question, optional negative marking.
- **Scheduling**: Start time, end time, duration limit (e.g. 45 minutes), attempt limits.
- **Anti-Cheating Controls**:
  - Fullscreen enforcement mode.
  - Tab switch & window blur detection count logging.
  - Copy/paste clipboard restrictions on question text.
- **Auto-Evaluation**: Automatic scoring for objective questions upon submission or time expiration.
- **Analytics**: Student score cards, batch-level average score, question-wise difficulty breakdown.

---

## 9. Internal Communication & Chat
- **Batch Channels**: Dedicated chat rooms for each batch where teachers and enrolled students communicate.
- **Direct Messaging**: 1-on-1 messaging between Teacher and Student (scoped by organization policy).
- **Institute Announcements**: Broadcast announcements from OrgAdmin to all teachers and students.
- **Rich Messages**: Text messages with PDF/image attachments and read/unread indicators.

---

## 10. Events & Academic Calendar
- **Event Creation**: Title, description, date/time, location, target batch or entire organization.
- **Posters & Attachments**: Attach event schedule or flyer image.
- **Calendar View**: Monthly/weekly calendar showing exams, holidays, parent-teacher meetings, and special sessions.

---

## 11. Super Admin Panel
- **Organization Directory**: View, filter, approve, and deactivate coaching institutes.
- **Platform Analytics**: Total organizations, active teachers, active students, aggregate platform storage.
- **Subscription Management**: Track plan types, trial expirations, and future billing tiers.

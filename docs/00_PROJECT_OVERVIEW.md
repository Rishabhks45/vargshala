# 00. Project Overview: Vargshala

## 1. Product Vision
**Vargshala** is a multi-tenant SaaS platform built specifically for small to medium-sized coaching institutes, tuition centers, and educational organizations. Many of these institutions currently manage academic operations, student communications, homework, fees, and announcements through fragmented and manual channels like WhatsApp groups, paper registers, notebooks, and spreadsheets.

The primary goal of **Version 1.0 (V1)** is not to build a bloated LMS or complex video streaming platform. Instead, Vargshala provides **one organized, dependable digital workspace** that streamlines:
- Student and teacher lifecycle management.
- Batch-oriented learning and communication.
- Study notes and document distribution.
- Homework assignment and grading.
- Objective quizzes with anti-cheating measures.
- Fee structures, payment logging, and overdue tracking.
- Organization-scoped messaging and events.

---

## 2. Core Product Principles
1. **Zero-Training Usability**: Intuitive enough for non-technical coaching owners, teachers, and students to operate immediately.
2. **Organization-First Multi-Tenancy**: Every coaching institute operates within its own isolated workspace with custom branding, sessions, batches, and records.
3. **Role-Based Scoped Access**: Users only see data and features permitted by their role and assigned batches.
4. **Batch-Oriented Workflows**: Batches represent the atomic unit of learning, messaging, homework, and test distribution.
5. **Mobile-First Experience**: High responsiveness and mobile usability for students and teachers, paired with rich administrative web portals for owners.
6. **Data Privacy & Tenant Isolation**: Rigid multi-tenancy model where cross-tenant data leakage is structurally impossible.
7. **Modular Monolith Foundation**: Designed to scale effortlessly into V2 (Live streaming, Payment Gateways, WhatsApp API, AI question generators) without major rewrites.

---

## 3. User Personas & Roles

| Role | Scope & Permissions | Key Objectives |
| :--- | :--- | :--- |
| **Super Admin** | Platform-wide (Global) | Manage tenant organizations, approve registrations, monitor subscriptions, analyze storage/usage, manage system-level configurations. |
| **Organization Admin** | Tenant-wide (Coaching Owner/Principal) | Manage academic sessions, teacher/staff accounts, student admissions, batch setups, fee collections, receipts, institute announcements, and events. |
| **Teacher** | Batch/Subject Assigned Scope | Upload study materials (PDF/images), create tasks/homework, grade submissions, build quizzes, communicate with assigned batches/students. |
| **Student** | Assigned Batch Scope | Access notes, submit homework, attempt quizzes, view fee status/receipts, communicate within permitted batch channels, check institute calendar. |

---

## 4. Key Value Differentiators
- **Tailored for Tier-2 / Tier-3 & Local Institutes**: Removes enterprise LMS friction while solving day-to-day coaching headaches.
- **Batch-Scoped Communication**: Replaces chaotic WhatsApp groups with dedicated, clean, professional channels.
- **Unified Academic + Financial Management**: One dashboard for homework, exams, and fee collections.
- **Instant Self-Service Onboarding**: Institutes can register, configure academic sessions, and invite students within minutes.

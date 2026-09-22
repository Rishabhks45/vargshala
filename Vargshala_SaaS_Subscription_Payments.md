# Vargshala SaaS Subscription & Payments

## 1. Overview

Vargshala has two payment contexts:

1. **SaaS Subscription** — Organization/Coaching Institute → Vargshala.
2. **Student Fee Collection** — Student → Coaching Institute.

Both use the common `Payments` transaction table. `PaymentType` identifies the purpose.

## 2. Tables

### SubscriptionPlans

Stores SaaS plans.

- `Id`
- `Name`
- `Description`
- `Price`
- `BillingCycle`
- `MaxStudents`
- `MaxTeachers`
- `MaxBranches`
- `IsActive`
- Base entity audit fields

Example:

| Plan | Price | Billing Cycle | Max Students |
|---|---:|---|---:|
| Starter | ₹499 | Monthly | 100 |
| Standard | ₹999 | Monthly | 250 |
| Pro | ₹2,499 | Monthly | 1,000 |
| Enterprise | ₹5,999 | Monthly | Unlimited |

`NULL` limits mean unlimited.

### OrganizationSubscriptions

Stores the plan subscribed to by an organization.

- `Id`
- `OrganizationId`
- `PlanId`
- `StartDate`
- `EndDate`
- `Status`
- `AutoRenew`
- Base entity audit fields

Statuses:

```text
Trial
Active
Expired
Cancelled
Suspended
```

Relationships:

```text
Organizations 1 ───── N OrganizationSubscriptions
SubscriptionPlans 1 ───── N OrganizationSubscriptions
```

### Payments

The existing `Payments` table is the common payment transaction table.

`StudentId` must become nullable because subscription payments do not belong to a student.

Add:

```text
OrganizationSubscriptionId UUID NULL
PaymentType VARCHAR(30)
```

Payment types:

```text
StudentFee
Subscription
```

## 3. Final Payment Relationship

```text
                         Payments
                            │
                ┌───────────┴───────────┐
                │                       │
          StudentFee               Subscription
                │                       │
          StudentId               OrganizationSubscriptionId
```

```text
SubscriptionPlans
       │ 1:N
       ▼
OrganizationSubscriptions
       │ 1:N
       ▼
Payments
       └── PaymentType = Subscription

StudentFees
       │ 1:N
       ▼
Payments
       └── PaymentType = StudentFee
```

## 4. Subscription Flow

```text
Vargshala SaaS
      │
      ▼
SubscriptionPlans
      │
      │ Select Plan
      ▼
OrganizationSubscriptions
      │
      │ Payment Required
      ▼
Payments
      │
      │ PaymentType = Subscription
      ▼
Payment Successful
      │
      ▼
Subscription Status = Active
      │
      ├── Renewal → New Payment → Extend EndDate
      │
      └── Expiry → Status = Expired
```

## 5. Existing Payments Table Changes

Current fields include:

```text
Id
OrganizationId
BranchId
StudentId
ReceiptNumber
Amount
PaymentDate
PaymentMethod
TransactionReference
Status
Remarks
CreatedBy
CreatedAt
UpdatedBy
UpdatedAt
IsDeleted
DeletedBy
DeletedAt
```

### Make StudentId nullable

```sql
ALTER TABLE public."Payments"
ALTER COLUMN "StudentId" DROP NOT NULL;
```

### Add subscription reference

```sql
ALTER TABLE public."Payments"
ADD COLUMN IF NOT EXISTS "OrganizationSubscriptionId" UUID NULL;
```

### Add payment type

```sql
ALTER TABLE public."Payments"
ADD COLUMN IF NOT EXISTS "PaymentType"
VARCHAR(30) NOT NULL DEFAULT 'StudentFee';
```

### Add foreign key

```sql
ALTER TABLE public."Payments"
ADD CONSTRAINT "FK_Payments_OrganizationSubscriptions"
FOREIGN KEY ("OrganizationSubscriptionId")
REFERENCES public."OrganizationSubscriptions" ("Id")
ON DELETE RESTRICT;
```

### Add index

```sql
CREATE INDEX IF NOT EXISTS "IX_Payments_OrganizationSubscriptionId"
ON public."Payments" ("OrganizationSubscriptionId");
```

## 6. ER Diagram

```text
┌──────────────────────┐
│    Organizations     │
│ Id (PK)              │
└──────────┬───────────┘
           │ 1:N
           ▼
┌──────────────────────────────┐
│ OrganizationSubscriptions   │
│ Id (PK)                     │
│ OrganizationId (FK)         │
│ PlanId (FK)                 │
│ StartDate                   │
│ EndDate                     │
│ Status                      │
│ AutoRenew                   │
└──────────────┬───────────────┘
               │ N:1
               ▼
┌────────────────────────┐
│   SubscriptionPlans    │
│ Id (PK)                │
│ Name                   │
│ Price                  │
│ BillingCycle           │
│ MaxStudents            │
│ MaxTeachers            │
│ MaxBranches            │
│ IsActive               │
└────────────────────────┘

┌──────────────────────┐
│     StudentFees      │
│ Id (PK)              │
│ StudentId            │
│ FinalAmount          │
└──────────┬───────────┘
           │ 1:N
           ▼
┌────────────────────────────────┐
│            Payments            │
│ Id (PK)                        │
│ OrganizationId                 │
│ BranchId                       │
│ StudentId (NULL)               │
│ OrganizationSubscriptionId     │
│ ReceiptNumber                  │
│ Amount                         │
│ PaymentDate                    │
│ PaymentMethod                  │
│ TransactionReference           │
│ PaymentType                    │
│ Status                         │
│ Remarks                        │
│ Audit fields                   │
└────────────────────────────────┘
```

## 7. Final Architecture

```text
BaseEntity
    │
    ├── SubscriptionPlan
    │
    └── OrganizationSubscription

Organization
    │
    └── OrganizationSubscription

SubscriptionPlan
    │
    └── OrganizationSubscription

OrganizationSubscription
    │
    └──< Payments

StudentFee
    │
    └──< Payments
```

A subscription can have multiple payment transactions, supporting renewals, retries, failed payments, and payment history.

The existing `Payments` table remains the common transaction table instead of creating a separate `SubscriptionPayments` table.

-- =====================================================================
-- SofyCRM PostgreSQL Schema
-- Phase 1: Foundation & CRM Core
-- Phase 2: Project & Service Management
-- Phase 3: Expense & Contract
-- + Sessions / Audit Logs / Notifications
--
-- 命名規則：snake_case (對應 EFCore.NamingConventions UseSnakeCaseNamingConvention)
-- =====================================================================

-- 必要 extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =====================================================================
-- ENUM Types
-- =====================================================================
DO $$ BEGIN
    CREATE TYPE user_role          AS ENUM ('Admin', 'Sales', 'Service');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE user_status        AS ENUM ('Active', 'Disabled');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE customer_status    AS ENUM ('Potential', 'Contacting', 'Quoting', 'Won', 'Lost', 'Maintenance');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE followup_type      AS ENUM ('Call', 'Email', 'Meeting', 'Visit', 'Line');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE opportunity_status AS ENUM ('NewLead', 'Contacted', 'Proposal', 'Negotiation', 'Won', 'Lost');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE quotation_status   AS ENUM ('Draft', 'Sent', 'Accepted', 'Rejected', 'Expired');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE project_status     AS ENUM ('Planning', 'Development', 'Testing', 'UAT', 'Completed', 'Maintenance');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE task_status        AS ENUM ('Todo', 'InProgress', 'Done', 'Blocked');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE ticket_status      AS ENUM ('Open', 'Processing', 'WaitingCustomer', 'Closed');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE ticket_priority    AS ENUM ('Low', 'Medium', 'High', 'Critical');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE expense_category   AS ENUM ('Meal', 'Transportation', 'Parking', 'Gift', 'Hotel', 'Other');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE expense_status     AS ENUM ('Draft', 'Submitted', 'Approved', 'Rejected', 'Paid');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE TYPE payment_status     AS ENUM ('Pending', 'PartialPaid', 'Paid', 'Overdue');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

-- =====================================================================
-- users
-- =====================================================================
CREATE TABLE IF NOT EXISTS users (
    id              uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    name            varchar(100)  NOT NULL,
    email           varchar(255)  NOT NULL UNIQUE,
    password_hash   varchar(255)  NOT NULL,
    role            user_role     NOT NULL DEFAULT 'Sales',
    status          user_status   NOT NULL DEFAULT 'Active',
    phone           varchar(50),
    created_at      timestamptz   NOT NULL DEFAULT now(),
    updated_at      timestamptz   NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_users_email ON users(email);

-- =====================================================================
-- sessions  (JWT + Session Table)
-- =====================================================================
CREATE TABLE IF NOT EXISTS sessions (
    id               uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id          uuid       NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    refresh_token    varchar(512) NOT NULL UNIQUE,
    user_agent       varchar(512),
    ip_address       varchar(64),
    expires_at       timestamptz NOT NULL,
    revoked_at       timestamptz,
    created_at       timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_sessions_user_id ON sessions(user_id);

-- =====================================================================
-- customers
-- =====================================================================
CREATE TABLE IF NOT EXISTS customers (
    id              uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_name    varchar(200) NOT NULL,
    tax_id          varchar(50),
    address         text,
    industry        varchar(100),
    owner_user_id   uuid REFERENCES users(id) ON DELETE SET NULL,
    status          customer_status NOT NULL DEFAULT 'Potential',
    tags            text[]       NOT NULL DEFAULT '{}',
    notes           text,
    created_at      timestamptz  NOT NULL DEFAULT now(),
    updated_at      timestamptz  NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_customers_owner_user_id ON customers(owner_user_id);
CREATE INDEX IF NOT EXISTS ix_customers_status        ON customers(status);
CREATE INDEX IF NOT EXISTS ix_customers_company_name  ON customers(company_name);

-- =====================================================================
-- customer_contacts
-- =====================================================================
CREATE TABLE IF NOT EXISTS customer_contacts (
    id              uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id     uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    name            varchar(100) NOT NULL,
    title           varchar(100),
    phone           varchar(50),
    email           varchar(255),
    is_primary      boolean NOT NULL DEFAULT false,
    created_at      timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_customer_contacts_customer_id ON customer_contacts(customer_id);

-- =====================================================================
-- customer_followups
-- =====================================================================
CREATE TABLE IF NOT EXISTS customer_followups (
    id                  uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id         uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    user_id             uuid NOT NULL REFERENCES users(id),
    followup_type       followup_type NOT NULL,
    content             text NOT NULL,
    next_followup_date  timestamptz,
    created_at          timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_followups_customer_id ON customer_followups(customer_id);
CREATE INDEX IF NOT EXISTS ix_followups_user_id     ON customer_followups(user_id);

-- =====================================================================
-- opportunities
-- =====================================================================
CREATE TABLE IF NOT EXISTS opportunities (
    id                      uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id             uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    owner_user_id           uuid NOT NULL REFERENCES users(id),
    title                   varchar(200) NOT NULL,
    amount                  numeric(18,2) NOT NULL DEFAULT 0,
    status                  opportunity_status NOT NULL DEFAULT 'NewLead',
    expected_close_date     date,
    description             text,
    created_at              timestamptz NOT NULL DEFAULT now(),
    updated_at              timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_opps_customer_id    ON opportunities(customer_id);
CREATE INDEX IF NOT EXISTS ix_opps_owner_user_id  ON opportunities(owner_user_id);
CREATE INDEX IF NOT EXISTS ix_opps_status         ON opportunities(status);

-- =====================================================================
-- quotations
-- =====================================================================
CREATE TABLE IF NOT EXISTS quotations (
    id              uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id     uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    opportunity_id  uuid REFERENCES opportunities(id) ON DELETE SET NULL,
    quotation_no    varchar(50) NOT NULL UNIQUE,
    version         int  NOT NULL DEFAULT 1,
    total_amount    numeric(18,2) NOT NULL DEFAULT 0,
    status          quotation_status NOT NULL DEFAULT 'Draft',
    valid_until     date,
    notes           text,
    created_by      uuid REFERENCES users(id),
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_quotations_customer_id ON quotations(customer_id);
CREATE INDEX IF NOT EXISTS ix_quotations_status      ON quotations(status);

-- =====================================================================
-- quotation_items
-- =====================================================================
CREATE TABLE IF NOT EXISTS quotation_items (
    id                  uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    quotation_id        uuid NOT NULL REFERENCES quotations(id) ON DELETE CASCADE,
    item_name           varchar(200) NOT NULL,
    description         text,
    qty                 int NOT NULL DEFAULT 1,
    unit_price          numeric(18,2) NOT NULL DEFAULT 0,
    estimated_hours     numeric(10,2) NOT NULL DEFAULT 0,
    sort_order          int NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS ix_quotation_items_quotation_id ON quotation_items(quotation_id);

-- =====================================================================
-- projects
-- =====================================================================
CREATE TABLE IF NOT EXISTS projects (
    id              uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id     uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    project_name    varchar(200) NOT NULL,
    pm_user_id      uuid REFERENCES users(id),
    start_date      date,
    end_date        date,
    status          project_status NOT NULL DEFAULT 'Planning',
    description     text,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_projects_customer_id ON projects(customer_id);
CREATE INDEX IF NOT EXISTS ix_projects_pm_user_id  ON projects(pm_user_id);
CREATE INDEX IF NOT EXISTS ix_projects_status      ON projects(status);

-- =====================================================================
-- project_tasks
-- =====================================================================
CREATE TABLE IF NOT EXISTS project_tasks (
    id                  uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id          uuid NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    assigned_user_id    uuid REFERENCES users(id),
    title               varchar(200) NOT NULL,
    description         text,
    status              task_status NOT NULL DEFAULT 'Todo',
    estimated_hours     numeric(10,2) NOT NULL DEFAULT 0,
    actual_hours        numeric(10,2) NOT NULL DEFAULT 0,
    due_date            date,
    created_at          timestamptz NOT NULL DEFAULT now(),
    updated_at          timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_tasks_project_id        ON project_tasks(project_id);
CREATE INDEX IF NOT EXISTS ix_tasks_assigned_user_id  ON project_tasks(assigned_user_id);

-- =====================================================================
-- tickets
-- =====================================================================
CREATE TABLE IF NOT EXISTS tickets (
    id                  uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id         uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    project_id          uuid REFERENCES projects(id) ON DELETE SET NULL,
    assigned_user_id    uuid REFERENCES users(id),
    priority            ticket_priority NOT NULL DEFAULT 'Medium',
    title               varchar(200) NOT NULL,
    content             text,
    status              ticket_status NOT NULL DEFAULT 'Open',
    sla_due_at          timestamptz,
    closed_at           timestamptz,
    created_by          uuid REFERENCES users(id),
    created_at          timestamptz NOT NULL DEFAULT now(),
    updated_at          timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_tickets_customer_id       ON tickets(customer_id);
CREATE INDEX IF NOT EXISTS ix_tickets_assigned_user_id  ON tickets(assigned_user_id);
CREATE INDEX IF NOT EXISTS ix_tickets_status            ON tickets(status);

-- =====================================================================
-- work_logs
-- =====================================================================
CREATE TABLE IF NOT EXISTS work_logs (
    id              uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id      uuid NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    user_id         uuid NOT NULL REFERENCES users(id),
    work_date       date NOT NULL,
    hours           numeric(6,2) NOT NULL,
    description     text,
    created_at      timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_work_logs_project_id ON work_logs(project_id);
CREATE INDEX IF NOT EXISTS ix_work_logs_user_id    ON work_logs(user_id);

-- =====================================================================
-- expenses
-- =====================================================================
CREATE TABLE IF NOT EXISTS expenses (
    id              uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id         uuid NOT NULL REFERENCES users(id),
    customer_id     uuid REFERENCES customers(id) ON DELETE SET NULL,
    category        expense_category NOT NULL,
    amount          numeric(18,2) NOT NULL,
    expense_date    date NOT NULL,
    receipt_url     varchar(500),
    description     text,
    status          expense_status NOT NULL DEFAULT 'Draft',
    approved_by     uuid REFERENCES users(id),
    approved_at     timestamptz,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_expenses_user_id ON expenses(user_id);
CREATE INDEX IF NOT EXISTS ix_expenses_status  ON expenses(status);

-- =====================================================================
-- contracts
-- =====================================================================
CREATE TABLE IF NOT EXISTS contracts (
    id                      uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id             uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    contract_name           varchar(200) NOT NULL,
    start_date              date,
    end_date                date,
    renewal_notice_days     int NOT NULL DEFAULT 30,
    file_url                varchar(500),
    notes                   text,
    owner_user_id           uuid REFERENCES users(id) ON DELETE SET NULL,
    created_at              timestamptz NOT NULL DEFAULT now(),
    updated_at              timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_contracts_customer_id ON contracts(customer_id);
CREATE INDEX IF NOT EXISTS ix_contracts_end_date    ON contracts(end_date);
CREATE INDEX IF NOT EXISTS ix_contracts_owner       ON contracts(owner_user_id);

-- 既有 DB 升級用（部署時若已存在 contracts 表也能補欄位）
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS owner_user_id uuid REFERENCES users(id) ON DELETE SET NULL;

-- =====================================================================
-- invoices
-- =====================================================================
CREATE TABLE IF NOT EXISTS invoices (
    id              uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id     uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    invoice_no      varchar(50) NOT NULL UNIQUE,
    amount          numeric(18,2) NOT NULL,
    issued_date     date,
    due_date        date,
    payment_status  payment_status NOT NULL DEFAULT 'Pending',
    paid_amount     numeric(18,2) NOT NULL DEFAULT 0,
    notes           text,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_invoices_customer_id    ON invoices(customer_id);
CREATE INDEX IF NOT EXISTS ix_invoices_payment_status ON invoices(payment_status);

-- =====================================================================
-- notifications (Phase 2)
-- =====================================================================
CREATE TABLE IF NOT EXISTS notifications (
    id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id     uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title       varchar(200) NOT NULL,
    message     text,
    type        varchar(50) NOT NULL,
    is_read     boolean NOT NULL DEFAULT false,
    link        varchar(500),
    created_at  timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_notifications_user_id ON notifications(user_id);

-- =====================================================================
-- audit_logs  (Audit.NET 寫入)
-- =====================================================================
CREATE TABLE IF NOT EXISTS audit_logs (
    id              uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id         uuid REFERENCES users(id) ON DELETE SET NULL,
    module          varchar(100) NOT NULL,
    action          varchar(100) NOT NULL,
    entity_id       varchar(100),
    before_data     jsonb,
    after_data      jsonb,
    ip_address      varchar(64),
    user_agent      varchar(512),
    created_at      timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX IF NOT EXISTS ix_audit_logs_module  ON audit_logs(module);
CREATE INDEX IF NOT EXISTS ix_audit_logs_created_at ON audit_logs(created_at DESC);

-- =====================================================================
-- updated_at trigger
-- =====================================================================
CREATE OR REPLACE FUNCTION trg_set_updated_at() RETURNS trigger AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DO $$
DECLARE
    t text;
BEGIN
    FOR t IN
        SELECT table_name
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND column_name = 'updated_at'
    LOOP
        EXECUTE format(
          'DROP TRIGGER IF EXISTS trg_%I_updated_at ON %I;
           CREATE TRIGGER trg_%I_updated_at
           BEFORE UPDATE ON %I
           FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();',
          t, t, t, t);
    END LOOP;
END $$;

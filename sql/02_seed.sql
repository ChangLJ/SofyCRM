-- =====================================================================
-- SofyCRM Seed Data
-- 預設使用者密碼 (BCrypt cost=11):
--   admin@sofycrm.local    / Admin@123
--   sales@sofycrm.local    / Sales@123
--   service@sofycrm.local  / Service@123
-- =====================================================================

-- 固定 UUID 方便 FK 對應
DO $$
DECLARE
    admin_id    uuid := '11111111-1111-1111-1111-111111111111';
    sales_id    uuid := '22222222-2222-2222-2222-222222222222';
    service_id  uuid := '33333333-3333-3333-3333-333333333333';
    cust1_id    uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1';
    cust2_id    uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2';
    cust3_id    uuid := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3';
    opp1_id     uuid := 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1';
    opp2_id     uuid := 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2';
    quot1_id    uuid := 'cccccccc-cccc-cccc-cccc-cccccccccc01';
    proj1_id    uuid := 'dddddddd-dddd-dddd-dddd-ddddddddddd1';
    tk1_id      uuid := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee';
BEGIN

-- =====================================================================
-- Users
-- =====================================================================
INSERT INTO users (id, name, email, password_hash, role, status, phone) VALUES
  (admin_id,   '系統管理員', 'admin@sofycrm.local',   '$2b$11$Xj.pw5X4OMG.pE9WpwTWROrIqTDNpFvqFnJWvahjxa12kVMyz5pxa', 'Admin',   'Active', '0900000001'),
  (sales_id,   '王業務',     'sales@sofycrm.local',   '$2b$11$kk3l8Hir8NjsaSI2M6KxxO4NjSnVQOd1K.92fm4uhfJn4cF2yhbjC', 'Sales',   'Active', '0900000002'),
  (service_id, '陳客服',     'service@sofycrm.local', '$2b$11$HU0RoCjTUDfuclbY4btbzOuFwB/dO.3Yv0H4h99WwlZl3xYt8.0ka', 'Service', 'Active', '0900000003'),
  -- Demo 簡易帳號：直接以 "Admin" 為登入帳號（後端會 lower → "admin"）
  ('44444444-4444-4444-4444-444444444444',
                '系統管理員', 'admin',                 '$2b$11$TXQo/mQ/N96Gqau36IOXg.bOEgyefcJGgdBPS2JpwObMBsXXNLrja', 'Admin',   'Active', NULL)
ON CONFLICT (email) DO NOTHING;

-- =====================================================================
-- Customers
-- =====================================================================
INSERT INTO customers (id, company_name, tax_id, address, industry, owner_user_id, status, tags, notes) VALUES
  (cust1_id, 'Acme 股份有限公司', '12345678', '台北市信義區信義路五段7號', '製造業', sales_id, 'Contacting',   ARRAY['VIP','長期'],     '由王業務跟進'),
  (cust2_id, 'Beta 科技有限公司', '23456789', '新北市板橋區文化路一段100號', '軟體業', sales_id, 'Quoting',     ARRAY['新客戶'],          '正在報價中'),
  (cust3_id, 'Gamma 工業',       '34567890', '台中市西屯區市政北二路1號',   '貿易業', admin_id, 'Maintenance', ARRAY['維護中'],          '舊客戶轉維護')
ON CONFLICT (id) DO NOTHING;

-- =====================================================================
-- Customer Contacts
-- =====================================================================
INSERT INTO customer_contacts (customer_id, name, title, phone, email, is_primary) VALUES
  (cust1_id, '張總經理', 'CEO',      '02-12345678',     'ceo@acme.example',  true),
  (cust1_id, '李採購',   'Purchasing','02-12345679',    'buy@acme.example',  false),
  (cust2_id, '林經理',   'PM',       '02-22223333',     'pm@beta.example',   true),
  (cust3_id, '黃廠長',   '廠長',     '04-33334444',     'plant@gamma.example', true);

-- =====================================================================
-- Followups
-- =====================================================================
INSERT INTO customer_followups (customer_id, user_id, followup_type, content, next_followup_date) VALUES
  (cust1_id, sales_id,   'Call',    '電話確認需求，客戶希望下週報價',           now() + interval '3 days'),
  (cust1_id, sales_id,   'Meeting', '到府拜訪，已展示產品 Demo',                 now() + interval '7 days'),
  (cust2_id, sales_id,   'Email',   '寄送初步估價單',                            now() + interval '2 days'),
  (cust3_id, service_id, 'Visit',   '到場處理客戶反映之問題，現場排除',          now() + interval '14 days');

-- =====================================================================
-- Opportunities
-- =====================================================================
INSERT INTO opportunities (id, customer_id, owner_user_id, title, amount, status, expected_close_date, description) VALUES
  (opp1_id, cust1_id, sales_id, 'Acme ERP 導入專案',  1200000.00, 'Proposal',    CURRENT_DATE + 30, '客戶有意導入 ERP 完整方案'),
  (opp2_id, cust2_id, sales_id, 'Beta 官網改版',       350000.00, 'Negotiation', CURRENT_DATE + 14, '報價中，等待客戶決議')
ON CONFLICT (id) DO NOTHING;

-- =====================================================================
-- Quotations + Items
-- =====================================================================
INSERT INTO quotations (id, customer_id, opportunity_id, quotation_no, version, total_amount, status, valid_until, notes, created_by) VALUES
  (quot1_id, cust1_id, opp1_id, 'Q-20260518-001', 1, 1200000.00, 'Sent', CURRENT_DATE + 30, '含一年保固', sales_id)
ON CONFLICT (id) DO NOTHING;

INSERT INTO quotation_items (quotation_id, item_name, description, qty, unit_price, estimated_hours, sort_order) VALUES
  (quot1_id, 'ERP 軟體授權',    '永久授權 50 人',   1,  800000.00,   0, 1),
  (quot1_id, '導入服務',        '系統建置 & 教育訓練', 1,  350000.00, 240, 2),
  (quot1_id, '一年原廠保固',    '系統維護',         1,   50000.00,   0, 3);

-- =====================================================================
-- Projects + Tasks
-- =====================================================================
INSERT INTO projects (id, customer_id, project_name, pm_user_id, start_date, end_date, status, description) VALUES
  (proj1_id, cust1_id, 'Acme ERP 導入', service_id, CURRENT_DATE - 10, CURRENT_DATE + 80, 'Development', '依合約已開案')
ON CONFLICT (id) DO NOTHING;

INSERT INTO project_tasks (project_id, assigned_user_id, title, description, status, estimated_hours, actual_hours, due_date) VALUES
  (proj1_id, service_id, '需求訪談',     '完成現場訪談並整理 SA',  'Done',       16,  18, CURRENT_DATE - 5),
  (proj1_id, service_id, '系統建置',     '主機環境準備',           'InProgress', 40,  20, CURRENT_DATE + 10),
  (proj1_id, service_id, '教育訓練',     '使用者訓練',             'Todo',       16,   0, CURRENT_DATE + 60);

-- =====================================================================
-- Tickets
-- =====================================================================
INSERT INTO tickets (id, customer_id, project_id, assigned_user_id, priority, title, content, status, sla_due_at, created_by) VALUES
  (tk1_id, cust1_id, proj1_id, service_id, 'High', '報表匯出失敗', '使用者按下匯出後系統報錯', 'Processing', now() + interval '1 day', sales_id);

-- =====================================================================
-- Work Logs
-- =====================================================================
INSERT INTO work_logs (project_id, user_id, work_date, hours, description) VALUES
  (proj1_id, service_id, CURRENT_DATE - 3, 4.0, '需求訪談會議'),
  (proj1_id, service_id, CURRENT_DATE - 2, 6.0, '撰寫訪談紀錄'),
  (proj1_id, service_id, CURRENT_DATE - 1, 8.0, '系統環境建置');

-- =====================================================================
-- Expenses
-- =====================================================================
INSERT INTO expenses (user_id, customer_id, category, amount, expense_date, description, status) VALUES
  (sales_id,   cust1_id, 'Meal',           1200.00, CURRENT_DATE - 5, '客戶午餐會議',     'Submitted'),
  (sales_id,   cust2_id, 'Transportation',  800.00, CURRENT_DATE - 3, '高鐵往返',         'Approved'),
  (service_id, cust1_id, 'Parking',         200.00, CURRENT_DATE - 2, '到客戶端停車費',   'Draft');

-- =====================================================================
-- Contracts
-- =====================================================================
INSERT INTO contracts (customer_id, contract_name, start_date, end_date, renewal_notice_days, notes) VALUES
  (cust1_id, 'Acme ERP 主合約',      CURRENT_DATE - 10, CURRENT_DATE + 355, 30, '一年期'),
  (cust3_id, 'Gamma 系統維護合約',   CURRENT_DATE - 200, CURRENT_DATE + 165, 30, '維護合約');

-- =====================================================================
-- Invoices
-- =====================================================================
INSERT INTO invoices (customer_id, invoice_no, amount, issued_date, due_date, payment_status, paid_amount) VALUES
  (cust1_id, 'INV-202605-0001', 600000.00, CURRENT_DATE - 7, CURRENT_DATE + 23, 'Pending',     0),
  (cust3_id, 'INV-202604-0098', 120000.00, CURRENT_DATE - 40, CURRENT_DATE - 10, 'Overdue',     0),
  (cust2_id, 'INV-202605-0002',  88000.00, CURRENT_DATE - 5,  CURRENT_DATE + 25, 'PartialPaid', 40000);

-- =====================================================================
-- Notifications
-- =====================================================================
INSERT INTO notifications (user_id, title, message, type, link) VALUES
  (admin_id,   '系統初始化完成', 'Seed data 已建立',                   'System',         '/dashboard'),
  (sales_id,   '今日跟進',       '您有 2 筆客戶今日需追蹤',            'FollowupReminder','/followups'),
  (service_id, 'SLA 即將逾時',   'Ticket TK-0001 將於 1 天內到期',     'SlaWarning',      '/tickets');

END $$;

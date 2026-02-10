-- ====================================================
-- إضافة تصنيفات وأنواع المرافق الخدمية الافتراضية
-- تشغيل مرة واحدة بعد Migration
-- ====================================================

USE [WaqfGIS]
GO

-- ====================================================
-- التصنيفات الرئيسية
-- ====================================================

-- 1. المرافق الأساسية
INSERT INTO ServiceCategories (NameAr, NameEn, Description, DefaultIconClass, DefaultIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
('المرافق الأساسية', 'Basic Infrastructure', 'خدمات الكهرباء والماء والغاز', 'fas fa-bolt', '#FBBF24', 1, 1, 'System', GETDATE());

DECLARE @InfrastructureId INT = SCOPE_IDENTITY();

-- 2. الخدمات الصحية
INSERT INTO ServiceCategories (NameAr, NameEn, Description, DefaultIconClass, DefaultIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
('الخدمات الصحية', 'Healthcare', 'المستشفيات والمراكز الصحية', 'fas fa-hospital', '#EF4444', 2, 1, 'System', GETDATE());

DECLARE @HealthcareId INT = SCOPE_IDENTITY();

-- 3. الخدمات التعليمية
INSERT INTO ServiceCategories (NameAr, NameEn, Description, DefaultIconClass, DefaultIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
('الخدمات التعليمية', 'Education', 'المدارس والجامعات والمعاهد', 'fas fa-school', '#8B5CF6', 3, 1, 'System', GETDATE());

DECLARE @EducationId INT = SCOPE_IDENTITY();

-- 4. الخدمات الأمنية
INSERT INTO ServiceCategories (NameAr, NameEn, Description, DefaultIconClass, DefaultIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
('الخدمات الأمنية', 'Security', 'الشرطة والإطفاء والدفاع المدني', 'fas fa-shield-alt', '#1E40AF', 4, 1, 'System', GETDATE());

DECLARE @SecurityId INT = SCOPE_IDENTITY();

-- 5. المواصلات
INSERT INTO ServiceCategories (NameAr, NameEn, Description, DefaultIconClass, DefaultIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
('المواصلات', 'Transportation', 'محطات الوقود والمواصلات العامة', 'fas fa-gas-pump', '#EC4899', 5, 1, 'System', GETDATE());

DECLARE @TransportId INT = SCOPE_IDENTITY();

-- 6. الخدمات التجارية
INSERT INTO ServiceCategories (NameAr, NameEn, Description, DefaultIconClass, DefaultIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
('الخدمات التجارية', 'Commercial', 'الأسواق والبنوك والمراكز التجارية', 'fas fa-shopping-cart', '#10B981', 6, 1, 'System', GETDATE());

DECLARE @CommercialId INT = SCOPE_IDENTITY();

-- ====================================================
-- الأنواع التابعة لكل تصنيف
-- ====================================================

-- أنواع المرافق الأساسية
INSERT INTO ServiceTypes (ServiceCategoryId, NameAr, NameEn, CustomIconClass, CustomIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
(@InfrastructureId, 'محطة كهرباء', 'Power Station', 'fas fa-bolt', '#FBBF24', 1, 1, 'System', GETDATE()),
(@InfrastructureId, 'محطة ماء', 'Water Station', 'fas fa-water', '#06B6D4', 2, 1, 'System', GETDATE()),
(@InfrastructureId, 'محطة غاز', 'Gas Station', 'fas fa-fire', '#F97316', 3, 1, 'System', GETDATE()),
(@InfrastructureId, 'محطة صرف صحي', 'Sewage Station', 'fas fa-tint', '#0891B2', 4, 1, 'System', GETDATE()),
(@InfrastructureId, 'كهرباء فرعية', 'Substation', 'fas fa-plug', '#FDE047', 5, 1, 'System', GETDATE());

-- أنواع الخدمات الصحية
INSERT INTO ServiceTypes (ServiceCategoryId, NameAr, NameEn, CustomIconClass, CustomIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
(@HealthcareId, 'مستشفى', 'Hospital', 'fas fa-hospital', '#EF4444', 1, 1, 'System', GETDATE()),
(@HealthcareId, 'مركز صحي', 'Health Center', 'fas fa-clinic-medical', '#F87171', 2, 1, 'System', GETDATE()),
(@HealthcareId, 'عيادة', 'Clinic', 'fas fa-stethoscope', '#FCA5A5', 3, 1, 'System', GETDATE()),
(@HealthcareId, 'صيدلية', 'Pharmacy', 'fas fa-prescription-bottle-medical', '#DC2626', 4, 1, 'System', GETDATE()),
(@HealthcareId, 'مختبر', 'Laboratory', 'fas fa-flask', '#FEE2E2', 5, 1, 'System', GETDATE());

-- أنواع الخدمات التعليمية
INSERT INTO ServiceTypes (ServiceCategoryId, NameAr, NameEn, CustomIconClass, CustomIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
(@EducationId, 'مدرسة ابتدائية', 'Primary School', 'fas fa-school', '#8B5CF6', 1, 1, 'System', GETDATE()),
(@EducationId, 'مدرسة متوسطة', 'Middle School', 'fas fa-school', '#7C3AED', 2, 1, 'System', GETDATE()),
(@EducationId, 'مدرسة ثانوية', 'High School', 'fas fa-school', '#6D28D9', 3, 1, 'System', GETDATE()),
(@EducationId, 'جامعة', 'University', 'fas fa-university', '#5B21B6', 4, 1, 'System', GETDATE()),
(@EducationId, 'معهد', 'Institute', 'fas fa-graduation-cap', '#A78BFA', 5, 1, 'System', GETDATE()),
(@EducationId, 'مكتبة', 'Library', 'fas fa-book', '#C4B5FD', 6, 1, 'System', GETDATE()),
(@EducationId, 'روضة أطفال', 'Kindergarten', 'fas fa-child', '#DDD6FE', 7, 1, 'System', GETDATE());

-- أنواع الخدمات الأمنية
INSERT INTO ServiceTypes (ServiceCategoryId, NameAr, NameEn, CustomIconClass, CustomIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
(@SecurityId, 'مركز شرطة', 'Police Station', 'fas fa-shield-alt', '#1E40AF', 1, 1, 'System', GETDATE()),
(@SecurityId, 'محطة إطفاء', 'Fire Station', 'fas fa-fire-extinguisher', '#DC2626', 2, 1, 'System', GETDATE()),
(@SecurityId, 'دفاع مدني', 'Civil Defense', 'fas fa-first-aid', '#B91C1C', 3, 1, 'System', GETDATE()),
(@SecurityId, 'نقطة تفتيش', 'Checkpoint', 'fas fa-stop', '#7C3AED', 4, 1, 'System', GETDATE());

-- أنواع المواصلات
INSERT INTO ServiceTypes (ServiceCategoryId, NameAr, NameEn, CustomIconClass, CustomIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
(@TransportId, 'محطة وقود', 'Gas Pump', 'fas fa-gas-pump', '#EC4899', 1, 1, 'System', GETDATE()),
(@TransportId, 'موقف سيارات', 'Parking', 'fas fa-parking', '#6B7280', 2, 1, 'System', GETDATE()),
(@TransportId, 'محطة حافلات', 'Bus Station', 'fas fa-bus', '#8B5CF6', 3, 1, 'System', GETDATE()),
(@TransportId, 'محطة قطار', 'Train Station', 'fas fa-train', '#6366F1', 4, 1, 'System', GETDATE()),
(@TransportId, 'مطار', 'Airport', 'fas fa-plane', '#4F46E5', 5, 1, 'System', GETDATE()),
(@TransportId, 'موقف تاكسي', 'Taxi Stand', 'fas fa-taxi', '#F472B6', 6, 1, 'System', GETDATE());

-- أنواع الخدمات التجارية
INSERT INTO ServiceTypes (ServiceCategoryId, NameAr, NameEn, CustomIconClass, CustomIconColor, DisplayOrder, IsActive, CreatedBy, CreatedAt)
VALUES 
(@CommercialId, 'سوق', 'Market', 'fas fa-shopping-basket', '#10B981', 1, 1, 'System', GETDATE()),
(@CommercialId, 'مركز تجاري', 'Shopping Mall', 'fas fa-shopping-cart', '#059669', 2, 1, 'System', GETDATE()),
(@CommercialId, 'بنك', 'Bank', 'fas fa-university', '#3B82F6', 3, 1, 'System', GETDATE()),
(@CommercialId, 'صراف آلي', 'ATM', 'fas fa-credit-card', '#60A5FA', 4, 1, 'System', GETDATE()),
(@CommercialId, 'مطعم', 'Restaurant', 'fas fa-utensils', '#F59E0B', 5, 1, 'System', GETDATE()),
(@CommercialId, 'كافيه', 'Cafe', 'fas fa-coffee', '#D97706', 6, 1, 'System', GETDATE()),
(@CommercialId, 'فندق', 'Hotel', 'fas fa-hotel', '#8B5CF6', 7, 1, 'System', GETDATE()),
(@CommercialId, 'محل بقالة', 'Grocery', 'fas fa-store-alt', '#34D399', 8, 1, 'System', GETDATE());

GO

PRINT '✅ تم إضافة التصنيفات والأنواع بنجاح!';
PRINT '';
PRINT '📊 الإحصائيات:';
SELECT 
    sc.NameAr as [التصنيف],
    COUNT(st.Id) as [عدد الأنواع]
FROM ServiceCategories sc
LEFT JOIN ServiceTypes st ON st.ServiceCategoryId = sc.Id
GROUP BY sc.NameAr, sc.DisplayOrder
ORDER BY sc.DisplayOrder;

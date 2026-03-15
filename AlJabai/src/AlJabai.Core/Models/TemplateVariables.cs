namespace AlJabai.Core.Constants;

public static class TemplateVariables
{
    public static readonly List<TemplateVariableInfo> All =
    [
        new("tenant_name", "اسم المستأجر", "المستأجر", "محمد أحمد الزهراني"),
        new("tenant_id", "رقم الهوية / السجل", "المستأجر", "1234567890"),
        new("tenant_phone", "رقم هاتف المستأجر", "المستأجر", "0501234567"),
        new("tenant_email", "البريد الإلكتروني", "المستأجر", "tenant@email.com"),
        new("tenant_type", "نوع المستأجر", "المستأجر", "فرد"),
        new("tenant_address", "عنوان المستأجر", "المستأجر", "الرياض، حي النزهة"),
        new("tenant_company_name", "اسم الشركة", "المستأجر", "شركة الأمل للتجارة"),
        new("tenant_authorized_person", "المفوض بالتوقيع", "المستأجر", "خالد عبدالله"),
        new("tenant_tax_number", "الرقم الضريبي", "المستأجر", "310123456700003"),
        new("property_name", "اسم العقار", "العقار", "مجمع النخيل التجاري"),
        new("property_address", "عنوان العقار", "العقار", "شارع الملك فهد، الرياض"),
        new("property_city", "المدينة", "العقار", "الرياض"),
        new("property_governorate", "المحافظة", "العقار", "منطقة الرياض"),
        new("property_type", "نوع العقار", "العقار", "تجاري"),
        new("unit_number", "رقم الوحدة", "العقار", "B-204"),
        new("unit_floor", "الدور", "العقار", "الثاني"),
        new("unit_area", "المساحة (م²)", "العقار", "150"),
        new("contract_number", "رقم العقد", "العقد", "PMS-2024-000001"),
        new("contract_type_name", "نوع العقد", "العقد", "إيجار سنوي"),
        new("start_date", "تاريخ بداية العقد", "العقد", "01/01/2025"),
        new("end_date", "تاريخ انتهاء العقد", "العقد", "31/12/2025"),
        new("contract_duration_months", "مدة العقد بالأشهر", "العقد", "12"),
        new("contract_duration_years", "مدة العقد بالسنوات", "العقد", "1"),
        new("total_contract_years", "إجمالي سنوات العقد", "العقد", "3"),
        new("notes", "ملاحظات العقد", "العقد", ""),
        new("rent_amount", "قيمة الإيجار بالأرقام", "مالية", "5,000.000"),
        new("rent_amount_words", "قيمة الإيجار بالحروف", "مالية", "خمسة آلاف دينار"),
        new("payment_cycle", "دورة الدفع", "مالية", "شهري"),
        new("payment_day", "يوم الاستحقاق", "مالية", "الأول"),
        new("annual_increase", "نسبة الزيادة السنوية", "مالية", "3%"),
        new("security_deposit", "مبلغ التأمين بالأرقام", "مالية", "10,000.000"),
        new("security_deposit_words", "مبلغ التأمين بالحروف", "مالية", "عشرة آلاف دينار"),
        new("total_contract_value", "إجمالي قيمة العقد", "مالية", "60,000.000"),
        new("total_contract_value_words", "إجمالي العقد بالحروف", "مالية", "ستون ألف دينار"),
        new("landlord_name", "اسم الجهة المؤجرة", "المؤجر", "هيئة إدارة الممتلكات"),
        new("landlord_representative", "اسم المفوض عن المؤجر", "المؤجر", "عبدالله محمد"),
        new("landlord_phone", "هاتف الجهة المؤجرة", "المؤجر", "0112345678"),
        new("landlord_address", "عنوان الجهة المؤجرة", "المؤجر", "الرياض، طريق الملك عبدالعزيز"),
        new("today_date", "تاريخ اليوم", "النظام", "15/06/2024"),
        new("today_date_hijri", "تاريخ اليوم هجري", "النظام", "08/12/1445"),
        new("contract_year", "سنة العقد", "النظام", "2024"),
        new("sequential_number", "الرقم التسلسلي", "النظام", "000001"),
        new("project_name", "اسم المشروع", "BOT", "مشروع النخيل"),
        new("operation_period", "فترة التشغيل", "BOT", "10 سنوات"),
        new("transfer_date", "تاريخ نقل الملكية", "BOT", "01/01/2035"),
        new("commercial_use", "الغرض التجاري", "تجاري", "بيع الملابس"),
        new("commercial_license_number", "رقم الرخصة التجارية", "تجاري", "CR-12345"),
        new("year_by_year_schedule", "جدول الإيجار السنوي", "متعدد السنوات", "السنة 1: 5000، السنة 2: 5150")
    ];

    public static IEnumerable<string> Groups => All.Select(v => v.Group).Distinct();
    public static IEnumerable<TemplateVariableInfo> GetByGroup(string group) => All.Where(v => v.Group == group);
    public static bool IsKnown(string variableName) => All.Any(v => v.Key == variableName);
}

public record TemplateVariableInfo(string Key, string Label, string Group, string SampleValue)
{
    public string Placeholder => "{{" + Key + "}}";
}

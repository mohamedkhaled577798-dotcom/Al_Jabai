namespace WaqfSystem.Core.Enums
{
    // =============================================
    // PROPERTY ENUMS
    // =============================================

    /// <summary>نوع العقار</summary>
    public enum PropertyType : byte
    {
        Mosque = 0,            // مسجد
        Hussainiya = 1,        // حسينية
        School = 2,            // مدرسة
        Hospital = 3,          // مستشفى
        CommercialBuilding = 4,// مبنى تجاري
        ResidentialBuilding = 5,// مبنى سكني
        MixedUse = 6,          // مبنى مختلط
        Land = 7,              // أرض
        Farm = 8,              // مزرعة
        Cemetery = 9,          // مقبرة
        Other = 10,            // أخرى
        Agricultural = 11      // زراعي
    }

    /// <summary>تصنيف العقار</summary>
    public enum PropertyCategory : byte
    {
        Building = 0,          // مبنى
        Agricultural = 1,     // زراعي
        Land = 2               // أرض
    }

    /// <summary>نوع الوقف</summary>
    public enum WaqfType : byte
    {
        Khairi = 0,            // وقف خيري
        Dhurri = 1,            // وقف ذري
        Mushtarak = 2          // وقف مشترك
    }

    /// <summary>نوع الملكية</summary>
    public enum OwnershipType : byte
    {
        FullWaqf = 0,          // مضبوط - ملكية كاملة
        Partnership = 1        // ملحق - شراكة
    }

    /// <summary>حالة العقار</summary>
    public enum PropertyStatus : byte
    {
        Active = 0,            // فعال
        UnderMaintenance = 1,  // قيد الصيانة
        Abandoned = 2,         // مهجور
        Disputed = 3,          // متنازع عليه
        Demolished = 4,        // مهدم
        UnderConstruction = 5  // قيد الإنشاء
    }

    /// <summary>نوع البناء</summary>
    public enum ConstructionType : byte
    {
        Concrete = 0,          // خرساني
        Steel = 1,             // فولاذي
        Brick = 2,             // طابوقي
        Mixed = 3,             // مختلط
        Wood = 4,              // خشبي
        Stone = 5,             // حجري
        AdobeMud = 6           // طيني
    }

    /// <summary>الحالة الإنشائية</summary>
    public enum StructuralCondition : byte
    {
        Excellent = 0,         // ممتاز
        Good = 1,              // جيد
        Average = 2,           // متوسط
        Poor = 3,              // سيء
        Dangerous = 4          // خطر
    }

    /// <summary>استخدام الطابق</summary>
    public enum FloorUsage : byte
    {
        Residential = 0,       // سكني
        Commercial = 1,        // تجاري
        Office = 2,            // مكتبي
        Religious = 3,         // ديني
        Mixed = 4,             // مختلط
        Storage = 5,           // مخزن
        Parking = 6            // موقف
    }

    /// <summary>نوع الوحدة</summary>
    public enum UnitType : byte
    {
        Apartment = 0,         // شقة
        Shop = 1,              // محل
        Office = 2,            // مكتب
        Warehouse = 3,         // مخزن
        HotelUnit = 4,         // وحدة فندقية
        Studio = 5             // ستوديو
    }

    /// <summary>حالة الإشغال</summary>
    public enum OccupancyStatus : byte
    {
        Vacant = 0,            // شاغر
        Rented = 1,            // مؤجر
        Closed = 2,            // مغلق
        UnderMaintenance = 3,  // قيد الصيانة
        WithPartner = 4        // مع شريك
    }

    /// <summary>حالة التأثيث</summary>
    public enum FurnishedStatus : byte
    {
        Furnished = 0,         // مفروش
        Unfurnished = 1,       // غير مفروش
        SemiFurnished = 2      // شبه مفروش
    }

    /// <summary>نوع الغرفة</summary>
    public enum RoomType : byte
    {
        Bedroom = 0,           // غرفة نوم
        LivingRoom = 1,        // غرفة معيشة
        Kitchen = 2,           // مطبخ
        Bathroom = 3,          // حمام
        MaidRoom = 4,          // غرفة خادمة
        Storage = 5,           // مخزن
        Office = 6,            // مكتب
        MeetingRoom = 7        // قاعة اجتماعات
    }

    /// <summary>نوع المرفق</summary>
    public enum FacilityType : byte
    {
        Parking = 0,           // موقف
        Elevator = 1,          // مصعد
        Generator = 2,         // مولد
        CentralAC = 3,         // تكييف مركزي
        Rooftop = 4,           // سطح
        Tank = 5,              // خزان
        SecurityCameras = 6,   // كاميرات أمنية
        PrayerRoom = 7         // مصلى
    }

    /// <summary>نوع العداد</summary>
    public enum MeterType : byte
    {
        Electric = 0,          // كهرباء
        Water = 1,             // ماء
        Gas = 2                // غاز
    }

    // Partnership enums are defined in PartnershipEnums.cs

    /// <summary>نوع التربة</summary>
    public enum SoilType : byte
    {
        Clay = 0,              // طينية
        Sandy = 1,             // رملية
        ClaySandy = 2,         // طينية رملية
        Saline = 3,            // ملحية
        Sabkha = 4             // سبخة
    }

    /// <summary>مصدر المياه</summary>
    public enum WaterSourceType : byte
    {
        River = 0,             // نهر
        Canal = 1,             // قناة
        ArtesianWell = 2,      // بئر ارتوازي
        Rain = 3,              // مطر
        Rainwater = 4          // ماء مطري مجمع
    }

    /// <summary>طريقة الري</summary>
    public enum IrrigationMethod : byte
    {
        Flood = 0,             // غمر
        Sprinkler = 1,         // رش
        Drip = 2,              // تنقيط
        Traditional = 3        // تقليدي
    }

    /// <summary>نوع الموسم</summary>
    public enum SeasonType : byte
    {
        Winter = 0,            // شتوي
        Summer = 1,            // صيفي
        TwoSeasons = 2,        // موسمين
        Permanent = 3          // دائم
    }

    /// <summary>نوع عقد الزراعة</summary>
    public enum FarmingContractType : byte
    {
        Muzaraa = 0,           // مزارعة
        Musaqa = 1,            // مساقاة
        CashRent = 2,          // إيجار نقدي
        SelfFarmed = 3         // زراعة ذاتية
    }

    /// <summary>تصنيف الوثيقة</summary>
    public enum DocumentCategory : byte
    {
        Ownership = 0,         // ملكية
        Survey = 1,            // مسح
        Construction = 2,      // بناء
        Services = 3,          // خدمات
        Legal = 4,             // قانوني
        Partnership = 5,       // شراكة
        Historical = 6,        // تاريخي
        Valuation = 7          // تقييم
    }

    /// <summary>صيغة الملف</summary>
    public enum FileFormat : byte
    {
        PDF = 0,
        JPG = 1,
        PNG = 2,
        TIFF = 3,
        DOC = 4
    }

    /// <summary>طريقة التحقق</summary>
    public enum VerificationMethod : byte
    {
        ManualReview = 0,      // مراجعة يدوية
        ElectronicCheck = 1,   // فحص إلكتروني
        FieldVisit = 2         // زيارة ميدانية
    }

    /// <summary>نوع الصورة</summary>
    public enum PhotoType : byte
    {
        FrontFacade = 0,       // واجهة أمامية
        Right = 1,             // جانب أيمن
        Left = 2,              // جانب أيسر
        Back = 3,              // خلفي
        Interior = 4,          // داخلي
        Document = 5,          // مستند
        Aerial = 6,            // جوي
        Panoramic = 7          // بانورامي
    }

    /// <summary>حالة مزامنة GIS</summary>
    public enum GisSyncStatus : byte
    {
        Pending = 0,           // قيد الانتظار
        Synced = 1,            // تمت المزامنة
        Failed = 2             // فشلت المزامنة
    }

    /// <summary>اتجاه المزامنة</summary>
    public enum GisSyncDirection : byte
    {
        ToGis = 0,             // إلى نظام GIS
        FromGis = 1            // من نظام GIS
    }
}

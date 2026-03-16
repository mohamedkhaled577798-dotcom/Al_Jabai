using AutoMapper;
using WaqfSystem.Application.DTOs.Mobile;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.Mappings
{
    public class PropertyMappingProfile : Profile
    {
        public PropertyMappingProfile()
        {
            // Property
            CreateMap<Property, PropertyListDto>()
                .ForMember(d => d.GovernorateName, o => o.MapFrom(s => s.Governorate != null ? s.Governorate.NameAr : null))
                .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy != null ? s.CreatedBy.FullNameAr : null))
                .ForMember(d => d.PropertyTypeDisplay, o => o.MapFrom(s => s.PropertyType.ToString())) // Arabic mapping handled in UI/Enums
                .ForMember(d => d.OwnershipTypeDisplay, o => o.MapFrom(s => s.OwnershipType.ToString()))
                .ForMember(d => d.ApprovalStageDisplay, o => o.MapFrom(s => s.ApprovalStage.ToString()));

            CreateMap<Property, PropertyDetailDto>()
                .ForMember(d => d.GovernorateName, o => o.MapFrom(s => s.Governorate != null ? s.Governorate.NameAr : null))
                .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy != null ? s.CreatedBy.FullNameAr : null));

            CreateMap<CreatePropertyDto, Property>();
            CreateMap<UpdatePropertyDto, Property>();

            // Address
            CreateMap<PropertyAddress, PropertyAddressDto>()
                .ForMember(d => d.NeighborhoodId, o => o.MapFrom(s => s.Street != null ? (int?)s.Street.NeighborhoodId : null))
                .ForMember(d => d.SubDistrictId, o => o.MapFrom(s => s.Street != null && s.Street.Neighborhood != null ? (int?)s.Street.Neighborhood.SubDistrictId : null))
                .ForMember(d => d.DistrictId, o => o.MapFrom(s => s.Street != null && s.Street.Neighborhood != null && s.Street.Neighborhood.SubDistrict != null ? (int?)s.Street.Neighborhood.SubDistrict.DistrictId : null))
                .ForMember(d => d.GovernorateId, o => o.MapFrom(s => s.Street != null && s.Street.Neighborhood != null && s.Street.Neighborhood.SubDistrict != null && s.Street.Neighborhood.SubDistrict.District != null ? (int?)s.Street.Neighborhood.SubDistrict.District.GovernorateId : null))
                .ForMember(d => d.StreetName, o => o.MapFrom(s => s.Street != null ? s.Street.NameAr : null))
                .ForMember(d => d.NeighborhoodName, o => o.MapFrom(s => s.Street != null && s.Street.Neighborhood != null ? s.Street.Neighborhood.NameAr : null))
                .ForMember(d => d.DistrictName, o => o.MapFrom(s => s.Street != null && s.Street.Neighborhood != null && s.Street.Neighborhood.SubDistrict != null && s.Street.Neighborhood.SubDistrict.District != null ? s.Street.Neighborhood.SubDistrict.District.NameAr : null))
                .ForMember(d => d.GovernorateName, o => o.MapFrom(s => s.Street != null && s.Street.Neighborhood != null && s.Street.Neighborhood.SubDistrict != null && s.Street.Neighborhood.SubDistrict.District != null && s.Street.Neighborhood.SubDistrict.District.Governorate != null ? s.Street.Neighborhood.SubDistrict.District.Governorate.NameAr : null));

            // Floors & Units
            CreateMap<PropertyFloor, FloorDto>();
            CreateMap<PropertyUnit, UnitDto>();
            CreateMap<PropertyRoom, RoomDto>();
            CreateMap<CreateFloorDto, PropertyFloor>();
            CreateMap<CreateUnitDto, PropertyUnit>();
            CreateMap<CreateRoomDto, PropertyRoom>();

            // Partnership
            CreateMap<PropertyPartnership, PropertyPartnershipDto>();
            CreateMap<PartnerRevenueDistribution, RevenuePeriodDto>();
            CreateMap<CreatePartnershipDto, PropertyPartnership>();

            // Agricultural
            CreateMap<AgriculturalDetail, AgriculturalDetailDto>();
            CreateMap<CreateAgriculturalDto, AgriculturalDetail>();

            // Documents & Photos
            CreateMap<PropertyDocument, DocumentDto>()
                .ForMember(d => d.DocumentCategory, o => o.MapFrom(s => ParseDocumentCategory(s.DocumentType != null ? s.DocumentType.Category : null)))
                .ForMember(d => d.DocumentType, o => o.MapFrom(s => s.DocumentType != null ? s.DocumentType.NameAr : null))
                .ForMember(d => d.DocumentDate, o => o.MapFrom(s => s.IssueDate))
                .ForMember(d => d.FileUrl, o => o.MapFrom(s => s.CurrentVersion != null ? s.CurrentVersion.FileUrl : string.Empty))
                .ForMember(d => d.FileFormat, o => o.MapFrom(s => ParseFileFormat(s.CurrentVersion != null ? s.CurrentVersion.FileExtension : null)))
                .ForMember(d => d.FileSizeKB, o => o.MapFrom(s => s.CurrentVersion != null ? (int?)(s.CurrentVersion.FileSizeBytes / 1024) : null))
                .ForMember(d => d.IsVerified, o => o.MapFrom(s => s.Status == DocumentStatus.Verified))
                .ForMember(d => d.VerifiedByName, o => o.MapFrom(s => s.VerifiedBy != null ? s.VerifiedBy.FullNameAr : null));
            CreateMap<UploadDocumentDto, PropertyDocument>();

            CreateMap<PropertyPhoto, PhotoDto>();

            // Workflow
            CreateMap<PropertyWorkflowHistory, WorkflowHistoryDto>()
                .ForMember(d => d.ActionByName, o => o.MapFrom(s => s.ActionBy != null ? s.ActionBy.FullNameAr : null));

            // Mobile Specific
            CreateMap<User, MobileUserDto>()
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.Code))
                .ForMember(d => d.GovernorateName, o => o.MapFrom(s => s.Governorate != null ? s.Governorate.NameAr : null));

            CreateMap<Property, PropertyMapPointDto>();
            
            // Geography for Mobile Initial Sync
            CreateMap<Governorate, GovernorateItem>();
            CreateMap<District, DistrictItem>();
            CreateMap<SubDistrict, SubDistrictItem>();
            CreateMap<Neighborhood, NeighborhoodItem>();
        }

        private static DocumentCategory ParseDocumentCategory(string? category)
        {
            if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<DocumentCategory>(category, true, out var parsed))
            {
                return parsed;
            }

            return DocumentCategory.Ownership;
        }

        private static FileFormat ParseFileFormat(string? extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) return FileFormat.PDF;

            return extension.Trim().TrimStart('.').ToLowerInvariant() switch
            {
                "jpg" => FileFormat.JPG,
                "jpeg" => FileFormat.JPG,
                "png" => FileFormat.PNG,
                "tif" => FileFormat.TIFF,
                "tiff" => FileFormat.TIFF,
                "doc" => FileFormat.DOC,
                "docx" => FileFormat.DOC,
                _ => FileFormat.PDF
            };
        }
    }
}

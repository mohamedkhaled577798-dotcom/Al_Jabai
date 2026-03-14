using AutoMapper;
using WaqfSystem.Application.DTOs.Mobile;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Core.Entities;

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
                .ForMember(d => d.StreetName, o => o.MapFrom(s => s.Street != null ? s.Street.NameAr : null))
                .ForMember(d => d.NeighborhoodName, o => o.MapFrom(s => s.Street != null && s.Street.Neighborhood != null ? s.Street.Neighborhood.NameAr : null))
                .ForMember(d => d.DistrictName, o => o.MapFrom(s => s.Street != null && s.Street.Neighborhood != null && s.Street.Neighborhood.SubDistrict != null && s.Street.Neighborhood.SubDistrict.District != null ? s.Street.Neighborhood.SubDistrict.District.NameAr : null));

            // Floors & Units
            CreateMap<PropertyFloor, FloorDto>();
            CreateMap<PropertyUnit, UnitDto>();
            CreateMap<PropertyRoom, RoomDto>();
            CreateMap<CreateFloorDto, PropertyFloor>();
            CreateMap<CreateUnitDto, PropertyUnit>();
            CreateMap<CreateRoomDto, PropertyRoom>();

            // Partnership
            CreateMap<PropertyPartnership, PropertyPartnershipDto>();
            CreateMap<CreatePartnershipDto, PropertyPartnership>();

            // Agricultural
            CreateMap<AgriculturalDetail, AgriculturalDetailDto>();
            CreateMap<CreateAgriculturalDto, AgriculturalDetail>();

            // Documents & Photos
            CreateMap<PropertyDocument, DocumentDto>()
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
    }
}

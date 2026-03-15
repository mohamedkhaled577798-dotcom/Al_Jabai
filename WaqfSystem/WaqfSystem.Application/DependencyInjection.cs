using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using FluentValidation;
using WaqfSystem.Application.Mappings;
using WaqfSystem.Application.Services;
using WaqfSystem.Application.Validators;

namespace WaqfSystem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // AutoMapper
            services.AddAutoMapper(typeof(PropertyMappingProfile).Assembly);

            // FluentValidation
            services.AddValidatorsFromAssembly(typeof(CreatePropertyValidator).Assembly);

            // Application Services
            services.AddScoped<IPropertyService, PropertyService>();
            services.AddScoped<IPartnershipService, PartnershipService>();
            services.AddScoped<IPartnerCommunicationService, PartnerCommunicationService>();
            services.AddScoped<IGeographicService, GeographicService>();
            services.AddScoped<IAgriculturalService, AgriculturalService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDqsService, DqsService>();
            services.AddScoped<IWorkflowService, WorkflowService>();
            services.AddScoped<IMissionService, MissionService>();
            services.AddScoped<IMobileSyncService, MobileSyncService>();

            services.AddValidatorsFromAssemblyContaining<CreatePartnershipValidator>();

            return services;
        }
    }
}

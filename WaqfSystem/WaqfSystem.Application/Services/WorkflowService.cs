using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public interface IWorkflowService
    {
        Task<bool> AdvanceStageAsync(WorkflowActionDto dto, int userId);
        Task<bool> RejectToCorrectionAsync(int propertyId, string reason, int userId);
        Task<List<WorkflowHistoryDto>> GetHistoryAsync(int propertyId);
        bool CanTransition(ApprovalStage from, ApprovalStage to, string userRole);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkflowService> _logger;
        private readonly IDqsService _dqsService;

        public WorkflowService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<WorkflowService> logger, IDqsService dqsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _dqsService = dqsService;
        }

        public async Task<bool> AdvanceStageAsync(WorkflowActionDto dto, int userId)
        {
            var property = await _unitOfWork.Properties.GetByIdWithDetailsAsync(dto.PropertyId);
            if (property == null) return false;

            var dqs = _dqsService.CalculateScore(property);

            // Minimum DQS to submit
            if (dto.ToStage != ApprovalStage.Draft && dto.ToStage != ApprovalStage.SentForCorrection && dqs < 50)
            {
                _logger.LogWarning("Property {Id} DQS {Dqs} is below minimum 50% for submission", dto.PropertyId, dqs);
                return false;
            }

            // Minimum DQS to approve
            if (dto.ToStage == ApprovalStage.Approved && dqs < 70)
            {
                _logger.LogWarning("Property {Id} DQS {Dqs} is below minimum 70% for approval", dto.PropertyId, dqs);
                return false;
            }

            var fromStage = property.ApprovalStage;

            // Record workflow history
            var history = new PropertyWorkflowHistory
            {
                PropertyId = dto.PropertyId,
                FromStage = fromStage,
                ToStage = dto.ToStage,
                ActionById = userId,
                ActionAt = DateTime.UtcNow,
                Notes = dto.Notes,
                DqsAtAction = dqs,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AddAsync(history);

            // Update property stage
            property.ApprovalStage = dto.ToStage;
            property.DqsScore = dqs;
            property.UpdatedById = userId;
            property.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Properties.UpdateAsync(property);

            // Write audit log
            var audit = new AuditLog
            {
                TableName = "Properties",
                RecordId = dto.PropertyId,
                Action = "WORKFLOW",
                OldValues = $"{{\"ApprovalStage\":\"{fromStage}\"}}",
                NewValues = $"{{\"ApprovalStage\":\"{dto.ToStage}\"}}",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AddAsync(audit);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Workflow: Property {Id} transitioned from {From} to {To} by user {UserId}",
                dto.PropertyId, fromStage, dto.ToStage, userId);

            return true;
        }

        public async Task<bool> RejectToCorrectionAsync(int propertyId, string reason, int userId)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                _logger.LogWarning("Rejection reason is mandatory for property {Id}", propertyId);
                return false;
            }

            return await AdvanceStageAsync(new WorkflowActionDto
            {
                PropertyId = propertyId,
                ToStage = ApprovalStage.SentForCorrection,
                Notes = reason
            }, userId);
        }

        public async Task<List<WorkflowHistoryDto>> GetHistoryAsync(int propertyId)
        {
            var history = await _unitOfWork.Properties.GetWorkflowHistoryAsync(propertyId);
            return _mapper.Map<List<WorkflowHistoryDto>>(history);
        }

        public bool CanTransition(ApprovalStage from, ApprovalStage to, string userRole)
        {
            // Define valid transitions per role
            var validTransitions = new Dictionary<(ApprovalStage, ApprovalStage), string[]>
            {
                { (ApprovalStage.Draft, ApprovalStage.FieldSupervisorReview), new[] { "FIELD_INSPECTOR", "FIELD_SUPERVISOR" } },
                { (ApprovalStage.FieldSupervisorReview, ApprovalStage.LegalReview), new[] { "FIELD_SUPERVISOR" } },
                { (ApprovalStage.LegalReview, ApprovalStage.EngineeringReview), new[] { "LEGAL_REVIEWER" } },
                { (ApprovalStage.EngineeringReview, ApprovalStage.RegionalApproval), new[] { "ENGINEER" } },
                { (ApprovalStage.RegionalApproval, ApprovalStage.Approved), new[] { "REGIONAL_MGR", "AUTH_DIRECTOR" } },
                { (ApprovalStage.SentForCorrection, ApprovalStage.FieldSupervisorReview), new[] { "FIELD_INSPECTOR", "FIELD_SUPERVISOR" } },
            };

            // SYS_ADMIN can do anything
            if (userRole == "SYS_ADMIN") return true;

            // Any stage can be sent for correction
            if (to == ApprovalStage.SentForCorrection)
                return true;

            if (validTransitions.TryGetValue((from, to), out var allowedRoles))
                return allowedRoles.Contains(userRole);

            return false;
        }
    }
}

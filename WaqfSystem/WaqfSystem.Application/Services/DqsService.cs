using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Application.Services
{
    public interface IDqsService
    {
        decimal CalculateScore(Property property);
        DqsScoreDto GetScoreBreakdown(Property property);
    }

    /// <summary>
    /// Data Quality Score (DQS) service — auto-recalculated on every property save.
    /// </summary>
    public class DqsService : IDqsService
    {
        public decimal CalculateScore(Property property)
        {
            var breakdown = GetScoreBreakdown(property);
            return breakdown.TotalScore;
        }

        public DqsScoreDto GetScoreBreakdown(Property property)
        {
            var criteria = new List<DqsCriterionDto>();
            decimal total = 0;

            // PropertyName present (5%)
            var hasName = !string.IsNullOrWhiteSpace(property.PropertyName);
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "PropertyName",
                CriterionNameAr = "اسم العقار",
                Weight = 5, Achieved = hasName, Score = hasName ? 5 : 0
            });
            if (hasName) total += 5;

            // WaqfType set (5%)
            var hasWaqfType = property.WaqfType.HasValue;
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "WaqfType",
                CriterionNameAr = "نوع الوقف",
                Weight = 5, Achieved = hasWaqfType, Score = hasWaqfType ? 5 : 0
            });
            if (hasWaqfType) total += 5;

            // PropertyType set (5%)
            var hasPropertyType = true; // PropertyType is required, always set
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "PropertyType",
                CriterionNameAr = "نوع العقار",
                Weight = 5, Achieved = hasPropertyType, Score = hasPropertyType ? 5 : 0
            });
            if (hasPropertyType) total += 5;

            // Location to SubDistrict (10%)
            var hasLocation = property.GovernorateId.HasValue;
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "LocationSubDistrict",
                CriterionNameAr = "الموقع إلى مستوى الناحية",
                Weight = 10, Achieved = hasLocation, Score = hasLocation ? 10 : 0
            });
            if (hasLocation) total += 10;

            // Location to Street (5%)
            var hasStreet = property.Address?.StreetId != null;
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "LocationStreet",
                CriterionNameAr = "الموقع إلى مستوى الشارع",
                Weight = 5, Achieved = hasStreet, Score = hasStreet ? 5 : 0
            });
            if (hasStreet) total += 5;

            // GPS coordinates accuracy ≤ 20m (15%)
            var hasGps = property.Latitude.HasValue && property.Longitude.HasValue &&
                        property.GpsAccuracyMeters.HasValue && property.GpsAccuracyMeters <= 20;
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "GpsAccuracy",
                CriterionNameAr = "دقة الإحداثيات ≤ 20م",
                Weight = 15, Achieved = hasGps, Score = hasGps ? 15 : 0
            });
            if (hasGps) total += 15;

            // Polygon boundary set (10%)
            var hasPolygon = !string.IsNullOrWhiteSpace(property.GisPolygon);
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "PolygonBoundary",
                CriterionNameAr = "حدود المضلع",
                Weight = 10, Achieved = hasPolygon, Score = hasPolygon ? 10 : 0
            });
            if (hasPolygon) total += 10;

            // DeedNumber + document uploaded (15%)
            var hasDeed = !string.IsNullOrWhiteSpace(property.DeedNumber) &&
                          property.Documents?.Any(d => d.DocumentType != null && d.DocumentType.Code == "OWNERSHIP_DEED") == true;
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "DeedDocument",
                CriterionNameAr = "رقم السند + مستند مرفوع",
                Weight = 15, Achieved = hasDeed, Score = hasDeed ? 15 : 0
            });
            if (hasDeed) total += 15;

            // Cadastral or Tabu number (5%)
            var hasCadastral = !string.IsNullOrWhiteSpace(property.CadastralNumber) || !string.IsNullOrWhiteSpace(property.TabuNumber);
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "CadastralNumber",
                CriterionNameAr = "رقم العقار أو الطابو",
                Weight = 5, Achieved = hasCadastral, Score = hasCadastral ? 5 : 0
            });
            if (hasCadastral) total += 5;

            // StructuralCondition set (5%)
            var hasCondition = property.StructuralCondition.HasValue;
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "StructuralCondition",
                CriterionNameAr = "الحالة الإنشائية",
                Weight = 5, Achieved = hasCondition, Score = hasCondition ? 5 : 0
            });
            if (hasCondition) total += 5;

            // Photos ≥ 4 (10%)
            var hasPhotos = property.Photos?.Count >= 4;
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "PhotoCount",
                CriterionNameAr = "صور ≥ 4",
                Weight = 10, Achieved = hasPhotos, Score = hasPhotos ? 10 : 0
            });
            if (hasPhotos) total += 10;

            // EstimatedValue set (5%)
            var hasValue = property.EstimatedValue.HasValue && property.EstimatedValue > 0;
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "EstimatedValue",
                CriterionNameAr = "القيمة التقديرية",
                Weight = 5, Achieved = hasValue, Score = hasValue ? 5 : 0
            });
            if (hasValue) total += 5;

            // AreaSqm set (5%)
            var hasArea = property.TotalAreaSqm.HasValue && property.TotalAreaSqm > 0;
            criteria.Add(new DqsCriterionDto
            {
                CriterionName = "AreaSqm",
                CriterionNameAr = "المساحة الكلية",
                Weight = 5, Achieved = hasArea, Score = hasArea ? 5 : 0
            });
            if (hasArea) total += 5;

            return new DqsScoreDto
            {
                PropertyId = property.Id,
                TotalScore = total,
                Criteria = criteria,
                CanSubmit = total >= 50,  // minimum to submit: 50%
                CanApprove = total >= 70  // minimum for approval: 70%
            };
        }
    }
}

import 'package:flutter/material.dart';

enum SuggestionType { overdue, dueToday, dueSoon, unpaidLastMonth }
enum AlertLevel { none, warning, critical }
enum ChipType { full, half, withPenalty, custom }

SuggestionType _suggestionTypeFrom(dynamic value) {
  if (value is int) {
    switch (value) {
      case 1:
        return SuggestionType.overdue;
      case 2:
        return SuggestionType.dueToday;
      case 3:
        return SuggestionType.dueSoon;
      case 4:
        return SuggestionType.unpaidLastMonth;
    }
  }

  final text = (value ?? '').toString().toLowerCase();
  return SuggestionType.values.firstWhere((e) => e.name.toLowerCase() == text, orElse: () => SuggestionType.dueSoon);
}

AlertLevel _alertLevelFrom(dynamic value) {
  if (value is int) {
    switch (value) {
      case 1:
        return AlertLevel.warning;
      case 2:
        return AlertLevel.critical;
      default:
        return AlertLevel.none;
    }
  }

  final text = (value ?? '').toString().toLowerCase();
  return AlertLevel.values.firstWhere((e) => e.name.toLowerCase() == text, orElse: () => AlertLevel.none);
}

ChipType _chipTypeFrom(dynamic value) {
  if (value is int) {
    switch (value) {
      case 1:
        return ChipType.full;
      case 2:
        return ChipType.half;
      case 3:
        return ChipType.withPenalty;
      case 4:
        return ChipType.custom;
    }
  }

  final text = (value ?? '').toString().toLowerCase();
  return ChipType.values.firstWhere((e) => e.name.toLowerCase() == text, orElse: () => ChipType.full);
}

class AmountChipDto {
  final String label;
  final double amount;
  final bool isDefault;
  final ChipType chipType;

  const AmountChipDto({required this.label, required this.amount, required this.isDefault, required this.chipType});

  factory AmountChipDto.fromJson(Map<String, dynamic> json) => AmountChipDto(
        label: (json['label'] as String?) ?? '',
        amount: (json['amount'] as num?)?.toDouble() ?? 0,
        isDefault: (json['isDefault'] as bool?) ?? false,
        chipType: _chipTypeFrom(json['chipType']),
      );

  Map<String, dynamic> toJson() => {'label': label, 'amount': amount, 'isDefault': isDefault, 'chipType': chipType.name};
}

class SmartSuggestionDto {
  final String suggestionId;
  final SuggestionType suggestionType;
  final int priority;
  final int propertyId;
  final String propertyNameAr;
  final int? floorId;
  final String? floorLabel;
  final int? unitId;
  final String? unitNumber;
  final String? unitType;
  final String collectionLevel;
  final String? tenantNameAr;
  final String? tenantPhone;
  final int? contractId;
  final double expectedAmount;
  final String contractType;
  final DateTime? dueDate;
  final int overdueDays;
  final String periodLabel;
  final List<AmountChipDto> suggestedAmountChips;
  final bool isLocked;
  final String? lockReason;
  final String urgencyColorHex;

  const SmartSuggestionDto({
    required this.suggestionId,
    required this.suggestionType,
    required this.priority,
    required this.propertyId,
    required this.propertyNameAr,
    required this.floorId,
    required this.floorLabel,
    required this.unitId,
    required this.unitNumber,
    required this.unitType,
    required this.collectionLevel,
    required this.tenantNameAr,
    required this.tenantPhone,
    required this.contractId,
    required this.expectedAmount,
    required this.contractType,
    required this.dueDate,
    required this.overdueDays,
    required this.periodLabel,
    required this.suggestedAmountChips,
    required this.isLocked,
    required this.lockReason,
    required this.urgencyColorHex,
  });

  Color get urgencyColor {
    switch (suggestionType) {
      case SuggestionType.overdue:
        return const Color(0xFFB3261E);
      case SuggestionType.dueToday:
        return const Color(0xFFC58A08);
      case SuggestionType.dueSoon:
        return const Color(0xFF1C6FA9);
      case SuggestionType.unpaidLastMonth:
        return const Color(0xFF6D5E2E);
    }
  }

  String get urgencyLabelAr {
    switch (suggestionType) {
      case SuggestionType.overdue:
        return 'متأخر';
      case SuggestionType.dueToday:
        return 'مستحق اليوم';
      case SuggestionType.dueSoon:
        return 'مستحق قريباً';
      case SuggestionType.unpaidLastMonth:
        return 'غير محصل الشهر الماضي';
    }
  }

  bool get isActionable => !isLocked;

  factory SmartSuggestionDto.fromJson(Map<String, dynamic> json) => SmartSuggestionDto(
        suggestionId: (json['suggestionId'] as String?) ?? '',
        suggestionType: _suggestionTypeFrom(json['suggestionType']),
        priority: (json['priority'] as num?)?.toInt() ?? 0,
        propertyId: (json['propertyId'] as num?)?.toInt() ?? 0,
        propertyNameAr: (json['propertyNameAr'] as String?) ?? '',
        floorId: (json['floorId'] as num?)?.toInt(),
        floorLabel: json['floorLabel'] as String?,
        unitId: (json['unitId'] as num?)?.toInt(),
        unitNumber: json['unitNumber'] as String?,
        unitType: json['unitType'] as String?,
        collectionLevel: (json['collectionLevel'] as String?) ?? 'Unit',
        tenantNameAr: json['tenantNameAr'] as String?,
        tenantPhone: json['tenantPhone'] as String?,
        contractId: (json['contractId'] as num?)?.toInt(),
        expectedAmount: (json['expectedAmount'] as num?)?.toDouble() ?? 0,
        contractType: (json['contractType'] as String?) ?? '',
        dueDate: DateTime.tryParse((json['dueDate'] as String?) ?? ''),
        overdueDays: (json['overdueDays'] as num?)?.toInt() ?? 0,
        periodLabel: (json['periodLabel'] as String?) ?? '',
        suggestedAmountChips: ((json['suggestedAmountChips'] as List?) ?? const <dynamic>[])
            .map((e) => AmountChipDto.fromJson((e as Map).cast<String, dynamic>()))
            .toList(),
        isLocked: (json['isLocked'] as bool?) ?? false,
        lockReason: json['lockReason'] as String?,
        urgencyColorHex: (json['urgencyColor'] as String?) ?? '#1C6FA9',
      );

  Map<String, dynamic> toJson() => {
        'suggestionId': suggestionId,
        'suggestionType': suggestionType.name,
        'priority': priority,
        'propertyId': propertyId,
        'propertyNameAr': propertyNameAr,
        'floorId': floorId,
        'floorLabel': floorLabel,
        'unitId': unitId,
        'unitNumber': unitNumber,
        'unitType': unitType,
        'collectionLevel': collectionLevel,
        'tenantNameAr': tenantNameAr,
        'tenantPhone': tenantPhone,
        'contractId': contractId,
        'expectedAmount': expectedAmount,
        'contractType': contractType,
        'dueDate': dueDate?.toIso8601String(),
        'overdueDays': overdueDays,
        'periodLabel': periodLabel,
        'suggestedAmountChips': suggestedAmountChips.map((e) => e.toJson()).toList(),
        'isLocked': isLocked,
        'lockReason': lockReason,
        'urgencyColor': urgencyColorHex,
      };

  SmartSuggestionDto copyWith({double? expectedAmount, bool? isLocked}) => SmartSuggestionDto(
        suggestionId: suggestionId,
        suggestionType: suggestionType,
        priority: priority,
        propertyId: propertyId,
        propertyNameAr: propertyNameAr,
        floorId: floorId,
        floorLabel: floorLabel,
        unitId: unitId,
        unitNumber: unitNumber,
        unitType: unitType,
        collectionLevel: collectionLevel,
        tenantNameAr: tenantNameAr,
        tenantPhone: tenantPhone,
        contractId: contractId,
        expectedAmount: expectedAmount ?? this.expectedAmount,
        contractType: contractType,
        dueDate: dueDate,
        overdueDays: overdueDays,
        periodLabel: periodLabel,
        suggestedAmountChips: suggestedAmountChips,
        isLocked: isLocked ?? this.isLocked,
        lockReason: lockReason,
        urgencyColorHex: urgencyColorHex,
      );
}

class QuickCollectRequest {
  final int propertyId;
  final String collectionLevel;
  final int? floorId;
  final int? unitId;
  final int? contractId;
  final String periodLabel;
  final DateTime periodStartDate;
  final DateTime periodEndDate;
  final double amount;
  final double? expectedAmount;
  final DateTime collectionDate;
  final String? paymentMethod;
  final String? receiptNumber;
  final String? payerNameAr;
  final String? notes;
  final String? suggestionId;
  final String? varianceApprovalNote;

  const QuickCollectRequest({
    required this.propertyId,
    required this.collectionLevel,
    required this.floorId,
    required this.unitId,
    required this.contractId,
    required this.periodLabel,
    required this.periodStartDate,
    required this.periodEndDate,
    required this.amount,
    required this.expectedAmount,
    required this.collectionDate,
    required this.paymentMethod,
    required this.receiptNumber,
    required this.payerNameAr,
    required this.notes,
    required this.suggestionId,
    required this.varianceApprovalNote,
  });

  Map<String, dynamic> toJson() => {
        'propertyId': propertyId,
        'collectionLevel': collectionLevel,
        'floorId': floorId,
        'unitId': unitId,
        'contractId': contractId,
        'periodLabel': periodLabel,
        'periodStartDate': periodStartDate.toIso8601String(),
        'periodEndDate': periodEndDate.toIso8601String(),
        'amount': amount,
        'expectedAmount': expectedAmount,
        'collectionDate': collectionDate.toIso8601String(),
        'paymentMethod': paymentMethod,
        'receiptNumber': receiptNumber,
        'payerNameAr': payerNameAr,
        'notes': notes,
        'suggestionId': suggestionId,
        'varianceApprovalNote': varianceApprovalNote,
      };
}

class BatchCollectItemRequest {
  final int propertyId;
  final String collectionLevel;
  final int? floorId;
  final int? unitId;
  final int? contractId;
  final double amount;
  final double? expectedAmount;
  final String? payerNameAr;
  final String? receiptNumber;

  const BatchCollectItemRequest({
    required this.propertyId,
    required this.collectionLevel,
    required this.floorId,
    required this.unitId,
    required this.contractId,
    required this.amount,
    required this.expectedAmount,
    required this.payerNameAr,
    required this.receiptNumber,
  });

  Map<String, dynamic> toJson() => {
        'propertyId': propertyId,
        'collectionLevel': collectionLevel,
        'floorId': floorId,
        'unitId': unitId,
        'contractId': contractId,
        'amount': amount,
        'expectedAmount': expectedAmount,
        'payerNameAr': payerNameAr,
        'receiptNumber': receiptNumber,
      };
}

class BatchCollectRequest {
  final String periodLabel;
  final DateTime collectionDate;
  final String? paymentMethod;
  final List<BatchCollectItemRequest> items;

  const BatchCollectRequest({required this.periodLabel, required this.collectionDate, required this.paymentMethod, required this.items});

  Map<String, dynamic> toJson() => {
        'periodLabel': periodLabel,
        'collectionDate': collectionDate.toIso8601String(),
        'paymentMethod': paymentMethod,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class BatchCollectItemResult {
  final String unitLabel;
  final double amount;
  final bool success;
  final String? revenueCode;
  final String? error;

  const BatchCollectItemResult({required this.unitLabel, required this.amount, required this.success, required this.revenueCode, required this.error});

  factory BatchCollectItemResult.fromJson(Map<String, dynamic> json) => BatchCollectItemResult(
        unitLabel: (json['unitLabel'] as String?) ?? '',
        amount: (json['amount'] as num?)?.toDouble() ?? 0,
        success: (json['success'] as bool?) ?? false,
        revenueCode: json['revenueCode'] as String?,
        error: json['error'] as String?,
      );
}

class BatchCollectResult {
  final String batchCode;
  final int successCount;
  final int failedCount;
  final double totalAmount;
  final List<BatchCollectItemResult> results;

  const BatchCollectResult({required this.batchCode, required this.successCount, required this.failedCount, required this.totalAmount, required this.results});

  factory BatchCollectResult.fromJson(Map<String, dynamic> json) => BatchCollectResult(
        batchCode: (json['batchCode'] as String?) ?? '',
        successCount: (json['successCount'] as num?)?.toInt() ?? 0,
        failedCount: (json['failedCount'] as num?)?.toInt() ?? 0,
        totalAmount: (json['totalAmount'] as num?)?.toDouble() ?? 0,
        results: ((json['results'] as List?) ?? const <dynamic>[])
            .map((e) => BatchCollectItemResult.fromJson((e as Map).cast<String, dynamic>()))
            .toList(),
      );
}

class VarianceAlertDto {
  final double expectedAmount;
  final double actualAmount;
  final double variance;
  final double variancePercent;
  final AlertLevel alertLevel;
  final String alertMessage;
  final bool requiresApproval;

  const VarianceAlertDto({
    required this.expectedAmount,
    required this.actualAmount,
    required this.variance,
    required this.variancePercent,
    required this.alertLevel,
    required this.alertMessage,
    required this.requiresApproval,
  });

  bool get requiresNote => alertLevel == AlertLevel.critical;

  Color get alertColor {
    switch (alertLevel) {
      case AlertLevel.none:
        return const Color(0xFF2E7D32);
      case AlertLevel.warning:
        return const Color(0xFFBA7517);
      case AlertLevel.critical:
        return const Color(0xFFB3261E);
    }
  }

  factory VarianceAlertDto.fromJson(Map<String, dynamic> json) => VarianceAlertDto(
        expectedAmount: (json['expectedAmount'] as num?)?.toDouble() ?? 0,
        actualAmount: (json['actualAmount'] as num?)?.toDouble() ?? 0,
        variance: (json['variance'] as num?)?.toDouble() ?? 0,
        variancePercent: (json['variancePercent'] as num?)?.toDouble() ?? 0,
        alertLevel: _alertLevelFrom(json['alertLevel']),
        alertMessage: (json['alertMessage'] as String?) ?? '',
        requiresApproval: (json['requiresApproval'] as bool?) ?? false,
      );

  Map<String, dynamic> toJson() => {
        'expectedAmount': expectedAmount,
        'actualAmount': actualAmount,
        'variance': variance,
        'variancePercent': variancePercent,
        'alertLevel': alertLevel.name,
        'alertMessage': alertMessage,
        'requiresApproval': requiresApproval,
      };
}

class DayProgressDto {
  final DateTime date;
  final double collected;
  final double expected;
  final bool isToday;

  const DayProgressDto({required this.date, required this.collected, required this.expected, required this.isToday});

  factory DayProgressDto.fromJson(Map<String, dynamic> json) => DayProgressDto(
        date: DateTime.tryParse((json['date'] as String?) ?? '') ?? DateTime.now(),
        collected: (json['collected'] as num?)?.toDouble() ?? 0,
        expected: (json['expected'] as num?)?.toDouble() ?? 0,
        isToday: (json['isToday'] as bool?) ?? false,
      );

  Map<String, dynamic> toJson() => {'date': date.toIso8601String(), 'collected': collected, 'expected': expected, 'isToday': isToday};
}

class FloorCollectionDto {
  final String floorLabel;
  final double collected;
  final double expected;
  final double rate;
  final int uncollectedUnits;

  const FloorCollectionDto({required this.floorLabel, required this.collected, required this.expected, required this.rate, required this.uncollectedUnits});

  factory FloorCollectionDto.fromJson(Map<String, dynamic> json) => FloorCollectionDto(
        floorLabel: (json['floorLabel'] as String?) ?? '',
        collected: (json['collected'] as num?)?.toDouble() ?? 0,
        expected: (json['expected'] as num?)?.toDouble() ?? 0,
        rate: (json['rate'] as num?)?.toDouble() ?? 0,
        uncollectedUnits: (json['uncollectedUnits'] as num?)?.toInt() ?? 0,
      );

  Map<String, dynamic> toJson() => {
        'floorLabel': floorLabel,
        'collected': collected,
        'expected': expected,
        'rate': rate,
        'uncollectedUnits': uncollectedUnits,
      };
}

class TodayDashboardDto {
  final double collectedToday;
  final int collectedTodayCount;
  final double monthCollected;
  final double monthExpected;
  final double monthCollectionRate;
  final int overdueCount;
  final double overdueAmount;
  final int pendingCount;
  final List<SmartSuggestionDto> smartSuggestions;
  final List<DayProgressDto> monthProgress;
  final List<FloorCollectionDto> collectionByFloor;

  const TodayDashboardDto({
    required this.collectedToday,
    required this.collectedTodayCount,
    required this.monthCollected,
    required this.monthExpected,
    required this.monthCollectionRate,
    required this.overdueCount,
    required this.overdueAmount,
    required this.pendingCount,
    required this.smartSuggestions,
    required this.monthProgress,
    required this.collectionByFloor,
  });

  factory TodayDashboardDto.fromJson(Map<String, dynamic> json) => TodayDashboardDto(
        collectedToday: (json['collectedToday'] as num?)?.toDouble() ?? 0,
        collectedTodayCount: (json['collectedTodayCount'] as num?)?.toInt() ?? 0,
        monthCollected: (json['monthCollected'] as num?)?.toDouble() ?? 0,
        monthExpected: (json['monthExpected'] as num?)?.toDouble() ?? 0,
        monthCollectionRate: (json['monthCollectionRate'] as num?)?.toDouble() ?? 0,
        overdueCount: (json['overdueCount'] as num?)?.toInt() ?? 0,
        overdueAmount: (json['overdueAmount'] as num?)?.toDouble() ?? 0,
        pendingCount: (json['pendingCount'] as num?)?.toInt() ?? 0,
        smartSuggestions: ((json['smartSuggestions'] as List?) ?? const <dynamic>[])
            .map((e) => SmartSuggestionDto.fromJson((e as Map).cast<String, dynamic>()))
            .toList(),
        monthProgress: ((json['monthProgress'] as List?) ?? const <dynamic>[])
            .map((e) => DayProgressDto.fromJson((e as Map).cast<String, dynamic>()))
            .toList(),
        collectionByFloor: ((json['collectionByFloor'] as List?) ?? const <dynamic>[])
            .map((e) => FloorCollectionDto.fromJson((e as Map).cast<String, dynamic>()))
            .toList(),
      );

  Map<String, dynamic> toJson() => {
        'collectedToday': collectedToday,
        'collectedTodayCount': collectedTodayCount,
        'monthCollected': monthCollected,
        'monthExpected': monthExpected,
        'monthCollectionRate': monthCollectionRate,
        'overdueCount': overdueCount,
        'overdueAmount': overdueAmount,
        'pendingCount': pendingCount,
        'smartSuggestions': smartSuggestions.map((e) => e.toJson()).toList(),
        'monthProgress': monthProgress.map((e) => e.toJson()).toList(),
        'collectionByFloor': collectionByFloor.map((e) => e.toJson()).toList(),
      };
}

class SearchResultDto {
  final int unitId;
  final String unitNumber;
  final String unitType;
  final String floorLabel;
  final String propertyNameAr;
  final String wqfNumber;
  final String? tenantNameAr;
  final String? tenantPhone;
  final double activeContractRent;
  final String collectionStatusThisMonth;
  final int overdueDays;
  final DateTime? lastCollectionDate;
  final String collectionLevel;
  final bool isLocked;

  const SearchResultDto({
    required this.unitId,
    required this.unitNumber,
    required this.unitType,
    required this.floorLabel,
    required this.propertyNameAr,
    required this.wqfNumber,
    required this.tenantNameAr,
    required this.tenantPhone,
    required this.activeContractRent,
    required this.collectionStatusThisMonth,
    required this.overdueDays,
    required this.lastCollectionDate,
    required this.collectionLevel,
    required this.isLocked,
  });

  String get statusDisplayAr => collectionStatusThisMonth;

  Color get statusColor {
    switch (collectionStatusThisMonth) {
      case 'محصل':
        return const Color(0xFF2E7D32);
      case 'متأخر':
        return const Color(0xFFB3261E);
      case 'مقفول':
        return const Color(0xFF6B7280);
      case 'شاغرة':
        return const Color(0xFF9CA3AF);
      default:
        return const Color(0xFFBA7517);
    }
  }

  factory SearchResultDto.fromJson(Map<String, dynamic> json) => SearchResultDto(
        unitId: (json['unitId'] as num?)?.toInt() ?? 0,
        unitNumber: (json['unitNumber'] as String?) ?? '',
        unitType: (json['unitType'] as String?) ?? '',
        floorLabel: (json['floorLabel'] as String?) ?? '',
        propertyNameAr: (json['propertyNameAr'] as String?) ?? '',
        wqfNumber: (json['wqfNumber'] as String?) ?? '',
        tenantNameAr: json['tenantNameAr'] as String?,
        tenantPhone: json['tenantPhone'] as String?,
        activeContractRent: (json['activeContractRent'] as num?)?.toDouble() ?? 0,
        collectionStatusThisMonth: (json['collectionStatusThisMonth'] as String?) ?? '',
        overdueDays: (json['overdueDays'] as num?)?.toInt() ?? 0,
        lastCollectionDate: DateTime.tryParse((json['lastCollectionDate'] as String?) ?? ''),
        collectionLevel: (json['collectionLevel'] as String?) ?? 'Unit',
        isLocked: (json['isLocked'] as bool?) ?? false,
      );

  Map<String, dynamic> toJson() => {
        'unitId': unitId,
        'unitNumber': unitNumber,
        'unitType': unitType,
        'floorLabel': floorLabel,
        'propertyNameAr': propertyNameAr,
        'wqfNumber': wqfNumber,
        'tenantNameAr': tenantNameAr,
        'tenantPhone': tenantPhone,
        'activeContractRent': activeContractRent,
        'collectionStatusThisMonth': collectionStatusThisMonth,
        'overdueDays': overdueDays,
        'lastCollectionDate': lastCollectionDate?.toIso8601String(),
        'collectionLevel': collectionLevel,
        'isLocked': isLocked,
      };
}

class PagedResult<T> {
  final List<T> items;
  final int totalCount;
  final int page;
  final int pageSize;

  const PagedResult({required this.items, required this.totalCount, required this.page, required this.pageSize});
}

class PropertyStructureDto {
  final int propertyId;
  final String propertyNameAr;

  const PropertyStructureDto({required this.propertyId, required this.propertyNameAr});

  factory PropertyStructureDto.fromJson(Map<String, dynamic> json) => PropertyStructureDto(
        propertyId: (json['propertyId'] as num?)?.toInt() ?? 0,
        propertyNameAr: (json['propertyNameAr'] as String?) ?? '',
      );
}

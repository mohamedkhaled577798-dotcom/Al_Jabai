import 'dart:convert';

enum PartnershipType {
  revenuePercent,
  floorOwnership,
  unitOwnership,
  usufructRight,
  landPercent,
  timedPartnership,
  harvestShare,
}

enum PartnerType { individual, company, heirs, government, foundation, other }
enum RevenueDistribMethod { monthly, quarterly, annual, perCollection }
enum TransferStatus { pending, transferred, cancelled }
enum ContactType { sms, whatsApp, email, phone, meeting, letter, pdf }

extension PartnershipTypeX on PartnershipType {
  String get displayAr => switch (this) {
        PartnershipType.revenuePercent => 'نسبة من الإيراد',
        PartnershipType.floorOwnership => 'ملكية طوابق',
        PartnershipType.unitOwnership => 'ملكية عينية',
        PartnershipType.usufructRight => 'حق انتفاع',
        PartnershipType.landPercent => 'نسبة من الأرض',
        PartnershipType.timedPartnership => 'شراكة مؤقتة',
        PartnershipType.harvestShare => 'مزارعة/مساقاة',
      };

  bool get hasEndDate =>
      this == PartnershipType.timedPartnership ||
      this == PartnershipType.usufructRight;

  bool get isAgriculturalOnly =>
      this == PartnershipType.landPercent || this == PartnershipType.harvestShare;
}

class PartnershipDetail {
  final int id;
  final int propertyId;
  final String propertyNameAr;
  final String propertyWqfNumber;
  final String partnerName;
  final String? partnerPhone;
  final String? partnerEmail;
  final PartnershipType partnershipType;
  final PartnerType partnerType;
  final double waqfSharePercent;
  final String? ownedFloorNumbers;
  final String? ownedUnitIds;
  final DateTime? partnershipEndDate;
  final DateTime? usufructEndDate;
  final bool isActive;

  PartnershipDetail({
    required this.id,
    required this.propertyId,
    required this.propertyNameAr,
    required this.propertyWqfNumber,
    required this.partnerName,
    required this.partnershipType,
    required this.partnerType,
    required this.waqfSharePercent,
    this.partnerPhone,
    this.partnerEmail,
    this.ownedFloorNumbers,
    this.ownedUnitIds,
    this.partnershipEndDate,
    this.usufructEndDate,
    required this.isActive,
  });

  List<int> get ownedFloorsList {
    if (ownedFloorNumbers == null || ownedFloorNumbers!.isEmpty) return <int>[];
    try {
      final List<dynamic> parsed = jsonDecode(ownedFloorNumbers!);
      return parsed.map((e) => (e as num).toInt()).toList();
    } catch (_) {
      return <int>[];
    }
  }

  List<int> get ownedUnitsList {
    if (ownedUnitIds == null || ownedUnitIds!.isEmpty) return <int>[];
    try {
      final List<dynamic> parsed = jsonDecode(ownedUnitIds!);
      return parsed.map((e) => (e as num).toInt()).toList();
    } catch (_) {
      return <int>[];
    }
  }

  int? get daysUntilExpiry {
    final DateTime? end = partnershipEndDate ?? usufructEndDate;
    if (end == null) return null;
    return end.difference(DateTime.now()).inDays;
  }

  bool get isExpiringSoon => daysUntilExpiry != null && daysUntilExpiry! < 90;
  bool get isExpired => daysUntilExpiry != null && daysUntilExpiry! < 0;

  String get partnerShareDisplay => '${(100 - waqfSharePercent).toStringAsFixed(1)}%';

  factory PartnershipDetail.fromJson(Map<String, dynamic> json) {
    return PartnershipDetail(
      id: (json['id'] as num?)?.toInt() ?? 0,
      propertyId: (json['propertyId'] as num?)?.toInt() ?? 0,
      propertyNameAr: (json['propertyNameAr'] as String?) ?? '',
      propertyWqfNumber: (json['propertyWqfNumber'] as String?) ?? '',
      partnerName: (json['partnerName'] as String?) ?? '',
      partnerPhone: json['partnerPhone'] as String?,
      partnerEmail: json['partnerEmail'] as String?,
      partnershipType: _partnershipTypeFromString((json['partnershipType'] as String?) ?? 'revenuePercent'),
      partnerType: _partnerTypeFromString((json['partnerType'] as String?) ?? 'individual'),
      waqfSharePercent: (json['waqfSharePercent'] as num?)?.toDouble() ?? 0,
      ownedFloorNumbers: json['ownedFloorNumbers'] as String?,
      ownedUnitIds: json['ownedUnitIds'] as String?,
      partnershipEndDate: json['partnershipEndDate'] != null ? DateTime.parse(json['partnershipEndDate']) : null,
      usufructEndDate: json['usufructEndDate'] != null ? DateTime.parse(json['usufructEndDate']) : null,
      isActive: (json['isActive'] as bool?) ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'propertyId': propertyId,
      'propertyNameAr': propertyNameAr,
      'propertyWqfNumber': propertyWqfNumber,
      'partnerName': partnerName,
      'partnerPhone': partnerPhone,
      'partnerEmail': partnerEmail,
      'partnershipType': partnershipType.name,
      'partnerType': partnerType.name,
      'waqfSharePercent': waqfSharePercent,
      'ownedFloorNumbers': ownedFloorNumbers,
      'ownedUnitIds': ownedUnitIds,
      'partnershipEndDate': partnershipEndDate?.toIso8601String(),
      'usufructEndDate': usufructEndDate?.toIso8601String(),
      'isActive': isActive,
    };
  }

  PartnershipDetail copyWith({
    int? id,
    int? propertyId,
    String? propertyNameAr,
    String? propertyWqfNumber,
    String? partnerName,
    String? partnerPhone,
    String? partnerEmail,
    PartnershipType? partnershipType,
    PartnerType? partnerType,
    double? waqfSharePercent,
    String? ownedFloorNumbers,
    String? ownedUnitIds,
    DateTime? partnershipEndDate,
    DateTime? usufructEndDate,
    bool? isActive,
  }) {
    return PartnershipDetail(
      id: id ?? this.id,
      propertyId: propertyId ?? this.propertyId,
      propertyNameAr: propertyNameAr ?? this.propertyNameAr,
      propertyWqfNumber: propertyWqfNumber ?? this.propertyWqfNumber,
      partnerName: partnerName ?? this.partnerName,
      partnerPhone: partnerPhone ?? this.partnerPhone,
      partnerEmail: partnerEmail ?? this.partnerEmail,
      partnershipType: partnershipType ?? this.partnershipType,
      partnerType: partnerType ?? this.partnerType,
      waqfSharePercent: waqfSharePercent ?? this.waqfSharePercent,
      ownedFloorNumbers: ownedFloorNumbers ?? this.ownedFloorNumbers,
      ownedUnitIds: ownedUnitIds ?? this.ownedUnitIds,
      partnershipEndDate: partnershipEndDate ?? this.partnershipEndDate,
      usufructEndDate: usufructEndDate ?? this.usufructEndDate,
      isActive: isActive ?? this.isActive,
    );
  }
}

class RevenueDistribution {
  final int id;
  final int partnershipId;
  final String periodLabel;
  final double totalRevenue;
  final double waqfAmount;
  final double partnerAmount;
  final TransferStatus transferStatus;

  RevenueDistribution({
    required this.id,
    required this.partnershipId,
    required this.periodLabel,
    required this.totalRevenue,
    required this.waqfAmount,
    required this.partnerAmount,
    required this.transferStatus,
  });

  factory RevenueDistribution.fromJson(Map<String, dynamic> json) {
    return RevenueDistribution(
      id: (json['id'] as num?)?.toInt() ?? 0,
      partnershipId: (json['partnershipId'] as num?)?.toInt() ?? 0,
      periodLabel: (json['periodLabel'] as String?) ?? '',
      totalRevenue: (json['totalRevenue'] as num?)?.toDouble() ?? 0,
      waqfAmount: (json['waqfAmount'] as num?)?.toDouble() ?? 0,
      partnerAmount: (json['partnerAmount'] as num?)?.toDouble() ?? 0,
      transferStatus: _transferStatusFromString((json['transferStatus'] as String?) ?? 'pending'),
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'partnershipId': partnershipId,
        'periodLabel': periodLabel,
        'totalRevenue': totalRevenue,
        'waqfAmount': waqfAmount,
        'partnerAmount': partnerAmount,
        'transferStatus': transferStatus.name,
      };

  RevenueDistribution copyWith({
    int? id,
    int? partnershipId,
    String? periodLabel,
    double? totalRevenue,
    double? waqfAmount,
    double? partnerAmount,
    TransferStatus? transferStatus,
  }) {
    return RevenueDistribution(
      id: id ?? this.id,
      partnershipId: partnershipId ?? this.partnershipId,
      periodLabel: periodLabel ?? this.periodLabel,
      totalRevenue: totalRevenue ?? this.totalRevenue,
      waqfAmount: waqfAmount ?? this.waqfAmount,
      partnerAmount: partnerAmount ?? this.partnerAmount,
      transferStatus: transferStatus ?? this.transferStatus,
    );
  }
}

class RevenueCalculationResult {
  final double waqfAmount;
  final double partnerAmount;
  final double totalRevenue;
  final double waqfPercent;
  final String calculationMethod;
  final String calculationDetail;

  RevenueCalculationResult({
    required this.waqfAmount,
    required this.partnerAmount,
    required this.totalRevenue,
    required this.waqfPercent,
    required this.calculationMethod,
    required this.calculationDetail,
  });

  factory RevenueCalculationResult.fromJson(Map<String, dynamic> json) {
    return RevenueCalculationResult(
      waqfAmount: (json['waqfAmount'] as num?)?.toDouble() ?? 0,
      partnerAmount: (json['partnerAmount'] as num?)?.toDouble() ?? 0,
      totalRevenue: (json['totalRevenue'] as num?)?.toDouble() ?? 0,
      waqfPercent: (json['waqfPercent'] as num?)?.toDouble() ?? 0,
      calculationMethod: (json['calculationMethod'] as String?) ?? '',
      calculationDetail: (json['calculationDetail'] as String?) ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
        'waqfAmount': waqfAmount,
        'partnerAmount': partnerAmount,
        'totalRevenue': totalRevenue,
        'waqfPercent': waqfPercent,
        'calculationMethod': calculationMethod,
        'calculationDetail': calculationDetail,
      };

  String get waqfAmountFormatted => '${waqfAmount.toStringAsFixed(0)} د.ع';
  String get partnerAmountFormatted => '${partnerAmount.toStringAsFixed(0)} د.ع';
}

class PartnerContactEntry {
  final int id;
  final int partnershipId;
  final ContactType contactType;
  final String messageBody;
  final DateTime sentAt;

  PartnerContactEntry({
    required this.id,
    required this.partnershipId,
    required this.contactType,
    required this.messageBody,
    required this.sentAt,
  });

  factory PartnerContactEntry.fromJson(Map<String, dynamic> json) {
    return PartnerContactEntry(
      id: (json['id'] as num?)?.toInt() ?? 0,
      partnershipId: (json['partnershipId'] as num?)?.toInt() ?? 0,
      contactType: _contactTypeFromString((json['contactType'] as String?) ?? 'sms'),
      messageBody: (json['messageBody'] as String?) ?? '',
      sentAt: json['sentAt'] != null ? DateTime.parse(json['sentAt']) : DateTime.now(),
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'partnershipId': partnershipId,
        'contactType': contactType.name,
        'messageBody': messageBody,
        'sentAt': sentAt.toIso8601String(),
      };

  String get typeIconEmoji => switch (contactType) {
        ContactType.sms => '💬',
        ContactType.whatsApp => '📱',
        ContactType.email => '📧',
        ContactType.phone => '📞',
        ContactType.meeting => '🤝',
        ContactType.letter => '📨',
        ContactType.pdf => '📄',
      };
}

class RevenueDistributionCreateRequest {
  final int partnershipId;
  final String periodLabel;
  final DateTime periodStartDate;
  final DateTime periodEndDate;
  final double totalRevenue;
  final String distributionType;
  final String? transferMethod;
  final String? notes;

  RevenueDistributionCreateRequest({
    required this.partnershipId,
    required this.periodLabel,
    required this.periodStartDate,
    required this.periodEndDate,
    required this.totalRevenue,
    this.distributionType = 'Revenue',
    this.transferMethod,
    this.notes,
  });

  Map<String, dynamic> toJson() => {
        'partnershipId': partnershipId,
        'periodLabel': periodLabel,
        'periodStartDate': periodStartDate.toIso8601String(),
        'periodEndDate': periodEndDate.toIso8601String(),
        'totalRevenue': totalRevenue,
        'distributionType': distributionType,
        'transferMethod': transferMethod,
        'notes': notes,
      };
}

PartnershipType _partnershipTypeFromString(String value) {
  return PartnershipType.values.firstWhere(
    (e) => e.name.toLowerCase() == value.toLowerCase(),
    orElse: () => PartnershipType.revenuePercent,
  );
}

PartnerType _partnerTypeFromString(String value) {
  return PartnerType.values.firstWhere(
    (e) => e.name.toLowerCase() == value.toLowerCase(),
    orElse: () => PartnerType.individual,
  );
}

TransferStatus _transferStatusFromString(String value) {
  return TransferStatus.values.firstWhere(
    (e) => e.name.toLowerCase() == value.toLowerCase(),
    orElse: () => TransferStatus.pending,
  );
}

ContactType _contactTypeFromString(String value) {
  final normalized = value.toLowerCase();
  if (normalized == 'whatsapp') return ContactType.whatsApp;
  return ContactType.values.firstWhere(
    (e) => e.name.toLowerCase() == normalized,
    orElse: () => ContactType.sms,
  );
}

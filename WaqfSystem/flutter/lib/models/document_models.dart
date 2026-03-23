import 'dart:convert';

enum DocumentStatus { pendingVerification, verified, rejected, expiringSoon, expired, archived }
enum DocumentCategory { ownership, survey, construction, legal, partnership, lease, engineering, insurance }

extension DocumentStatusX on DocumentStatus {
  String get displayAr => switch (this) {
        DocumentStatus.pendingVerification => 'تنتظر تحقق',
        DocumentStatus.verified => 'معتمدة',
        DocumentStatus.rejected => 'مرفوضة',
        DocumentStatus.expiringSoon => 'تنتهي قريباً',
        DocumentStatus.expired => 'منتهية',
        DocumentStatus.archived => 'أرشيف',
      };

  String get colorHex => switch (this) {
        DocumentStatus.pendingVerification => '#378ADD',
        DocumentStatus.verified => '#3B6D11',
        DocumentStatus.rejected => '#A32D2D',
        DocumentStatus.expiringSoon => '#BA7517',
        DocumentStatus.expired => '#DC2626',
        DocumentStatus.archived => '#888780',
      };
}

DocumentStatus _statusFromString(String? value) {
  final v = (value ?? '').trim().toLowerCase();
  return DocumentStatus.values.firstWhere((e) => e.name.toLowerCase() == v, orElse: () => DocumentStatus.pendingVerification);
}

class PropertyDocumentListItem {
  final int id;
  final int propertyId;
  final String propertyNameAr;
  final String propertyWqfNumber;
  final int documentTypeId;
  final String documentTypeNameAr;
  final String documentTypeCode;
  final String category;
  final String title;
  final String? documentNumber;
  final String? issuingAuthority;
  final DateTime? issueDate;
  final DateTime? expiryDate;
  final DocumentStatus status;
  final String statusDisplayAr;
  final String statusColor;
  final int versionCount;
  final int? currentVersionId;
  final String? currentFileName;
  final int? currentFileSizeBytes;
  final String? primaryResponsibleName;
  final String? verifiedByName;
  final DateTime? verifiedAt;
  final List<String> tags;
  final bool hasOcr;
  final DateTime createdAt;
  final String createdByName;
  final int unreadAlertCount;

  const PropertyDocumentListItem({
    required this.id,
    required this.propertyId,
    required this.propertyNameAr,
    required this.propertyWqfNumber,
    required this.documentTypeId,
    required this.documentTypeNameAr,
    required this.documentTypeCode,
    required this.category,
    required this.title,
    required this.documentNumber,
    required this.issuingAuthority,
    required this.issueDate,
    required this.expiryDate,
    required this.status,
    required this.statusDisplayAr,
    required this.statusColor,
    required this.versionCount,
    required this.currentVersionId,
    required this.currentFileName,
    required this.currentFileSizeBytes,
    required this.primaryResponsibleName,
    required this.verifiedByName,
    required this.verifiedAt,
    required this.tags,
    required this.hasOcr,
    required this.createdAt,
    required this.createdByName,
    required this.unreadAlertCount,
  });

  int? get daysUntilExpiry => expiryDate == null ? null : expiryDate!.difference(DateTime.now()).inDays;
  bool get isExpired => daysUntilExpiry != null && daysUntilExpiry! < 0;
  bool get isExpiringSoon => daysUntilExpiry != null && daysUntilExpiry! <= 90 && daysUntilExpiry! > 0;

  String get expiryDisplayAr {
    if (expiryDate == null) return 'بدون انتهاء';
    if (isExpired) return 'منتهية';
    return 'تنتهي خلال ${daysUntilExpiry ?? 0} يوم';
  }

  factory PropertyDocumentListItem.fromJson(Map<String, dynamic> json) => PropertyDocumentListItem(
        id: (json['id'] as num?)?.toInt() ?? 0,
        propertyId: (json['propertyId'] as num?)?.toInt() ?? 0,
        propertyNameAr: (json['propertyNameAr'] as String?) ?? '',
        propertyWqfNumber: (json['propertyWqfNumber'] as String?) ?? '',
        documentTypeId: (json['documentTypeId'] as num?)?.toInt() ?? 0,
        documentTypeNameAr: (json['documentTypeNameAr'] as String?) ?? '',
        documentTypeCode: (json['documentTypeCode'] as String?) ?? '',
        category: (json['category'] as String?) ?? '',
        title: (json['title'] as String?) ?? '',
        documentNumber: json['documentNumber'] as String?,
        issuingAuthority: json['issuingAuthority'] as String?,
        issueDate: DateTime.tryParse((json['issueDate'] as String?) ?? ''),
        expiryDate: DateTime.tryParse((json['expiryDate'] as String?) ?? ''),
        status: _statusFromString(json['status'] as String?),
        statusDisplayAr: (json['statusDisplayAr'] as String?) ?? '',
        statusColor: (json['statusColor'] as String?) ?? '#378ADD',
        versionCount: (json['versionCount'] as num?)?.toInt() ?? 0,
        currentVersionId: (json['currentVersionId'] as num?)?.toInt(),
        currentFileName: json['currentFileName'] as String?,
        currentFileSizeBytes: (json['currentFileSizeBytes'] as num?)?.toInt(),
        primaryResponsibleName: json['primaryResponsibleName'] as String?,
        verifiedByName: json['verifiedByName'] as String?,
        verifiedAt: DateTime.tryParse((json['verifiedAt'] as String?) ?? ''),
        tags: ((json['tags'] as List?) ?? <dynamic>[]).map((e) => e.toString()).toList(),
        hasOcr: (json['hasOcr'] as bool?) ?? false,
        createdAt: DateTime.tryParse((json['createdAt'] as String?) ?? '') ?? DateTime.now(),
        createdByName: (json['createdByName'] as String?) ?? '',
        unreadAlertCount: (json['unreadAlertCount'] as num?)?.toInt() ?? 0,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'propertyId': propertyId,
        'propertyNameAr': propertyNameAr,
        'propertyWqfNumber': propertyWqfNumber,
        'documentTypeId': documentTypeId,
        'documentTypeNameAr': documentTypeNameAr,
        'documentTypeCode': documentTypeCode,
        'category': category,
        'title': title,
        'documentNumber': documentNumber,
        'issuingAuthority': issuingAuthority,
        'issueDate': issueDate?.toIso8601String(),
        'expiryDate': expiryDate?.toIso8601String(),
        'status': status.name,
        'statusDisplayAr': statusDisplayAr,
        'statusColor': statusColor,
        'versionCount': versionCount,
        'currentVersionId': currentVersionId,
        'currentFileName': currentFileName,
        'currentFileSizeBytes': currentFileSizeBytes,
        'primaryResponsibleName': primaryResponsibleName,
        'verifiedByName': verifiedByName,
        'verifiedAt': verifiedAt?.toIso8601String(),
        'tags': tags,
        'hasOcr': hasOcr,
        'createdAt': createdAt.toIso8601String(),
        'createdByName': createdByName,
        'unreadAlertCount': unreadAlertCount,
      };

  PropertyDocumentListItem copyWith({
    DocumentStatus? status,
    int? currentVersionId,
    int? versionCount,
  }) {
    return PropertyDocumentListItem(
      id: id,
      propertyId: propertyId,
      propertyNameAr: propertyNameAr,
      propertyWqfNumber: propertyWqfNumber,
      documentTypeId: documentTypeId,
      documentTypeNameAr: documentTypeNameAr,
      documentTypeCode: documentTypeCode,
      category: category,
      title: title,
      documentNumber: documentNumber,
      issuingAuthority: issuingAuthority,
      issueDate: issueDate,
      expiryDate: expiryDate,
      status: status ?? this.status,
      statusDisplayAr: statusDisplayAr,
      statusColor: statusColor,
      versionCount: versionCount ?? this.versionCount,
      currentVersionId: currentVersionId ?? this.currentVersionId,
      currentFileName: currentFileName,
      currentFileSizeBytes: currentFileSizeBytes,
      primaryResponsibleName: primaryResponsibleName,
      verifiedByName: verifiedByName,
      verifiedAt: verifiedAt,
      tags: tags,
      hasOcr: hasOcr,
      createdAt: createdAt,
      createdByName: createdByName,
      unreadAlertCount: unreadAlertCount,
    );
  }
}

class DocumentVersion {
  final int id;
  final int documentId;
  final int versionNumber;
  final String fileUrl;
  final String fileName;
  final String fileExtension;
  final int fileSizeBytes;
  final String fileSizeDisplay;
  final String mimeType;
  final String? thumbnailUrl;
  final int? pageCount;
  final String uploadedByName;
  final DateTime uploadedAt;
  final bool isCurrent;
  final String? notes;

  const DocumentVersion({
    required this.id,
    required this.documentId,
    required this.versionNumber,
    required this.fileUrl,
    required this.fileName,
    required this.fileExtension,
    required this.fileSizeBytes,
    required this.fileSizeDisplay,
    required this.mimeType,
    required this.thumbnailUrl,
    required this.pageCount,
    required this.uploadedByName,
    required this.uploadedAt,
    required this.isCurrent,
    required this.notes,
  });

  factory DocumentVersion.fromJson(Map<String, dynamic> json) => DocumentVersion(
        id: (json['id'] as num?)?.toInt() ?? 0,
        documentId: (json['documentId'] as num?)?.toInt() ?? 0,
        versionNumber: (json['versionNumber'] as num?)?.toInt() ?? 0,
        fileUrl: (json['fileUrl'] as String?) ?? '',
        fileName: (json['fileName'] as String?) ?? '',
        fileExtension: (json['fileExtension'] as String?) ?? '',
        fileSizeBytes: (json['fileSizeBytes'] as num?)?.toInt() ?? 0,
        fileSizeDisplay: (json['fileSizeDisplay'] as String?) ?? '',
        mimeType: (json['mimeType'] as String?) ?? '',
        thumbnailUrl: json['thumbnailUrl'] as String?,
        pageCount: (json['pageCount'] as num?)?.toInt(),
        uploadedByName: (json['uploadedByName'] as String?) ?? '',
        uploadedAt: DateTime.tryParse((json['uploadedAt'] as String?) ?? '') ?? DateTime.now(),
        isCurrent: (json['isCurrent'] as bool?) ?? false,
        notes: json['notes'] as String?,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'documentId': documentId,
        'versionNumber': versionNumber,
        'fileUrl': fileUrl,
        'fileName': fileName,
        'fileExtension': fileExtension,
        'fileSizeBytes': fileSizeBytes,
        'fileSizeDisplay': fileSizeDisplay,
        'mimeType': mimeType,
        'thumbnailUrl': thumbnailUrl,
        'pageCount': pageCount,
        'uploadedByName': uploadedByName,
        'uploadedAt': uploadedAt.toIso8601String(),
        'isCurrent': isCurrent,
        'notes': notes,
      };
}

class DocumentAuditTrailItem {
  final int id;
  final String actionType;
  final String actionTypeDisplayAr;
  final String? actionByName;
  final DateTime actionAt;
  final String? details;
  final String? oldValue;
  final String? newValue;
  final int? versionNumber;

  const DocumentAuditTrailItem({
    required this.id,
    required this.actionType,
    required this.actionTypeDisplayAr,
    required this.actionByName,
    required this.actionAt,
    required this.details,
    required this.oldValue,
    required this.newValue,
    required this.versionNumber,
  });

  factory DocumentAuditTrailItem.fromJson(Map<String, dynamic> json) => DocumentAuditTrailItem(
        id: (json['id'] as num?)?.toInt() ?? 0,
        actionType: (json['actionType'] as String?) ?? '',
        actionTypeDisplayAr: (json['actionTypeDisplayAr'] as String?) ?? '',
        actionByName: json['actionByName'] as String?,
        actionAt: DateTime.tryParse((json['actionAt'] as String?) ?? '') ?? DateTime.now(),
        details: json['details'] as String?,
        oldValue: json['oldValue'] as String?,
        newValue: json['newValue'] as String?,
        versionNumber: (json['versionNumber'] as num?)?.toInt(),
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'actionType': actionType,
        'actionTypeDisplayAr': actionTypeDisplayAr,
        'actionByName': actionByName,
        'actionAt': actionAt.toIso8601String(),
        'details': details,
        'oldValue': oldValue,
        'newValue': newValue,
        'versionNumber': versionNumber,
      };
}

class DocumentAlert {
  final int id;
  final int documentId;
  final String documentTitle;
  final String propertyNameAr;
  final String propertyWqfNumber;
  final String documentTypeNameAr;
  final DateTime? expiryDate;
  final int? daysRemaining;
  final int alertLevel;
  final String alertType;
  final bool isRead;
  final DateTime? readAt;
  final DateTime createdAt;
  final String urgencyColor;
  final String urgencyLabel;

  const DocumentAlert({
    required this.id,
    required this.documentId,
    required this.documentTitle,
    required this.propertyNameAr,
    required this.propertyWqfNumber,
    required this.documentTypeNameAr,
    required this.expiryDate,
    required this.daysRemaining,
    required this.alertLevel,
    required this.alertType,
    required this.isRead,
    required this.readAt,
    required this.createdAt,
    required this.urgencyColor,
    required this.urgencyLabel,
  });

  bool get isExpired => daysRemaining != null && daysRemaining! < 0;

  String get urgencyLabelAr {
    if (alertLevel == 3) return 'منتهية';
    if (alertLevel == 2) return 'حرجة';
    return 'تنبيه';
  }

  factory DocumentAlert.fromJson(Map<String, dynamic> json) => DocumentAlert(
        id: (json['id'] as num?)?.toInt() ?? 0,
        documentId: (json['documentId'] as num?)?.toInt() ?? 0,
        documentTitle: (json['documentTitle'] as String?) ?? '',
        propertyNameAr: (json['propertyNameAr'] as String?) ?? '',
        propertyWqfNumber: (json['propertyWqfNumber'] as String?) ?? '',
        documentTypeNameAr: (json['documentTypeNameAr'] as String?) ?? '',
        expiryDate: DateTime.tryParse((json['expiryDate'] as String?) ?? ''),
        daysRemaining: (json['daysRemaining'] as num?)?.toInt(),
        alertLevel: (json['alertLevel'] as num?)?.toInt() ?? 0,
        alertType: (json['alertType'] as String?) ?? '',
        isRead: (json['isRead'] as bool?) ?? false,
        readAt: DateTime.tryParse((json['readAt'] as String?) ?? ''),
        createdAt: DateTime.tryParse((json['createdAt'] as String?) ?? '') ?? DateTime.now(),
        urgencyColor: (json['urgencyColor'] as String?) ?? '#d97706',
        urgencyLabel: (json['urgencyLabel'] as String?) ?? '',
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'documentId': documentId,
        'documentTitle': documentTitle,
        'propertyNameAr': propertyNameAr,
        'propertyWqfNumber': propertyWqfNumber,
        'documentTypeNameAr': documentTypeNameAr,
        'expiryDate': expiryDate?.toIso8601String(),
        'daysRemaining': daysRemaining,
        'alertLevel': alertLevel,
        'alertType': alertType,
        'isRead': isRead,
        'readAt': readAt?.toIso8601String(),
        'createdAt': createdAt.toIso8601String(),
        'urgencyColor': urgencyColor,
        'urgencyLabel': urgencyLabel,
      };

  DocumentAlert copyWith({bool? isRead}) => DocumentAlert(
        id: id,
        documentId: documentId,
        documentTitle: documentTitle,
        propertyNameAr: propertyNameAr,
        propertyWqfNumber: propertyWqfNumber,
        documentTypeNameAr: documentTypeNameAr,
        expiryDate: expiryDate,
        daysRemaining: daysRemaining,
        alertLevel: alertLevel,
        alertType: alertType,
        isRead: isRead ?? this.isRead,
        readAt: readAt,
        createdAt: createdAt,
        urgencyColor: urgencyColor,
        urgencyLabel: urgencyLabel,
      );
}

class DocumentType {
  final int id;
  final String code;
  final String nameAr;
  final String nameEn;
  final String category;
  final bool isRequired;
  final bool hasExpiry;
  final int? alertDays1;
  final int? alertDays2;
  final String allowedExtensions;
  final List<String> verifierRoles;
  final bool isActive;
  final int documentCount;

  const DocumentType({
    required this.id,
    required this.code,
    required this.nameAr,
    required this.nameEn,
    required this.category,
    required this.isRequired,
    required this.hasExpiry,
    required this.alertDays1,
    required this.alertDays2,
    required this.allowedExtensions,
    required this.verifierRoles,
    required this.isActive,
    required this.documentCount,
  });

  factory DocumentType.fromJson(Map<String, dynamic> json) => DocumentType(
        id: (json['id'] as num?)?.toInt() ?? 0,
        code: (json['code'] as String?) ?? '',
        nameAr: (json['nameAr'] as String?) ?? '',
        nameEn: (json['nameEn'] as String?) ?? '',
        category: (json['category'] as String?) ?? '',
        isRequired: (json['isRequired'] as bool?) ?? false,
        hasExpiry: (json['hasExpiry'] as bool?) ?? false,
        alertDays1: (json['alertDays1'] as num?)?.toInt(),
        alertDays2: (json['alertDays2'] as num?)?.toInt(),
        allowedExtensions: (json['allowedExtensions'] as String?) ?? '',
        verifierRoles: ((json['verifierRoles'] as List?) ?? <dynamic>[]).map((e) => e.toString()).toList(),
        isActive: (json['isActive'] as bool?) ?? true,
        documentCount: (json['documentCount'] as num?)?.toInt() ?? 0,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'code': code,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'category': category,
        'isRequired': isRequired,
        'hasExpiry': hasExpiry,
        'alertDays1': alertDays1,
        'alertDays2': alertDays2,
        'allowedExtensions': allowedExtensions,
        'verifierRoles': verifierRoles,
        'isActive': isActive,
        'documentCount': documentCount,
      };
}

class PropertyDocumentSummary {
  final int propertyId;
  final int totalDocuments;
  final int verifiedCount;
  final int pendingCount;
  final int expiredCount;
  final int expiringSoonCount;
  final int expiringSoon90Count;
  final List<String> missingRequiredTypes;
  final double compliancePercent;
  final DateTime? lastUpdatedAt;

  const PropertyDocumentSummary({
    required this.propertyId,
    required this.totalDocuments,
    required this.verifiedCount,
    required this.pendingCount,
    required this.expiredCount,
    required this.expiringSoonCount,
    required this.expiringSoon90Count,
    required this.missingRequiredTypes,
    required this.compliancePercent,
    required this.lastUpdatedAt,
  });

  String get complianceDisplayAr => '${compliancePercent.toStringAsFixed(1)}%';

  factory PropertyDocumentSummary.fromJson(Map<String, dynamic> json) => PropertyDocumentSummary(
        propertyId: (json['propertyId'] as num?)?.toInt() ?? 0,
        totalDocuments: (json['totalDocuments'] as num?)?.toInt() ?? 0,
        verifiedCount: (json['verifiedCount'] as num?)?.toInt() ?? 0,
        pendingCount: (json['pendingCount'] as num?)?.toInt() ?? 0,
        expiredCount: (json['expiredCount'] as num?)?.toInt() ?? 0,
        expiringSoonCount: (json['expiringSoonCount'] as num?)?.toInt() ?? 0,
        expiringSoon90Count: (json['expiringSoon90Count'] as num?)?.toInt() ?? 0,
        missingRequiredTypes: ((json['missingRequiredTypes'] as List?) ?? <dynamic>[]).map((e) => e.toString()).toList(),
        compliancePercent: (json['compliancePercent'] as num?)?.toDouble() ?? 0,
        lastUpdatedAt: DateTime.tryParse((json['lastUpdatedAt'] as String?) ?? ''),
      );

  Map<String, dynamic> toJson() => {
        'propertyId': propertyId,
        'totalDocuments': totalDocuments,
        'verifiedCount': verifiedCount,
        'pendingCount': pendingCount,
        'expiredCount': expiredCount,
        'expiringSoonCount': expiringSoonCount,
        'expiringSoon90Count': expiringSoon90Count,
        'missingRequiredTypes': missingRequiredTypes,
        'compliancePercent': compliancePercent,
        'lastUpdatedAt': lastUpdatedAt?.toIso8601String(),
      };
}

class PropertyDocumentDetail extends PropertyDocumentListItem {
  final String? description;
  final String? ocrText;
  final double? ocrConfidence;
  final int? linkedUnitId;
  final String? linkedUnitNumber;
  final int? linkedPartnershipId;
  final String? verificationNotes;
  final String? rejectionReason;
  final List<DocumentVersion> versions;
  final List<DocumentAuditTrailItem> auditTrail;
  final List<DocumentAlert> activeAlerts;

  const PropertyDocumentDetail({
    required super.id,
    required super.propertyId,
    required super.propertyNameAr,
    required super.propertyWqfNumber,
    required super.documentTypeId,
    required super.documentTypeNameAr,
    required super.documentTypeCode,
    required super.category,
    required super.title,
    required super.documentNumber,
    required super.issuingAuthority,
    required super.issueDate,
    required super.expiryDate,
    required super.status,
    required super.statusDisplayAr,
    required super.statusColor,
    required super.versionCount,
    required super.currentVersionId,
    required super.currentFileName,
    required super.currentFileSizeBytes,
    required super.primaryResponsibleName,
    required super.verifiedByName,
    required super.verifiedAt,
    required super.tags,
    required super.hasOcr,
    required super.createdAt,
    required super.createdByName,
    required super.unreadAlertCount,
    required this.description,
    required this.ocrText,
    required this.ocrConfidence,
    required this.linkedUnitId,
    required this.linkedUnitNumber,
    required this.linkedPartnershipId,
    required this.verificationNotes,
    required this.rejectionReason,
    required this.versions,
    required this.auditTrail,
    required this.activeAlerts,
  });

  factory PropertyDocumentDetail.fromJson(Map<String, dynamic> json) {
    final base = PropertyDocumentListItem.fromJson(json);
    return PropertyDocumentDetail(
      id: base.id,
      propertyId: base.propertyId,
      propertyNameAr: base.propertyNameAr,
      propertyWqfNumber: base.propertyWqfNumber,
      documentTypeId: base.documentTypeId,
      documentTypeNameAr: base.documentTypeNameAr,
      documentTypeCode: base.documentTypeCode,
      category: base.category,
      title: base.title,
      documentNumber: base.documentNumber,
      issuingAuthority: base.issuingAuthority,
      issueDate: base.issueDate,
      expiryDate: base.expiryDate,
      status: base.status,
      statusDisplayAr: base.statusDisplayAr,
      statusColor: base.statusColor,
      versionCount: base.versionCount,
      currentVersionId: base.currentVersionId,
      currentFileName: base.currentFileName,
      currentFileSizeBytes: base.currentFileSizeBytes,
      primaryResponsibleName: base.primaryResponsibleName,
      verifiedByName: base.verifiedByName,
      verifiedAt: base.verifiedAt,
      tags: base.tags,
      hasOcr: base.hasOcr,
      createdAt: base.createdAt,
      createdByName: base.createdByName,
      unreadAlertCount: base.unreadAlertCount,
      description: json['description'] as String?,
      ocrText: json['ocrText'] as String?,
      ocrConfidence: (json['ocrConfidence'] as num?)?.toDouble(),
      linkedUnitId: (json['linkedUnitId'] as num?)?.toInt(),
      linkedUnitNumber: json['linkedUnitNumber'] as String?,
      linkedPartnershipId: (json['linkedPartnershipId'] as num?)?.toInt(),
      verificationNotes: json['verificationNotes'] as String?,
      rejectionReason: json['rejectionReason'] as String?,
      versions: ((json['versions'] as List?) ?? <dynamic>[]).map((e) => DocumentVersion.fromJson((e as Map).cast<String, dynamic>())).toList(),
      auditTrail: ((json['auditTrail'] as List?) ?? <dynamic>[]).map((e) => DocumentAuditTrailItem.fromJson((e as Map).cast<String, dynamic>())).toList(),
      activeAlerts: ((json['activeAlerts'] as List?) ?? <dynamic>[]).map((e) => DocumentAlert.fromJson((e as Map).cast<String, dynamic>())).toList(),
    );
  }

  Map<String, dynamic> toJson() {
    final base = super.toJson();
    base.addAll({
      'description': description,
      'ocrText': ocrText,
      'ocrConfidence': ocrConfidence,
      'linkedUnitId': linkedUnitId,
      'linkedUnitNumber': linkedUnitNumber,
      'linkedPartnershipId': linkedPartnershipId,
      'verificationNotes': verificationNotes,
      'rejectionReason': rejectionReason,
      'versions': versions.map((e) => e.toJson()).toList(),
      'auditTrail': auditTrail.map((e) => e.toJson()).toList(),
      'activeAlerts': activeAlerts.map((e) => e.toJson()).toList(),
    });
    return base;
  }

  PropertyDocumentDetail copyWith({DocumentStatus? status}) => PropertyDocumentDetail.fromJson({
        ...toJson(),
        'status': (status ?? this.status).name,
      });
}

class UploadDocumentRequest {
  final int propertyId;
  final int documentTypeId;
  final String title;
  final String? description;
  final String? documentNumber;
  final String? issuingAuthority;
  final DateTime? issueDate;
  final DateTime? expiryDate;
  final int? linkedUnitId;
  final int? linkedPartnershipId;
  final int? primaryResponsibleId;
  final List<String>? tags;
  final String? notes;

  const UploadDocumentRequest({
    required this.propertyId,
    required this.documentTypeId,
    required this.title,
    required this.description,
    required this.documentNumber,
    required this.issuingAuthority,
    required this.issueDate,
    required this.expiryDate,
    required this.linkedUnitId,
    required this.linkedPartnershipId,
    required this.primaryResponsibleId,
    required this.tags,
    required this.notes,
  });

  Map<String, dynamic> toJson() => {
        'propertyId': propertyId,
        'documentTypeId': documentTypeId,
        'title': title,
        'description': description,
        'documentNumber': documentNumber,
        'issuingAuthority': issuingAuthority,
        'issueDate': issueDate?.toIso8601String(),
        'expiryDate': expiryDate?.toIso8601String(),
        'linkedUnitId': linkedUnitId,
        'linkedPartnershipId': linkedPartnershipId,
        'primaryResponsibleId': primaryResponsibleId,
        'tags': tags,
        'notes': notes,
      };

  Map<String, String> toFormData() {
    final map = <String, String>{
      'propertyId': '$propertyId',
      'documentTypeId': '$documentTypeId',
      'title': title,
    };

    if (description != null) map['description'] = description!;
    if (documentNumber != null) map['documentNumber'] = documentNumber!;
    if (issuingAuthority != null) map['issuingAuthority'] = issuingAuthority!;
    if (issueDate != null) map['issueDate'] = issueDate!.toIso8601String();
    if (expiryDate != null) map['expiryDate'] = expiryDate!.toIso8601String();
    if (linkedUnitId != null) map['linkedUnitId'] = '$linkedUnitId';
    if (linkedPartnershipId != null) map['linkedPartnershipId'] = '$linkedPartnershipId';
    if (primaryResponsibleId != null) map['primaryResponsibleId'] = '$primaryResponsibleId';
    if (notes != null) map['notes'] = notes!;
    if (tags != null) map['tags'] = jsonEncode(tags);
    return map;
  }
}

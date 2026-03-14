import 'dart:convert';

enum MissionStage {
  created,
  assigned,
  accepted,
  inProgress,
  dataEntry,
  submittedForReview,
  underReview,
  completed,
  sentForCorrection,
  cancelled,
  rejected
}

enum MissionType { propertyCensus, periodicInspection, documentVerification, emergencyAssessment, followUp }
enum MissionPriority { low, normal, high, urgent }
enum EntryStatus { inProgress, submitted, underReview, approved, rejected }

extension MissionStageX on MissionStage {
  String get displayAr => switch (this) {
        MissionStage.created => 'مُنشأة',
        MissionStage.assigned => 'مُكلَّفة',
        MissionStage.accepted => 'مقبولة',
        MissionStage.inProgress => 'جارية',
        MissionStage.dataEntry => 'إدخال بيانات',
        MissionStage.submittedForReview => 'مُقدَّمة للمراجعة',
        MissionStage.underReview => 'قيد المراجعة',
        MissionStage.completed => 'مكتملة',
        MissionStage.sentForCorrection => 'مُعادة للتصحيح',
        MissionStage.cancelled => 'ملغاة',
        MissionStage.rejected => 'مرفوضة',
      };

  String get colorHex => switch (this) {
        MissionStage.created => '#888780',
        MissionStage.assigned => '#378ADD',
        MissionStage.accepted => '#1D9E75',
        MissionStage.inProgress => '#BA7517',
        MissionStage.dataEntry => '#534AB7',
        MissionStage.submittedForReview => '#D97706',
        MissionStage.underReview => '#D85A30',
        MissionStage.completed => '#3B6D11',
        MissionStage.sentForCorrection => '#A32D2D',
        MissionStage.cancelled => '#444441',
        MissionStage.rejected => '#791F1F',
      };
}

MissionStage _stageFromString(String value) {
  return MissionStage.values.firstWhere(
    (e) => e.name.toLowerCase() == value.toLowerCase(),
    orElse: () => MissionStage.created,
  );
}

MissionType _typeFromString(String value) {
  return MissionType.values.firstWhere(
    (e) => e.name.toLowerCase() == value.toLowerCase(),
    orElse: () => MissionType.propertyCensus,
  );
}

MissionPriority _priorityFromString(String value) {
  return MissionPriority.values.firstWhere(
    (e) => e.name.toLowerCase() == value.toLowerCase(),
    orElse: () => MissionPriority.normal,
  );
}

class MissionListItem {
  final int id;
  final String missionCode;
  final String title;
  final MissionType missionType;
  final MissionStage stage;
  final MissionPriority priority;
  final String governorateNameAr;
  final String? districtNameAr;
  final String? assignedToName;
  final String assignedToAvatar;
  final DateTime missionDate;
  final DateTime? expectedCompletionDate;
  final int targetPropertyCount;
  final int enteredPropertyCount;
  final double progressPercent;
  final double? averageDqsScore;
  final bool isUrgent;
  final bool isOverdue;
  final int daysRemaining;
  final String stageColor;
  final String stageDisplayAr;

  const MissionListItem({
    required this.id,
    required this.missionCode,
    required this.title,
    required this.missionType,
    required this.stage,
    required this.priority,
    required this.governorateNameAr,
    required this.districtNameAr,
    required this.assignedToName,
    required this.assignedToAvatar,
    required this.missionDate,
    required this.expectedCompletionDate,
    required this.targetPropertyCount,
    required this.enteredPropertyCount,
    required this.progressPercent,
    required this.averageDqsScore,
    required this.isUrgent,
    required this.isOverdue,
    required this.daysRemaining,
    required this.stageColor,
    required this.stageDisplayAr,
  });

  bool get canAccept => stage == MissionStage.assigned;
  bool get canCheckin => stage == MissionStage.accepted;
  bool get canSubmit => stage == MissionStage.dataEntry;

  factory MissionListItem.fromJson(Map<String, dynamic> json) {
    return MissionListItem(
      id: (json['id'] as num?)?.toInt() ?? 0,
      missionCode: (json['missionCode'] as String?) ?? '',
      title: (json['title'] as String?) ?? '',
      missionType: _typeFromString((json['missionType'] as String?) ?? 'propertyCensus'),
      stage: _stageFromString((json['stage'] as String?) ?? 'created'),
      priority: _priorityFromString((json['priority'] as String?) ?? 'normal'),
      governorateNameAr: (json['governorateNameAr'] as String?) ?? '',
      districtNameAr: json['districtNameAr'] as String?,
      assignedToName: json['assignedToName'] as String?,
      assignedToAvatar: (json['assignedToAvatar'] as String?) ?? '--',
      missionDate: DateTime.tryParse((json['missionDate'] as String?) ?? '') ?? DateTime.now(),
      expectedCompletionDate: DateTime.tryParse((json['expectedCompletionDate'] as String?) ?? ''),
      targetPropertyCount: (json['targetPropertyCount'] as num?)?.toInt() ?? 0,
      enteredPropertyCount: (json['enteredPropertyCount'] as num?)?.toInt() ?? 0,
      progressPercent: (json['progressPercent'] as num?)?.toDouble() ?? 0,
      averageDqsScore: (json['averageDqsScore'] as num?)?.toDouble(),
      isUrgent: (json['isUrgent'] as bool?) ?? false,
      isOverdue: (json['isOverdue'] as bool?) ?? false,
      daysRemaining: (json['daysRemaining'] as num?)?.toInt() ?? 0,
      stageColor: (json['stageColor'] as String?) ?? '#999999',
      stageDisplayAr: (json['stageDisplayAr'] as String?) ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'missionCode': missionCode,
        'title': title,
        'missionType': missionType.name,
        'stage': stage.name,
        'priority': priority.name,
        'governorateNameAr': governorateNameAr,
        'districtNameAr': districtNameAr,
        'assignedToName': assignedToName,
        'assignedToAvatar': assignedToAvatar,
        'missionDate': missionDate.toIso8601String(),
        'expectedCompletionDate': expectedCompletionDate?.toIso8601String(),
        'targetPropertyCount': targetPropertyCount,
        'enteredPropertyCount': enteredPropertyCount,
        'progressPercent': progressPercent,
        'averageDqsScore': averageDqsScore,
        'isUrgent': isUrgent,
        'isOverdue': isOverdue,
        'daysRemaining': daysRemaining,
        'stageColor': stageColor,
        'stageDisplayAr': stageDisplayAr,
      };

  MissionListItem copyWith({
    double? progressPercent,
    MissionStage? stage,
  }) {
    return MissionListItem(
      id: id,
      missionCode: missionCode,
      title: title,
      missionType: missionType,
      stage: stage ?? this.stage,
      priority: priority,
      governorateNameAr: governorateNameAr,
      districtNameAr: districtNameAr,
      assignedToName: assignedToName,
      assignedToAvatar: assignedToAvatar,
      missionDate: missionDate,
      expectedCompletionDate: expectedCompletionDate,
      targetPropertyCount: targetPropertyCount,
      enteredPropertyCount: enteredPropertyCount,
      progressPercent: progressPercent ?? this.progressPercent,
      averageDqsScore: averageDqsScore,
      isUrgent: isUrgent,
      isOverdue: isOverdue,
      daysRemaining: daysRemaining,
      stageColor: stageColor,
      stageDisplayAr: stageDisplayAr,
    );
  }
}

class UserBrief {
  final int id;
  final String fullName;
  final String role;
  final int? governorateId;
  final String? phone;
  final String avatarInitials;

  const UserBrief({required this.id, required this.fullName, required this.role, required this.governorateId, required this.phone, required this.avatarInitials});

  factory UserBrief.fromJson(Map<String, dynamic> json) => UserBrief(
        id: (json['id'] as num?)?.toInt() ?? 0,
        fullName: (json['fullName'] as String?) ?? '',
        role: (json['role'] as String?) ?? '',
        governorateId: (json['governorateId'] as num?)?.toInt(),
        phone: json['phone'] as String?,
        avatarInitials: (json['avatarInitials'] as String?) ?? '--',
      );

  Map<String, dynamic> toJson() => {'id': id, 'fullName': fullName, 'role': role, 'governorateId': governorateId, 'phone': phone, 'avatarInitials': avatarInitials};
}

class MissionStageHistoryItem {
  final String fromStageAr;
  final String toStageAr;
  final String changedByName;
  final DateTime changedAt;
  final String? notes;

  const MissionStageHistoryItem({required this.fromStageAr, required this.toStageAr, required this.changedByName, required this.changedAt, required this.notes});

  factory MissionStageHistoryItem.fromJson(Map<String, dynamic> json) => MissionStageHistoryItem(
        fromStageAr: (json['fromStageAr'] as String?) ?? '',
        toStageAr: (json['toStageAr'] as String?) ?? '',
        changedByName: (json['changedByName'] as String?) ?? '',
        changedAt: DateTime.tryParse((json['changedAt'] as String?) ?? '') ?? DateTime.now(),
        notes: json['notes'] as String?,
      );

  Map<String, dynamic> toJson() => {'fromStageAr': fromStageAr, 'toStageAr': toStageAr, 'changedByName': changedByName, 'changedAt': changedAt.toIso8601String(), 'notes': notes};
}

class MissionPropertyEntry {
  final int id;
  final int? propertyId;
  final String? localId;
  final String? propertyNameAr;
  final String? propertyWqfNumber;
  final String enteredByName;
  final EntryStatus entryStatus;
  final double? dqsAtEntry;
  final DateTime entryStartedAt;
  final DateTime? entryCompletedAt;
  final String? reviewNotes;
  final String? reviewedByName;

  const MissionPropertyEntry({
    required this.id,
    required this.propertyId,
    required this.localId,
    required this.propertyNameAr,
    required this.propertyWqfNumber,
    required this.enteredByName,
    required this.entryStatus,
    required this.dqsAtEntry,
    required this.entryStartedAt,
    required this.entryCompletedAt,
    required this.reviewNotes,
    required this.reviewedByName,
  });

  factory MissionPropertyEntry.fromJson(Map<String, dynamic> json) => MissionPropertyEntry(
        id: (json['id'] as num?)?.toInt() ?? 0,
        propertyId: (json['propertyId'] as num?)?.toInt(),
        localId: json['localId'] as String?,
        propertyNameAr: json['propertyNameAr'] as String?,
        propertyWqfNumber: json['propertyWqfNumber'] as String?,
        enteredByName: (json['enteredByName'] as String?) ?? '',
        entryStatus: EntryStatus.values.firstWhere((e) => e.name.toLowerCase() == ((json['entryStatus'] as String?) ?? '').toLowerCase(), orElse: () => EntryStatus.inProgress),
        dqsAtEntry: (json['dqsAtEntry'] as num?)?.toDouble(),
        entryStartedAt: DateTime.tryParse((json['entryStartedAt'] as String?) ?? '') ?? DateTime.now(),
        entryCompletedAt: DateTime.tryParse((json['entryCompletedAt'] as String?) ?? ''),
        reviewNotes: json['reviewNotes'] as String?,
        reviewedByName: json['reviewedByName'] as String?,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'propertyId': propertyId,
        'localId': localId,
        'propertyNameAr': propertyNameAr,
        'propertyWqfNumber': propertyWqfNumber,
        'enteredByName': enteredByName,
        'entryStatus': entryStatus.name,
        'dqsAtEntry': dqsAtEntry,
        'entryStartedAt': entryStartedAt.toIso8601String(),
        'entryCompletedAt': entryCompletedAt?.toIso8601String(),
        'reviewNotes': reviewNotes,
        'reviewedByName': reviewedByName,
      };

  MissionPropertyEntry copyWith({EntryStatus? entryStatus, double? dqsAtEntry}) => MissionPropertyEntry(
        id: id,
        propertyId: propertyId,
        localId: localId,
        propertyNameAr: propertyNameAr,
        propertyWqfNumber: propertyWqfNumber,
        enteredByName: enteredByName,
        entryStatus: entryStatus ?? this.entryStatus,
        dqsAtEntry: dqsAtEntry ?? this.dqsAtEntry,
        entryStartedAt: entryStartedAt,
        entryCompletedAt: entryCompletedAt,
        reviewNotes: reviewNotes,
        reviewedByName: reviewedByName,
      );
}

class MissionProgressStats {
  final int targetCount;
  final int enteredCount;
  final int reviewedCount;
  final int approvedCount;
  final double progressPercent;
  final double averageDqsScore;
  final bool isOnSchedule;
  final int daysRemaining;
  final bool isOverdue;

  const MissionProgressStats({required this.targetCount, required this.enteredCount, required this.reviewedCount, required this.approvedCount, required this.progressPercent, required this.averageDqsScore, required this.isOnSchedule, required this.daysRemaining, required this.isOverdue});

  factory MissionProgressStats.fromJson(Map<String, dynamic> json) => MissionProgressStats(
        targetCount: (json['targetCount'] as num?)?.toInt() ?? 0,
        enteredCount: (json['enteredCount'] as num?)?.toInt() ?? 0,
        reviewedCount: (json['reviewedCount'] as num?)?.toInt() ?? 0,
        approvedCount: (json['approvedCount'] as num?)?.toInt() ?? 0,
        progressPercent: (json['progressPercent'] as num?)?.toDouble() ?? 0,
        averageDqsScore: (json['averageDqsScore'] as num?)?.toDouble() ?? 0,
        isOnSchedule: (json['isOnSchedule'] as bool?) ?? true,
        daysRemaining: (json['daysRemaining'] as num?)?.toInt() ?? 0,
        isOverdue: (json['isOverdue'] as bool?) ?? false,
      );

  Map<String, dynamic> toJson() => {
        'targetCount': targetCount,
        'enteredCount': enteredCount,
        'reviewedCount': reviewedCount,
        'approvedCount': approvedCount,
        'progressPercent': progressPercent,
        'averageDqsScore': averageDqsScore,
        'isOnSchedule': isOnSchedule,
        'daysRemaining': daysRemaining,
        'isOverdue': isOverdue,
      };
}

class ChecklistItem {
  final int id;
  final String questionAr;
  final String type;
  final bool requiredField;
  final List<String>? options;

  const ChecklistItem({required this.id, required this.questionAr, required this.type, required this.requiredField, required this.options});

  factory ChecklistItem.fromJson(Map<String, dynamic> json) => ChecklistItem(
        id: (json['id'] as num?)?.toInt() ?? 0,
        questionAr: (json['questionAr'] as String?) ?? '',
        type: (json['type'] as String?) ?? 'text',
        requiredField: (json['required'] as bool?) ?? false,
        options: (json['options'] as List?)?.map((e) => e.toString()).toList(),
      );

  Map<String, dynamic> toJson() => {'id': id, 'questionAr': questionAr, 'type': type, 'required': requiredField, 'options': options};
}

class ChecklistTemplate {
  final int id;
  final String templateName;
  final String? missionType;
  final List<ChecklistItem> items;

  const ChecklistTemplate({required this.id, required this.templateName, required this.missionType, required this.items});

  factory ChecklistTemplate.fromJson(Map<String, dynamic> json) {
    final rawItems = json['items'];
    List<dynamic> list;
    if (rawItems is String) {
      list = jsonDecode(rawItems) as List<dynamic>;
    } else if (rawItems is List) {
      list = rawItems;
    } else {
      list = <dynamic>[];
    }
    return ChecklistTemplate(
      id: (json['id'] as num?)?.toInt() ?? 0,
      templateName: (json['templateName'] as String?) ?? '',
      missionType: json['missionType'] as String?,
      items: list.map((e) => ChecklistItem.fromJson((e as Map).cast<String, dynamic>())).toList(),
    );
  }
}

class ChecklistItemResult {
  final int itemId;
  final String answer;
  final String? notes;

  const ChecklistItemResult({required this.itemId, required this.answer, required this.notes});

  Map<String, dynamic> toJson() => {'itemId': itemId, 'answer': answer, 'notes': notes};
}

class MissionDetail {
  final MissionListItem summary;
  final String? description;
  final String? targetArea;
  final UserBrief? assignedToUser;
  final UserBrief? assignedByUser;
  final UserBrief? reviewerUser;
  final List<MissionStageHistoryItem> stageHistory;
  final List<MissionPropertyEntry> propertyEntries;
  final ChecklistTemplate? checklistTemplate;
  final MissionProgressStats progressStats;
  final List<MissionStage> allowedNextStages;
  final bool canAccept;
  final bool canReject;
  final bool canCheckin;
  final bool canSubmitReview;
  final bool canApprove;
  final bool canSendBack;
  final bool canCancel;
  final bool canReassign;

  const MissionDetail({
    required this.summary,
    required this.description,
    required this.targetArea,
    required this.assignedToUser,
    required this.assignedByUser,
    required this.reviewerUser,
    required this.stageHistory,
    required this.propertyEntries,
    required this.checklistTemplate,
    required this.progressStats,
    required this.allowedNextStages,
    required this.canAccept,
    required this.canReject,
    required this.canCheckin,
    required this.canSubmitReview,
    required this.canApprove,
    required this.canSendBack,
    required this.canCancel,
    required this.canReassign,
  });

  factory MissionDetail.fromJson(Map<String, dynamic> json) => MissionDetail(
        summary: MissionListItem.fromJson(json),
        description: json['description'] as String?,
        targetArea: json['targetArea'] as String?,
        assignedToUser: json['assignedToUser'] == null ? null : UserBrief.fromJson((json['assignedToUser'] as Map).cast<String, dynamic>()),
        assignedByUser: json['assignedByUser'] == null ? null : UserBrief.fromJson((json['assignedByUser'] as Map).cast<String, dynamic>()),
        reviewerUser: json['reviewerUser'] == null ? null : UserBrief.fromJson((json['reviewerUser'] as Map).cast<String, dynamic>()),
        stageHistory: ((json['stageHistory'] as List?) ?? <dynamic>[]).map((e) => MissionStageHistoryItem.fromJson((e as Map).cast<String, dynamic>())).toList(),
        propertyEntries: ((json['propertyEntries'] as List?) ?? <dynamic>[]).map((e) => MissionPropertyEntry.fromJson((e as Map).cast<String, dynamic>())).toList(),
        checklistTemplate: json['checklistTemplate'] == null ? null : ChecklistTemplate.fromJson((json['checklistTemplate'] as Map).cast<String, dynamic>()),
        progressStats: MissionProgressStats.fromJson((json['progressStats'] as Map?)?.cast<String, dynamic>() ?? <String, dynamic>{}),
        allowedNextStages: ((json['allowedNextStages'] as List?) ?? <dynamic>[])
            .map((e) => _stageFromString(e.toString()))
            .toList(),
        canAccept: (json['canAccept'] as bool?) ?? false,
        canReject: (json['canReject'] as bool?) ?? false,
        canCheckin: (json['canCheckin'] as bool?) ?? false,
        canSubmitReview: (json['canSubmitReview'] as bool?) ?? false,
        canApprove: (json['canApprove'] as bool?) ?? false,
        canSendBack: (json['canSendBack'] as bool?) ?? false,
        canCancel: (json['canCancel'] as bool?) ?? false,
        canReassign: (json['canReassign'] as bool?) ?? false,
      );

  Map<String, dynamic> toJson() => {
        ...summary.toJson(),
        'description': description,
        'targetArea': targetArea,
        'assignedToUser': assignedToUser?.toJson(),
        'assignedByUser': assignedByUser?.toJson(),
        'reviewerUser': reviewerUser?.toJson(),
        'stageHistory': stageHistory.map((e) => e.toJson()).toList(),
        'propertyEntries': propertyEntries.map((e) => e.toJson()).toList(),
        'checklistTemplate': checklistTemplate == null ? null : {'id': checklistTemplate!.id, 'templateName': checklistTemplate!.templateName, 'missionType': checklistTemplate!.missionType, 'items': checklistTemplate!.items.map((i) => i.toJson()).toList()},
        'progressStats': progressStats.toJson(),
        'allowedNextStages': allowedNextStages.map((e) => e.name).toList(),
        'canAccept': canAccept,
        'canReject': canReject,
        'canCheckin': canCheckin,
        'canSubmitReview': canSubmitReview,
        'canApprove': canApprove,
        'canSendBack': canSendBack,
        'canCancel': canCancel,
        'canReassign': canReassign,
      };

  MissionDetail copyWith({MissionListItem? summary, MissionProgressStats? progressStats}) => MissionDetail(
        summary: summary ?? this.summary,
        description: description,
        targetArea: targetArea,
        assignedToUser: assignedToUser,
        assignedByUser: assignedByUser,
        reviewerUser: reviewerUser,
        stageHistory: stageHistory,
        propertyEntries: propertyEntries,
        checklistTemplate: checklistTemplate,
        progressStats: progressStats ?? this.progressStats,
        allowedNextStages: allowedNextStages,
        canAccept: canAccept,
        canReject: canReject,
        canCheckin: canCheckin,
        canSubmitReview: canSubmitReview,
        canApprove: canApprove,
        canSendBack: canSendBack,
        canCancel: canCancel,
        canReassign: canReassign,
      );
}

class AdvanceStageRequest {
  final String toStage;
  final String? notes;
  final double? checkinLat;
  final double? checkinLng;

  const AdvanceStageRequest({required this.toStage, required this.notes, required this.checkinLat, required this.checkinLng});

  Map<String, dynamic> toJson() => {'toStage': toStage, 'notes': notes, 'checkinLat': checkinLat, 'checkinLng': checkinLng};
}

class MissionCheckinRequest {
  final double lat;
  final double lng;
  final DateTime checkinAt;

  const MissionCheckinRequest({required this.lat, required this.lng, required this.checkinAt});

  Map<String, dynamic> toJson() => {'lat': lat, 'lng': lng, 'checkinAt': checkinAt.toIso8601String()};
}

class InspectorStats {
  final int todayCompleted;
  final int monthCompleted;
  final int pendingMissions;
  final int inProgressMissions;
  final double avgDqsScore;
  final int totalPropertiesEntered;
  final DateTime? lastSyncAt;

  const InspectorStats({required this.todayCompleted, required this.monthCompleted, required this.pendingMissions, required this.inProgressMissions, required this.avgDqsScore, required this.totalPropertiesEntered, required this.lastSyncAt});

  factory InspectorStats.fromJson(Map<String, dynamic> json) => InspectorStats(
        todayCompleted: (json['todayCompleted'] as num?)?.toInt() ?? 0,
        monthCompleted: (json['monthCompleted'] as num?)?.toInt() ?? 0,
        pendingMissions: (json['pendingMissions'] as num?)?.toInt() ?? 0,
        inProgressMissions: (json['inProgressMissions'] as num?)?.toInt() ?? 0,
        avgDqsScore: (json['avgDqsScore'] as num?)?.toDouble() ?? 0,
        totalPropertiesEntered: (json['totalPropertiesEntered'] as num?)?.toInt() ?? 0,
        lastSyncAt: DateTime.tryParse((json['lastSyncAt'] as String?) ?? ''),
      );
}

class PagedResult<T> {
  final List<T> items;
  final int totalCount;
  final int page;
  final int pageSize;

  const PagedResult({required this.items, required this.totalCount, required this.page, required this.pageSize});
}

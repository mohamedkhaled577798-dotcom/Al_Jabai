class SyncItem {
  final int? serverId;
  final String localId;
  final String jsonData;
  final DateTime? modifiedAt;

  SyncItem({
    this.serverId,
    required this.localId,
    required this.jsonData,
    this.modifiedAt,
  });

  factory SyncItem.fromJson(Map<String, dynamic> json) {
    return SyncItem(
      serverId: json['serverId'],
      localId: json['localId'],
      jsonData: json['jsonData'],
      modifiedAt: json['modifiedAt'] != null ? DateTime.parse(json['modifiedAt']) : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'serverId': serverId,
      'localId': localId,
      'jsonData': jsonData,
      'modifiedAt': modifiedAt?.toIso8601String(),
    };
  }
}

class InitialSyncData {
  final List<dynamic> governorates;
  final List<dynamic> propertyTypes;

  InitialSyncData({required this.governorates, required this.propertyTypes});

  factory InitialSyncData.fromJson(Map<String, dynamic> json) {
    return InitialSyncData(
      governorates: json['governorates'] ?? [],
      propertyTypes: json['propertyTypes'] ?? [],
    );
  }
}

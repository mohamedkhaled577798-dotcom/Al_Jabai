class PropertyModel {
  final int id;
  final String wqfNumber;
  final String propertyName;
  final int? governorateId;
  final String? governorateName;
  final int propertyType;
  final String? propertyTypeDisplay;
  final double? totalAreaSqm;
  final double? latitude;
  final double? longitude;
  final double dqsScore;
  final int approvalStage;
  final String? approvalStageDisplay;

  PropertyModel({
    required this.id,
    required this.wqfNumber,
    required this.propertyName,
    this.governorateId,
    this.governorateName,
    required this.propertyType,
    this.propertyTypeDisplay,
    this.totalAreaSqm,
    this.latitude,
    this.longitude,
    required this.dqsScore,
    required this.approvalStage,
    this.approvalStageDisplay,
  });

  factory PropertyModel.fromJson(Map<String, dynamic> json) {
    return PropertyModel(
      id: json['id'],
      wqfNumber: json['wqfNumber'],
      propertyName: json['propertyName'],
      governorateId: json['governorateId'],
      governorateName: json['governorateName'],
      propertyType: json['propertyType'],
      propertyTypeDisplay: json['propertyTypeDisplay'],
      totalAreaSqm: json['totalAreaSqm']?.toDouble(),
      latitude: json['latitude']?.toDouble(),
      longitude: json['longitude']?.toDouble(),
      dqsScore: json['dqsScore']?.toDouble() ?? 0.0,
      approvalStage: json['approvalStage'],
      approvalStageDisplay: json['approvalStageDisplay'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'wqfNumber': wqfNumber,
      'propertyName': propertyName,
      'governorateId': governorateId,
      'propertyType': propertyType,
      'totalAreaSqm': totalAreaSqm,
      'latitude': latitude,
      'longitude': longitude,
      'dqsScore': dqsScore,
      'approvalStage': approvalStage,
    };
  }
}

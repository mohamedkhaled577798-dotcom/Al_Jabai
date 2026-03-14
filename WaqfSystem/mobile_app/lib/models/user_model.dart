class UserModel {
  final int id;
  final String fullNameAr;
  final String role;
  final String token;
  final int? governorateId;

  UserModel({
    required this.id,
    required this.fullNameAr,
    required this.role,
    required this.token,
    this.governorateId,
  });

  factory UserModel.fromJson(Map<String, dynamic> json) {
    return UserModel(
      id: json['id'],
      fullNameAr: json['fullNameAr'],
      role: json['role'] ?? 'FIELD_INSPECTOR',
      token: json['token'] ?? '',
      governorateId: json['governorateId'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'fullNameAr': fullNameAr,
      'role': role,
      'token': token,
      'governorateId': governorateId,
    };
  }
}

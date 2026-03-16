import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/partnership_models.dart';

class PartnershipApiException implements Exception {
  final String message;
  final int? statusCode;

  PartnershipApiException(this.message, {this.statusCode});

  @override
  String toString() => 'PartnershipApiException($statusCode): $message';
}

class PartnershipService {
  final String baseUrl;
  final String token;

  PartnershipService({required this.baseUrl, required this.token});

  Map<String, String> get _headers => <String, String>{
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      };

  Future<List<PartnershipDetail>> getForProperty(int propertyId) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/partnerships?propertyId=$propertyId');
    final response = await http.get(uri, headers: _headers);
    final data = _decodeApiResponse(response);
    final list = (data as List<dynamic>? ?? <dynamic>[])
        .map((e) => PartnershipDetail.fromJson((e as Map<String, dynamic>? ?? <String, dynamic>{})))
        .toList();
    return list;
  }

  Future<PartnershipDetail> getById(int id) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/partnerships/$id');
    final response = await http.get(uri, headers: _headers);
    final data = _decodeApiResponse(response);
    return PartnershipDetail.fromJson((data as Map<String, dynamic>? ?? <String, dynamic>{}));
  }

  Future<RevenueCalculationResult> previewRevenue(
    int partnershipId,
    double total, {
    double expenses = 0,
    String? distributionType,
    String? season,
  }) async {
    final uri = Uri.parse(
      '$baseUrl/api/v1/mobile/partnerships/$partnershipId/revenue-preview?total=$total&expenses=$expenses${distributionType != null ? '&distributionType=$distributionType' : ''}${season != null ? '&season=$season' : ''}',
    );
    final response = await http.get(uri, headers: _headers);
    final data = _decodeApiResponse(response);
    return RevenueCalculationResult.fromJson((data as Map<String, dynamic>? ?? <String, dynamic>{}));
  }

  Future<RevenueDistribution> recordDistribution(RevenueDistributionCreateRequest req) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/partnerships/${req.partnershipId}/record-revenue');
    final response = await http.post(uri, headers: _headers, body: jsonEncode(req.toJson()));
    final data = _decodeApiResponse(response);
    return RevenueDistribution.fromJson((data as Map<String, dynamic>? ?? <String, dynamic>{}));
  }

  Future<List<RevenueDistribution>> getDistributions(int partnershipId) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/partnerships/$partnershipId/distributions');
    final response = await http.get(uri, headers: _headers);
    final data = _decodeApiResponse(response);
    final list = (data as List<dynamic>? ?? <dynamic>[])
        .map((e) => RevenueDistribution.fromJson((e as Map<String, dynamic>? ?? <String, dynamic>{})))
        .toList();
    return list;
  }

  Future<Map<String, dynamic>> addExpense({
    required int partnershipId,
    required String periodLabel,
    required DateTime periodStartDate,
    required DateTime periodEndDate,
    required String expenseType,
    required double amount,
    String? referenceNo,
    String? notes,
  }) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/partnerships/$partnershipId/expenses');
    final response = await http.post(
      uri,
      headers: _headers,
      body: jsonEncode(<String, dynamic>{
        'partnershipId': partnershipId,
        'periodLabel': periodLabel,
        'periodStartDate': periodStartDate.toIso8601String(),
        'periodEndDate': periodEndDate.toIso8601String(),
        'expenseType': expenseType,
        'amount': amount,
        'referenceNo': referenceNo,
        'notes': notes,
      }),
    );
    final data = _decodeApiResponse(response);
    return (data as Map<String, dynamic>? ?? <String, dynamic>{});
  }

  Future<List<Map<String, dynamic>>> getExpenses(int partnershipId, {DateTime? from, DateTime? to}) async {
    final uri = Uri.parse(
      '$baseUrl/api/v1/mobile/partnerships/$partnershipId/expenses${from != null || to != null ? '?' : ''}${from != null ? 'from=${from.toIso8601String()}' : ''}${from != null && to != null ? '&' : ''}${to != null ? 'to=${to.toIso8601String()}' : ''}',
    );
    final response = await http.get(uri, headers: _headers);
    final data = _decodeApiResponse(response);
    return (data as List<dynamic>? ?? <dynamic>[])
        .map((e) => (e as Map<String, dynamic>? ?? <String, dynamic>{}))
        .toList();
  }

  Future<bool> logContact(int partnershipId, String contactType, String notes) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/partnerships/$partnershipId/contact-log');
    final response = await http.post(
      uri,
      headers: _headers,
      body: jsonEncode(<String, dynamic>{
        'contactType': contactType,
        'notes': notes,
      }),
    );
    _decodeApiResponse(response);
    return true;
  }

  Future<List<PartnershipDetail>> getExpiring(int days) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/partnerships/expiring?days=$days');
    final response = await http.get(uri, headers: _headers);
    final data = _decodeApiResponse(response);
    final list = (data as List<dynamic>? ?? <dynamic>[])
        .map((e) => PartnershipDetail.fromJson((e as Map<String, dynamic>? ?? <String, dynamic>{})))
        .toList();
    return list;
  }

  dynamic _decodeApiResponse(http.Response response) {
    final body = response.body.isEmpty ? '{}' : response.body;
    final parsed = jsonDecode(body) as Map<String, dynamic>? ?? <String, dynamic>{};

    final success = (parsed['success'] as bool?) ?? false;
    final message = (parsed['message'] as String?) ?? 'حدث خطأ غير متوقع';

    if (response.statusCode < 200 || response.statusCode >= 300 || !success) {
      throw PartnershipApiException(message, statusCode: response.statusCode);
    }

    return parsed['data'];
  }
}

import 'dart:convert';

import 'package:http/http.dart' as http;
import '../models/smart_collection_models.dart';

class MobileApiException implements Exception {
  final String message;
  final int? statusCode;

  const MobileApiException(this.message, {this.statusCode});

  @override
  String toString() => 'MobileApiException: $message';
}

class SmartCollectionService {
  final String baseUrl;
  final String Function() getToken;

  SmartCollectionService({required this.baseUrl, required this.getToken});

  Map<String, String> _headers() => {
        'Authorization': 'Bearer ${getToken()}',
        'Content-Type': 'application/json',
      };

  T _extractWrapped<T>(Map<String, dynamic> json, T Function(dynamic data) parser) {
    final success = (json['success'] as bool?) ?? false;
    if (!success) {
      throw MobileApiException((json['message'] as String?) ?? 'فشل الطلب');
    }
    return parser(json['data']);
  }

  Future<List<SmartSuggestionDto>> getSuggestions(String period) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/collection/suggestions').replace(queryParameters: {'period': period});
    final res = await http.get(uri, headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;

    return _extractWrapped(body, (data) {
      final list = (data as List?) ?? const <dynamic>[];
      return list.map((e) => SmartSuggestionDto.fromJson((e as Map).cast<String, dynamic>())).toList();
    });
  }

  Future<TodayDashboardDto> getTodayDashboard() async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/collection/today'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return _extractWrapped(body, (data) => TodayDashboardDto.fromJson((data as Map).cast<String, dynamic>()));
  }

  Future<Map<String, dynamic>> quickCollect(QuickCollectRequest req) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/collection/quick'), headers: _headers(), body: jsonEncode(req.toJson()));
    if (res.statusCode == 409) {
      final body409 = jsonDecode(res.body) as Map<String, dynamic>;
      throw MobileApiException((body409['message'] as String?) ?? 'العنصر مقفول', statusCode: 409);
    }

    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return _extractWrapped(body, (data) => (data as Map).cast<String, dynamic>());
  }

  Future<BatchCollectResult> batchCollect(BatchCollectRequest req) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/collection/batch'), headers: _headers(), body: jsonEncode(req.toJson()));
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return _extractWrapped(body, (data) => BatchCollectResult.fromJson((data as Map).cast<String, dynamic>()));
  }

  Future<PagedResult<SearchResultDto>> search(String query, int page) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/collection/search').replace(queryParameters: {'q': query, 'page': '$page'});
    final res = await http.get(uri, headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;

    return _extractWrapped(body, (data) {
      final d = (data as Map).cast<String, dynamic>();
      final items = ((d['items'] as List?) ?? const <dynamic>[])
          .map((e) => SearchResultDto.fromJson((e as Map).cast<String, dynamic>()))
          .toList();

      return PagedResult<SearchResultDto>(
        items: items,
        totalCount: (d['totalCount'] as num?)?.toInt() ?? 0,
        page: (d['page'] as num?)?.toInt() ?? page,
        pageSize: (d['pageSize'] as num?)?.toInt() ?? 20,
      );
    });
  }

  Future<Map<String, dynamic>> checkCollision({required int propertyId, required String level, int? floorId, int? unitId, required String period}) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/collection/check-collision').replace(queryParameters: {
      'propertyId': '$propertyId',
      'level': level,
      'floorId': floorId?.toString() ?? '',
      'unitId': unitId?.toString() ?? '',
      'period': period,
    });

    final res = await http.get(uri, headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return _extractWrapped(body, (data) => (data as Map).cast<String, dynamic>());
  }

  Future<VarianceAlertDto> checkVariance(int? contractId, double amount) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/collection/check-variance').replace(queryParameters: {
      'contractId': contractId?.toString() ?? '',
      'amount': amount.toString(),
    });

    final res = await http.get(uri, headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return _extractWrapped(body, (data) => VarianceAlertDto.fromJson((data as Map).cast<String, dynamic>()));
  }

  Future<PropertyStructureDto> getStructure(int propertyId, String period) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/collection/structure/$propertyId').replace(queryParameters: {'period': period});
    final res = await http.get(uri, headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return _extractWrapped(body, (data) => PropertyStructureDto.fromJson((data as Map).cast<String, dynamic>()));
  }
}

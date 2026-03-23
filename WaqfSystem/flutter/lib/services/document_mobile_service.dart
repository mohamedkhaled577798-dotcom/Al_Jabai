import 'dart:convert';
import 'dart:io';

import 'package:dio/dio.dart';
import 'package:http/http.dart' as http;
import '../models/document_models.dart';

class DocumentMobileService {
  final String baseUrl;
  final String Function() getToken;
  final Dio _dio;

  DocumentMobileService({required this.baseUrl, required this.getToken}) : _dio = Dio();

  Map<String, String> _headers() => {
        'Authorization': 'Bearer ${getToken()}',
        'Content-Type': 'application/json',
      };

  dynamic _extractData(Map<String, dynamic> json) {
    final success = (json['success'] as bool?) ?? false;
    if (!success) {
      throw Exception((json['message'] as String?) ?? 'فشل الطلب');
    }
    return json['data'];
  }

  Future<PropertyDocumentSummary> getPropertySummary(int propertyId) async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/documents/$propertyId'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as Map).cast<String, dynamic>();
    return PropertyDocumentSummary.fromJson((data['summary'] as Map).cast<String, dynamic>());
  }

  Future<List<PropertyDocumentListItem>> getPropertyDocuments(int propertyId) async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/documents/$propertyId'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as Map).cast<String, dynamic>();
    final list = (data['documents'] as List?) ?? <dynamic>[];
    return list.map((e) => PropertyDocumentListItem.fromJson((e as Map).cast<String, dynamic>())).toList();
  }

  Future<PropertyDocumentDetail> getDetail(int id) async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/documents/detail/$id'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as Map).cast<String, dynamic>();
    return PropertyDocumentDetail.fromJson(data);
  }

  Future<int> uploadDocument(UploadDocumentRequest req, File file, void Function(double) onProgress) async {
    final formDataMap = req.toFormData();
    final formData = FormData.fromMap({
      ...formDataMap,
      'file': await MultipartFile.fromFile(file.path, filename: file.uri.pathSegments.last),
    });

    final response = await _dio.post(
      '$baseUrl/api/v1/mobile/documents/upload',
      data: formData,
      options: Options(headers: {'Authorization': 'Bearer ${getToken()}'}),
      onSendProgress: (sent, total) {
        if (total > 0) {
          onProgress(sent / total);
        }
      },
    );

    final body = (response.data as Map).cast<String, dynamic>();
    final data = (_extractData(body) as Map).cast<String, dynamic>();
    return (data['documentId'] as num?)?.toInt() ?? 0;
  }

  Future<int> uploadNewVersion(int documentId, File file, String? notes, DateTime? newExpiry) async {
    final formData = FormData.fromMap({
      'file': await MultipartFile.fromFile(file.path, filename: file.uri.pathSegments.last),
      if (notes != null) 'notes': notes,
      if (newExpiry != null) 'expiryDate': newExpiry.toIso8601String(),
    });

    final response = await _dio.post(
      '$baseUrl/api/v1/mobile/documents/$documentId/upload-version',
      data: formData,
      options: Options(headers: {'Authorization': 'Bearer ${getToken()}'}),
    );

    final body = (response.data as Map).cast<String, dynamic>();
    final data = (_extractData(body) as Map).cast<String, dynamic>();
    return (data['versionId'] as num?)?.toInt() ?? 0;
  }

  Future<List<DocumentVersion>> getVersions(int id) async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/documents/$id/versions'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as List?) ?? <dynamic>[];
    return data.map((e) => DocumentVersion.fromJson((e as Map).cast<String, dynamic>())).toList();
  }

  Future<List<DocumentAuditTrailItem>> getAuditTrail(int id) async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/documents/$id/audit-trail'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as List?) ?? <dynamic>[];
    return data.map((e) => DocumentAuditTrailItem.fromJson((e as Map).cast<String, dynamic>())).toList();
  }

  Future<bool> recordDownload(int id) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/documents/$id/record-download?versionId=0'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    _extractData(body);
    return true;
  }

  Future<List<DocumentAlert>> getAlerts() async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/documents/alerts'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as Map).cast<String, dynamic>();
    final all = <DocumentAlert>[];
    for (final key in ['expired', 'expiring30', 'expiring90']) {
      final arr = (data[key] as List?) ?? <dynamic>[];
      all.addAll(arr.map((e) => DocumentAlert.fromJson((e as Map).cast<String, dynamic>())));
    }
    return all;
  }

  Future<int> getAlertCount() async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/documents/alert-count'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as Map).cast<String, dynamic>();
    return (data['count'] as num?)?.toInt() ?? 0;
  }

  Future<bool> markAlertRead(int alertId) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/documents/alerts/$alertId/read'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    _extractData(body);
    return true;
  }

  Future<List<DocumentType>> getDocumentTypes() async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/documents/types'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as List?) ?? <dynamic>[];
    return data.map((e) => DocumentType.fromJson((e as Map).cast<String, dynamic>())).toList();
  }
}

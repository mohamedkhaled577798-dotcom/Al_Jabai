import 'dart:convert';

import 'package:http/http.dart' as http;
import '../models/mission_models.dart';

class MissionMobileService {
  final String baseUrl;
  final String Function() getToken;

  MissionMobileService({required this.baseUrl, required this.getToken});

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

  Future<PagedResult<MissionListItem>> getMissions({String? stage, DateTime? from, DateTime? to, int page = 1, int pageSize = 20}) async {
    final uri = Uri.parse('$baseUrl/api/v1/mobile/missions').replace(queryParameters: {
      if (stage != null) 'stage': stage,
      if (from != null) 'from': from.toIso8601String(),
      if (to != null) 'to': to.toIso8601String(),
      'page': '$page',
      'pageSize': '$pageSize',
    });

    final res = await http.get(uri, headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as Map).cast<String, dynamic>();

    final items = ((data['items'] as List?) ?? <dynamic>[])
        .map((e) => MissionListItem.fromJson((e as Map).cast<String, dynamic>()))
        .toList();

    return PagedResult<MissionListItem>(
      items: items,
      totalCount: (data['totalCount'] as num?)?.toInt() ?? 0,
      page: (data['page'] as num?)?.toInt() ?? page,
      pageSize: (data['pageSize'] as num?)?.toInt() ?? pageSize,
    );
  }

  Future<List<MissionListItem>> getTodayMissions() async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/missions/today'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as List?) ?? <dynamic>[];
    return data.map((e) => MissionListItem.fromJson((e as Map).cast<String, dynamic>())).toList();
  }

  Future<MissionDetail> getMissionDetail(int id) async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/missions/$id'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return MissionDetail.fromJson((_extractData(body) as Map).cast<String, dynamic>());
  }

  Future<Map<String, dynamic>> acceptMission(int id) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/missions/$id/accept'), headers: _headers(), body: '{}');
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return (_extractData(body) as Map).cast<String, dynamic>();
  }

  Future<Map<String, dynamic>> rejectMission(int id, String reason) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/missions/$id/reject'), headers: _headers(), body: jsonEncode({'reason': reason}));
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return (_extractData(body) as Map).cast<String, dynamic>();
  }

  Future<Map<String, dynamic>> checkin(int id, double lat, double lng) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/missions/$id/checkin'), headers: _headers(), body: jsonEncode({'lat': lat, 'lng': lng, 'checkinAt': DateTime.now().toIso8601String()}));
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return (_extractData(body) as Map).cast<String, dynamic>();
  }

  Future<Map<String, dynamic>> advanceStage(int id, AdvanceStageRequest req) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/missions/$id/advance-stage'), headers: _headers(), body: jsonEncode(req.toJson()));
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return (_extractData(body) as Map).cast<String, dynamic>();
  }

  Future<Map<String, dynamic>> submitForReview(int id, String notes) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/missions/$id/submit-review'), headers: _headers(), body: jsonEncode({'inspectorNotes': notes}));
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return (_extractData(body) as Map).cast<String, dynamic>();
  }

  Future<Map<String, dynamic>> recordPropertyEntry(int id, {int? propertyId, String? localId}) async {
    final res = await http.post(Uri.parse('$baseUrl/api/v1/mobile/missions/$id/properties'), headers: _headers(), body: jsonEncode({'propertyId': propertyId, 'localId': localId}));
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return (_extractData(body) as Map).cast<String, dynamic>();
  }

  Future<ChecklistTemplate?> getChecklist(int missionId) async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/missions/$missionId/checklist'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = _extractData(body);
    if (data == null) return null;
    return ChecklistTemplate.fromJson((data as Map).cast<String, dynamic>());
  }

  Future<Map<String, dynamic>> submitChecklist(int missionId, int templateId, List<ChecklistItemResult> results) async {
    final res = await http.post(
      Uri.parse('$baseUrl/api/v1/mobile/missions/$missionId/checklist'),
      headers: _headers(),
      body: jsonEncode({'templateId': templateId, 'results': results.map((e) => e.toJson()).toList()}),
    );
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return (_extractData(body) as Map).cast<String, dynamic>();
  }

  Future<List<MissionStageHistoryItem>> getStageHistory(int missionId) async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/missions/$missionId/stage-history'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    final data = (_extractData(body) as List?) ?? <dynamic>[];
    return data.map((e) => MissionStageHistoryItem.fromJson((e as Map).cast<String, dynamic>())).toList();
  }

  Future<InspectorStats> getStats() async {
    final res = await http.get(Uri.parse('$baseUrl/api/v1/mobile/inspector/stats'), headers: _headers());
    final body = jsonDecode(res.body) as Map<String, dynamic>;
    return InspectorStats.fromJson((_extractData(body) as Map).cast<String, dynamic>());
  }
}

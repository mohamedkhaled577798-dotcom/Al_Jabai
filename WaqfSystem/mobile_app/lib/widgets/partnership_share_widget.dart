import 'package:flutter/material.dart';

import '../models/partnership_models.dart';

class PartnershipShareWidget extends StatelessWidget {
  final PartnershipDetail partnership;
  final double? width;

  const PartnershipShareWidget({
    super.key,
    required this.partnership,
    this.width,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: width,
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: _buildByType(context),
        ),
      ),
    );
  }

  Widget _buildByType(BuildContext context) {
    switch (partnership.partnershipType) {
      case PartnershipType.revenuePercent:
        return _buildRevenuePercent();
      case PartnershipType.floorOwnership:
        return _buildFloorOwnership();
      case PartnershipType.unitOwnership:
        return _buildUnitOwnership();
      case PartnershipType.usufructRight:
        return _buildUsufructRight();
      case PartnershipType.timedPartnership:
        return _buildTimedPartnership();
      case PartnershipType.landPercent:
        return _buildLandPercent();
      case PartnershipType.harvestShare:
        return _buildHarvestShare();
    }
  }

  Widget _buildRevenuePercent() {
    final value = (partnership.waqfSharePercent / 100).clamp(0.0, 1.0);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.center,
      children: <Widget>[
        SizedBox(
          width: 96,
          height: 96,
          child: CircularProgressIndicator(
            value: value,
            strokeWidth: 10,
            backgroundColor: Colors.grey.shade300,
            color: Colors.green,
          ),
        ),
        const SizedBox(height: 8),
        Text('حصة الوقف ${partnership.waqfSharePercent.toStringAsFixed(1)}%'),
      ],
    );
  }

  Widget _buildFloorOwnership() {
    final floors = partnership.ownedFloorsList;
    return Wrap(
      spacing: 6,
      runSpacing: 6,
      children: List<Widget>.generate(
        floors.isEmpty ? 4 : floors.length,
        (int index) => Container(
          width: 44,
          height: 24,
          alignment: Alignment.center,
          decoration: BoxDecoration(
            color: Colors.green,
            borderRadius: BorderRadius.circular(6),
          ),
          child: Text(
            floors.isEmpty ? '${index + 1}' : '${floors[index]}',
            style: const TextStyle(color: Colors.white, fontSize: 12),
          ),
        ),
      ),
    );
  }

  Widget _buildUnitOwnership() {
    final units = partnership.ownedUnitsList;
    return Wrap(
      spacing: 6,
      runSpacing: 6,
      children: List<Widget>.generate(
        units.isEmpty ? 6 : units.length,
        (int index) => Chip(
          label: Text(units.isEmpty ? 'وحدة ${index + 1}' : 'وحدة ${units[index]}'),
          backgroundColor: Colors.green.shade100,
        ),
      ),
    );
  }

  Widget _buildUsufructRight() {
    final totalDays = 365.0;
    final remaining = (partnership.daysUntilExpiry ?? 0).toDouble();
    final elapsedRatio = (1 - (remaining / totalDays)).clamp(0.0, 1.0);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        const Text('مؤشر مدة الانتفاع'),
        const SizedBox(height: 8),
        LinearProgressIndicator(value: elapsedRatio, color: Colors.orange),
        const SizedBox(height: 6),
        Text('المتبقي: ${partnership.daysUntilExpiry ?? 0} يوم'),
      ],
    );
  }

  Widget _buildTimedPartnership() {
    final days = partnership.daysUntilExpiry;
    final Color color = days == null
        ? Colors.blueGrey
        : days < 0
            ? Colors.red
            : days <= 30
                ? Colors.orange
                : Colors.green;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        Text('المدة المتبقية: ${days ?? '-'} يوم', style: TextStyle(color: color, fontWeight: FontWeight.bold)),
        const SizedBox(height: 8),
        LinearProgressIndicator(value: days == null ? 0 : (days / 365).clamp(0.0, 1.0), color: color),
      ],
    );
  }

  Widget _buildLandPercent() {
    final value = (partnership.waqfSharePercent / 100).clamp(0.0, 1.0);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        const Text('نسبة الأرض'),
        const SizedBox(height: 8),
        Stack(
          children: <Widget>[
            Container(height: 22, decoration: BoxDecoration(color: Colors.grey.shade300, borderRadius: BorderRadius.circular(8))),
            Container(height: 22, width: 220 * value, decoration: BoxDecoration(color: Colors.green, borderRadius: BorderRadius.circular(8))),
          ],
        ),
        const SizedBox(height: 6),
        Text('حصة الوقف ${partnership.waqfSharePercent.toStringAsFixed(1)}%'),
      ],
    );
  }

  Widget _buildHarvestShare() {
    return Row(
      children: <Widget>[
        const Text('🌾', style: TextStyle(fontSize: 28)),
        const SizedBox(width: 8),
        Text('نسبة الوقف من المحصول ${partnership.waqfSharePercent.toStringAsFixed(1)}%'),
      ],
    );
  }
}

import 'package:flutter/material.dart';
import '../models/smart_collection_models.dart';

class SuggestionCard extends StatelessWidget {
  final SmartSuggestionDto item;
  final VoidCallback? onTap;

  const SuggestionCard({super.key, required this.item, this.onTap});

  @override
  Widget build(BuildContext context) {
    return Card(
      color: item.isLocked ? Colors.grey.shade200 : Colors.white,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12), side: BorderSide(color: item.urgencyColor, width: 1.4)),
      child: InkWell(
        onTap: item.isActionable ? onTap : null,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                    decoration: BoxDecoration(color: item.urgencyColor.withOpacity(.12), borderRadius: BorderRadius.circular(20)),
                    child: Text(item.urgencyLabelAr, style: TextStyle(color: item.urgencyColor, fontWeight: FontWeight.w700)),
                  ),
                  const Spacer(),
                  if (item.isLocked) const Icon(Icons.lock, color: Colors.grey),
                ],
              ),
              const SizedBox(height: 8),
              Text('${item.propertyNameAr} / ${item.unitNumber ?? '-'}', style: const TextStyle(fontWeight: FontWeight.bold)),
              const SizedBox(height: 4),
              Text(item.tenantNameAr ?? '-', style: const TextStyle(color: Colors.black54)),
              const SizedBox(height: 6),
              Text('${item.expectedAmount.toStringAsFixed(0)} د.ع', style: const TextStyle(fontWeight: FontWeight.bold)),
            ],
          ),
        ),
      ),
    );
  }
}

class AmountChipRow extends StatelessWidget {
  final List<AmountChipDto> chips;
  final double? selectedAmount;
  final ValueChanged<double> onSelected;

  const AmountChipRow({super.key, required this.chips, required this.selectedAmount, required this.onSelected});

  @override
  Widget build(BuildContext context) {
    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: chips.map((chip) {
        final active = selectedAmount != null && (selectedAmount! - chip.amount).abs() < 0.01;
        return ChoiceChip(
          selected: active,
          label: Text(chip.label),
          onSelected: (_) => onSelected(chip.amount),
        );
      }).toList(),
    );
  }
}

class VarianceIndicator extends StatelessWidget {
  final VarianceAlertDto variance;

  const VarianceIndicator({super.key, required this.variance});

  @override
  Widget build(BuildContext context) {
    final percent = variance.variancePercent.abs();
    final widthFactor = (1 - (percent / 100)).clamp(0.05, 1.0);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        ClipRRect(
          borderRadius: BorderRadius.circular(6),
          child: Container(
            height: 10,
            color: Colors.grey.shade200,
            child: AnimatedFractionallySizedBox(
              duration: const Duration(milliseconds: 300),
              curve: Curves.ease,
              alignment: Alignment.centerRight,
              widthFactor: widthFactor,
              child: Container(color: variance.alertColor),
            ),
          ),
        ),
        const SizedBox(height: 6),
        Text('الفارق: ${variance.variance.toStringAsFixed(0)} د.ع (${variance.variancePercent.toStringAsFixed(1)}%)', style: TextStyle(color: variance.alertColor, fontWeight: FontWeight.w600)),
      ],
    );
  }
}

class CollisionBadge extends StatelessWidget {
  final bool hasCollision;
  final String message;

  const CollisionBadge({super.key, required this.hasCollision, required this.message});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(8),
        color: hasCollision ? const Color(0xFFFFE4E4) : const Color(0xFFE8F5E8),
      ),
      child: Row(
        children: [
          Icon(hasCollision ? Icons.lock : Icons.check_circle, color: hasCollision ? const Color(0xFFB3261E) : const Color(0xFF2E7D32)),
          const SizedBox(width: 8),
          Expanded(child: Text(message, style: TextStyle(color: hasCollision ? const Color(0xFFB3261E) : const Color(0xFF2E7D32), fontWeight: FontWeight.w600))),
        ],
      ),
    );
  }
}

class BatchItemRow extends StatelessWidget {
  final SmartSuggestionDto item;
  final bool selected;
  final double amount;
  final ValueChanged<bool> onSelected;
  final ValueChanged<double> onAmountChanged;

  const BatchItemRow({super.key, required this.item, required this.selected, required this.amount, required this.onSelected, required this.onAmountChanged});

  @override
  Widget build(BuildContext context) {
    return Opacity(
      opacity: item.isLocked ? .55 : 1,
      child: Row(
        children: [
          Checkbox(value: selected, onChanged: item.isLocked ? null : (v) => onSelected(v ?? false)),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('${item.propertyNameAr} / ${item.unitNumber ?? '-'}', style: const TextStyle(fontWeight: FontWeight.w700)),
                Text(item.tenantNameAr ?? '-', style: const TextStyle(color: Colors.black54)),
              ],
            ),
          ),
          SizedBox(
            width: 120,
            child: TextFormField(
              initialValue: amount.toStringAsFixed(0),
              enabled: !item.isLocked,
              keyboardType: TextInputType.number,
              onChanged: (v) => onAmountChanged(double.tryParse(v) ?? 0),
              decoration: const InputDecoration(isDense: true, suffixText: 'د.ع'),
            ),
          ),
        ],
      ),
    );
  }
}

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../providers/flash_control_provider.dart';
import '../../utils/constants.dart';


class FlashIntensitySlider extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Consumer<FlashControlProvider>(
      builder: (context, flashControlProvider, child) {
        return Container(
          width: MediaQuery.of(context).size.width > 800
              ? kWebWidth
              : kMobileWidth,
          child: Row(
            children: [
              Icon(Icons.flash_off, color: Colors.grey),
              Expanded(
                child: Slider(
                  value: flashControlProvider.flashIntensity.toDouble(),
                  min: 0,
                  max: 100,
                  divisions: 100,
                  label: flashControlProvider.flashIntensity.round().toString(),
                  onChanged: (value) {
                    flashControlProvider.setFlashIntensity(value.round());
                    flashControlProvider.notifyCarControlProvider(context);
                  },
                ),
              ),
              Icon(Icons.flash_on, color: Colors.yellow),
            ],
          ),
        );
      },
    );
  }
}

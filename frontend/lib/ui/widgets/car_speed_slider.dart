import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../providers/speed.control.provider.dart';
import '../../utils/constants.dart';


class CarSpeedSlider extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Consumer<SpeedControlProvider>(
      builder: (context, speedControlProvider, child) {
        return Container(
          width: MediaQuery.of(context).size.width > 800
              ? kWebWidth
              : kMobileWidth,
          child: Row(
            children: [
              Icon(Icons.speed, color: Colors.grey),
              Expanded(
                child: Slider(
                  value: speedControlProvider.carSpeed.toDouble(),
                  min: 0,
                  max: 255,
                  divisions: 255,
                  label: speedControlProvider.carSpeed.round().toString(),
                  onChanged: (value) {
                    speedControlProvider.setCarSpeed(value.round());
                    speedControlProvider.notifyCarControlProvider(context);
                  },
                ),
              ),
              Icon(Icons.speed, color: Colors.red),
            ],
          ),
        );
      },
    );
  }
}

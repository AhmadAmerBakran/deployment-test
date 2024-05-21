import 'package:flutter/material.dart';
import 'dart:async';
import 'car_control_provider.dart';
import 'package:provider/provider.dart';


class SpeedControlProvider with ChangeNotifier {
  int _carSpeed = 136;
  Timer? _debounce;


  int get carSpeed => _carSpeed;


  void setCarSpeed(int value) {
    _carSpeed = value;
    notifyListeners();
  }


  void notifyCarControlProvider(BuildContext context) {
    if (_debounce?.isActive ?? false) _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 300), () {
      final carControlProvider = Provider.of<CarControlProvider>(context, listen: false);
      carControlProvider.sendCommand('car/speed', _carSpeed.toString());
    });
  }
}

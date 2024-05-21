import 'package:flutter/material.dart';
import 'dart:async';
import 'car_control_provider.dart';
import 'package:provider/provider.dart';


class FlashControlProvider with ChangeNotifier {
  int _flashIntensity = 0;
  Timer? _debounce;


  int get flashIntensity => _flashIntensity;


  void setFlashIntensity(int value) {
    _flashIntensity = value;
    notifyListeners();
  }


  void notifyCarControlProvider(BuildContext context) {
    if (_debounce?.isActive ?? false) _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 300), () {
      final carControlProvider = Provider.of<CarControlProvider>(context, listen: false);
      carControlProvider.sendCommand('cam/flash', _flashIntensity.toString());
    });
  }
}

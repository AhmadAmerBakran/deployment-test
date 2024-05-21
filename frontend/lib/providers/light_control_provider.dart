import 'package:flutter/material.dart';


enum LightState { off, on, auto }


class LightControlProvider with ChangeNotifier {
  LightState _lightState = LightState.off;


  LightState get lightState => _lightState;


  void setLightState(LightState state) {
    _lightState = state;
    notifyListeners();
  }


  void cycleLightState() {
    _lightState = _lightState == LightState.off
        ? LightState.on
        : _lightState == LightState.on
        ? LightState.auto
        : LightState.off;
    notifyListeners();
  }


  String get command {
    switch (_lightState) {
      case LightState.on:
        return 'on';
      case LightState.auto:
        return 'auto';
      case LightState.off:
      default:
        return 'off';
    }
  }
}

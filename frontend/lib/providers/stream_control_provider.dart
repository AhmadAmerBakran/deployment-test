import 'package:flutter/material.dart';
import 'dart:typed_data';
import 'package:provider/provider.dart';
import 'car_control_provider.dart';


class StreamControlProvider with ChangeNotifier {
  bool _isStreaming = false;
  Uint8List? _currentImage;


  bool get isStreaming => _isStreaming;
  Uint8List? get currentImage => _currentImage;


  void startStream(BuildContext context) {
    _isStreaming = true;
    context.read<CarControlProvider>().sendCommand('cam/control', 'start');
    notifyListeners();
  }


  void stopStream(BuildContext context) {
    _isStreaming = false;
    _currentImage = null;
    context.read<CarControlProvider>().sendCommand('cam/control', 'stop');
    notifyListeners();
  }


  void setCurrentImage(Uint8List image) {
    _currentImage = image;
    notifyListeners();
  }
}

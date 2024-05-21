import 'dart:typed_data';

import 'package:flutter/material.dart';
import '../models/user.dart';
import '../services/websocket_service.dart';


class CarControlProvider with ChangeNotifier {
  final WebSocketService webSocketService;


  String? _errorMessage;

  CarControlProvider({required this.webSocketService}) {
    webSocketService.errorStream.listen((errorEvent) {
      _errorMessage = errorEvent.errorMessage;
      notifyListeners();
    });
  }

  String? get errorMessage => _errorMessage;


  void sendCommand(String topic, String command) {
    try {
      webSocketService.sendCarControlCommand(topic, command);
    } catch (e) {
      _errorMessage = e.toString();
      notifyListeners();
    }
  }


    void signIn(User user) {
      try {
        webSocketService.sendSignIn(user.nickname);
      } catch (e) {
        _errorMessage = e.toString();
        notifyListeners();
      }
    }


    void signOut() {
      try {
        webSocketService.sendSignOut();
      } catch (e) {
        _errorMessage = e.toString();
        notifyListeners();
      }
    }


    void receiveNotifications() {
      try {
        webSocketService.sendReceiveNotifications();
      } catch (e) {
        _errorMessage = e.toString();
        notifyListeners();
      }
    }


    void getCarLog() {
      try {
        webSocketService.sendGetCarLog();
      } catch (e) {
        _errorMessage = e.toString();
        notifyListeners();
      }
    }

    void reconnect(String url, Function(String) onMessageReceived,
        Function(Uint8List) onBinaryMessageReceived) {
      try {
        webSocketService.init(url, onMessageReceived, onBinaryMessageReceived);
      } catch (e) {
        _errorMessage = e.toString();
        notifyListeners();
      }
    }

    void clearError() {
      _errorMessage = null;
      notifyListeners();
  }
}
import 'package:flutter/material.dart';


class NotificationsProvider with ChangeNotifier {
  List<String> _notifications = [];
  int _unreadCount = 0;
  bool _notificationsEnabled = true;


  List<String> get notifications => _notifications;
  int get unreadCount => _unreadCount;
  bool get notificationsEnabled => _notificationsEnabled;


  void addNotification(String notification) {
    if (_notificationsEnabled && notification.startsWith('Notification on')) {
      _notifications.add(notification);
      _unreadCount++;
      notifyListeners();
    }
  }


  void clearUnreadCount() {
    _unreadCount = 0;
    notifyListeners();
  }


  void toggleNotifications(bool isEnabled) {
    _notificationsEnabled = isEnabled;
    notifyListeners();
  }
}

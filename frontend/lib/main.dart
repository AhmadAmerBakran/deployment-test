import 'package:flutter/material.dart';
import 'package:frontend/providers/speed.control.provider.dart';
import 'package:frontend/ui/widgets/error_listener_widget.dart';
import 'package:provider/provider.dart';
import 'providers/car_control_provider.dart';
import 'providers/user_provider.dart';
import 'providers/light_control_provider.dart';
import 'providers/notifications_provider.dart';
import 'providers/flash_control_provider.dart';
import 'providers/stream_control_provider.dart';
import 'services/websocket_service.dart';
import 'ui/screens/car_control_screen.dart';
import 'ui/screens/login_screen.dart';
import 'utils/constants.dart';


void main() {
  final webSocketService = WebSocketService();
  webSocketService.init(
    WEBSOCKET_URL,
        (message) {
      print('WebSocket message: $message');
    },
        (binaryMessage) {
      // Handle binary message
    },
  );

  runApp(MyApp(webSocketService: webSocketService));
}


class MyApp extends StatelessWidget {
  final WebSocketService webSocketService;

  MyApp({required this.webSocketService});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => UserProvider()),
        ChangeNotifierProvider(create: (_) => LightControlProvider()),
        ChangeNotifierProvider(create: (_) => SpeedControlProvider()),
        ChangeNotifierProvider(create: (_) => NotificationsProvider()),
        ChangeNotifierProvider(create: (_) => FlashControlProvider()),
        ChangeNotifierProvider(create: (_) => StreamControlProvider()),
        ChangeNotifierProvider(
          create: (context) => CarControlProvider(webSocketService: webSocketService),
        ),
      ],
      child: ErrorListenerWidget(
        child: MaterialApp(
          title: 'Car Control App',
          theme: ThemeData(
            primarySwatch: Colors.blue,
          ),
          initialRoute: '/',
          routes: {
            '/': (context) => LoginScreen(),
            '/carControl': (context) => CarControlScreen(),
          },
        ),
      ),
    );
  }
}
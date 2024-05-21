import 'dart:async';
import 'dart:io';
import 'dart:ui' as ui;
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import '../../models/events.dart';
import '../../providers/notifications_provider.dart';
import '../../providers/car_control_provider.dart';
import '../../providers/user_provider.dart';
import '../widgets/Aanimated_background.dart';
import '../widgets/animated_app_bar.dart';
import '../widgets/car_speed_slider.dart';
import '../widgets/error_listener_widget.dart';
import '../widgets/flash_intensity_slider.dart';
import '../widgets/notification_list_widget.dart';
import '../widgets/stream_container_widget.dart';
import '../widgets/control_buttons.dart';
import '../widgets/gamepad_widget.dart';

class CarControlScreen extends StatefulWidget {
  @override
  _CarControlScreenState createState() => _CarControlScreenState();
}


class _CarControlScreenState extends State<CarControlScreen> {
  ui.Image? _currentImage;
  bool _isStreaming = false;
  bool _notificationsEnabled = false;
  StreamSubscription<ErrorResponseEvent>? _errorSubscription;


  @override
  void initState() {
    super.initState();
    print("CarControlScreen initialized");
    _setOrientation();
    final webSocketService = context.read<CarControlProvider>().webSocketService;
    webSocketService.messageStream.listen(_onMessageReceived);
    webSocketService.binaryMessageStream.listen(_onBinaryMessageReceived);
    _errorSubscription = context.read<CarControlProvider>().webSocketService.errorStream.listen((errorEvent) {
      _showErrorSnackbar(errorEvent.errorMessage);
    });
  }

  void _showErrorSnackbar(String? errorMessage) {
    if (errorMessage != null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(errorMessage),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  void _toggleNotifications(bool value) {
    setState(() {
      _notificationsEnabled = value;
    });
    context.read<NotificationsProvider>().toggleNotifications(value);
    context.read<CarControlProvider>().receiveNotifications();
  }


  void _setOrientation() {
    if (!kIsWeb && Platform.isAndroid) {
      SystemChrome.setPreferredOrientations([DeviceOrientation.landscapeLeft]);
    }
  }


  void _onMessageReceived(String message) {
    print("Received text message: $message");
    if (message.startsWith("Notification on")) {
      context.read<NotificationsProvider>().addNotification(message);
    }
  }

  void _onBinaryMessageReceived(Uint8List message) async {
    print("Received binary message of length: ${message.length}");
    final decodedImage = await _decodeImageFromList(message);
    setState(() {
      _currentImage = decodedImage;
    });
  }


  Future<ui.Image> _decodeImageFromList(Uint8List list) async {
    final Completer<ui.Image> completer = Completer();
    ui.decodeImageFromList(list, (ui.Image img) {
      completer.complete(img);
    });
    return completer.future;
  }



  void _startStream() {
    print("Starting stream...");
    setState(() {
      _isStreaming = true;
    });
    print("Connected to WebSocket");
  }


  void _stopStream() {
    print("Stopping stream...");
    context.read<CarControlProvider>().webSocketService.close();
    setState(() {
      _isStreaming = false;
      _currentImage = null;
    });
    print("Disconnected from WebSocket");
  }


  @override
  Widget build(BuildContext context) {
    final userProvider = Provider.of<UserProvider>(context);
    return ErrorListenerWidget(
      child: Scaffold(
        body: Stack(
          children: [
            Positioned.fill(
              child: AnimatedBackground(),
            ),
            Column(
              children: [
                AnimatedAppBar(
                  title: userProvider.user?.nickname ?? 'Car Control',
                  leading: Builder(
                    builder: (context) => PopupMenuButton(
                      icon: Icon(Icons.settings),
                      itemBuilder: (context) => [
                        PopupMenuItem(
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text("Enable Notifications"),
                              Switch(
                                value: _notificationsEnabled,
                                onChanged: (value) {
                                  _toggleNotifications(value);
                                  Navigator.pop(context);
                                },
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                  actions: [
                    Consumer<NotificationsProvider>(
                      builder: (context, notificationsProvider, child) {
                        return Stack(
                          children: [
                            IconButton(
                              icon: Icon(Icons.notifications),
                              onPressed: () {
                                notificationsProvider.clearUnreadCount();
                                showModalBottomSheet(
                                  context: context,
                                  builder: (context) => NotificationList(),
                                );
                              },
                            ),
                            if (notificationsProvider.unreadCount > 0)
                              Positioned(
                                right: 8,
                                top: 8,
                                child: Container(
                                  padding: EdgeInsets.all(2),
                                  decoration: BoxDecoration(
                                    color: Colors.red,
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  constraints: BoxConstraints(
                                    minWidth: 16,
                                    minHeight: 16,
                                  ),
                                  child: Center(
                                    child: Text(
                                      '${notificationsProvider.unreadCount}',
                                      style: TextStyle(
                                        color: Colors.white,
                                        fontSize: 10,
                                      ),
                                      textAlign: TextAlign.center,
                                    ),
                                  ),
                                ),
                              ),
                          ],
                        );
                      },
                    ),
                    IconButton(
                      icon: Icon(Icons.history),
                      onPressed: () {
                        context.read<CarControlProvider>().getCarLog();
                      },
                    ),
                    IconButton(
                      icon: Icon(Icons.exit_to_app),
                      onPressed: () {
                        context.read<CarControlProvider>().signOut();
                        Navigator.pushReplacementNamed(context, '/');
                      },
                    ),
                  ],
                ),
                Expanded(
                  child: LayoutBuilder(
                    builder: (context, constraints) {
                      return constraints.maxWidth > 800
                          ? _buildWebLayout()
                          : _buildMobileLayout();
                    },
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }


  Widget _buildMobileLayout() {
    return Container(
      constraints: BoxConstraints(minHeight: 200),
      child: Column(
        children: [
          Expanded(
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: [
                Flexible(
                  child: Column(
                    children: [
                      Expanded(
                        child: LayoutBuilder(
                          builder: (context, constraints) {
                            return Container(
                              constraints: BoxConstraints(
                                maxHeight: constraints.maxHeight - 20,
                              ),
                              child: GamepadWidget(),
                            );
                          },
                        ),
                      ),
                    ],
                  ),
                ),
                Flexible(
                  child: Column(
                    children: [
                      Expanded(
                        child: LayoutBuilder(
                          builder: (context, constraints) {
                            return Container(
                              constraints: BoxConstraints(
                                maxHeight: constraints.maxHeight - 20,
                              ),
                              child: StreamContainer(
                                isStreaming: _isStreaming,
                                currentImage: _currentImage,
                              ),
                            );
                          },
                        ),
                      ),
                      FlashIntensitySlider(),
                      CarSpeedSlider(),
                    ],
                  ),
                ),
                Flexible(
                  child: Column(
                    children: [
                      Expanded(
                        child: LayoutBuilder(
                          builder: (context, constraints) {
                            return Container(
                              constraints: BoxConstraints(
                                maxHeight: constraints.maxHeight - 20,
                              ),
                              child: ControlButtons(
                                onStartStream: _startStream,
                                onStopStream: _stopStream,
                              ),
                            );
                          },
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }


  Widget _buildWebLayout() {
    return Column(
      children: [
        Expanded(
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Column(
                children: [
                  Flexible(
                    flex: 3,
                    child: Row(
                      children: [
                        StreamContainer(
                          isStreaming: _isStreaming,
                          currentImage: _currentImage,
                        ),
                      ],
                    ),
                  ),
                  Flexible(

                    child: Row(
                      children: [
                        FlashIntensitySlider(),
                      ],
                    ),
                  ),
                  Flexible(

                    child: Row(
                      children: [
                        CarSpeedSlider(),
                      ],
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
        Expanded(
          child: Row(
            children: [
              Expanded(
                child: GamepadWidget(),
              ),
              Expanded(
                child: ControlButtons(
                  onStartStream: _startStream,
                  onStopStream: _stopStream,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }


  @override
  void dispose() {
    if (_isStreaming) {
      context.read<CarControlProvider>().webSocketService.close();
    }
    SystemChrome.setPreferredOrientations([
      DeviceOrientation.portraitUp,
      DeviceOrientation.portraitDown,
      DeviceOrientation.landscapeLeft,
      DeviceOrientation.landscapeRight,
    ]);
    super.dispose();
  }
}

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../providers/car_control_provider.dart';
import 'styled_button_widget.dart';

class GamepadWidget extends StatefulWidget {
  @override
  _GamepadWidgetState createState() => _GamepadWidgetState();
}

class _GamepadWidgetState extends State<GamepadWidget> {
  @override
  Widget build(BuildContext context) {
    final carControlProvider = Provider.of<CarControlProvider>(context, listen: false);

    return LayoutBuilder(
      builder: (context, constraints) {
        return Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Flexible(

              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CustomIconButton(
                    icon: Icons.arrow_upward,
                    onTap: () {},
                    onTapDown: (_) {
                      carControlProvider.sendCommand('car/control', '1');
                    },
                    onTapUp: (_) {
                      carControlProvider.sendCommand('car/control', '0');
                    },
                    color: Colors.blue,
                    size: 60,
                  ),
                ],
              ),
            ),
            Flexible(

              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CustomIconButton(
                    icon: Icons.arrow_back,
                    onTap: () {},
                    onTapDown: (_) {
                      carControlProvider.sendCommand('car/control', '6');
                    },
                    onTapUp: (_) {
                      carControlProvider.sendCommand('car/control', '0');
                    },
                    color: Colors.blue,
                    size: 60,
                  ),
                  SizedBox(width: 20),
                  CustomIconButton(
                    icon: Icons.arrow_forward,
                    onTap: () {},
                    onTapDown: (_) {
                      carControlProvider.sendCommand('car/control', '5');
                    },
                    onTapUp: (_) {
                      carControlProvider.sendCommand('car/control', '0');
                    },
                    color: Colors.blue,
                    size: 60,
                  ),
                ],
              ),
            ),
            Flexible(
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CustomIconButton(
                    icon: Icons.arrow_downward,
                    onTap: () {},
                    onTapDown: (_) {
                      carControlProvider.sendCommand('car/control', '2');
                    },
                    onTapUp: (_) {
                      carControlProvider.sendCommand('car/control', '0');
                    },
                    color: Colors.blue,
                    size: 60,
                  ),
                ],
              ),
            ),
          ],
        );
      },
    );
  }
}
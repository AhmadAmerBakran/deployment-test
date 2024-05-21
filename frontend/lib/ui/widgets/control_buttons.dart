import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../providers/car_control_provider.dart';
import '../../providers/light_control_provider.dart';
import '../../providers/stream_control_provider.dart';
import 'styled_button_widget.dart';


class ControlButtons extends StatelessWidget {
  final VoidCallback onStartStream;
  final VoidCallback onStopStream;


  ControlButtons({required this.onStartStream, required this.onStopStream});


  @override
  Widget build(BuildContext context) {
    final carControlProvider = Provider.of<CarControlProvider>(context);
    final lightControlProvider = Provider.of<LightControlProvider>(context);
    final streamControlProvider = Provider.of<StreamControlProvider>(context);
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Flexible(

          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              CustomIconButton(
                icon: Icons.drive_eta,
                onTap: () => carControlProvider.sendCommand('car/control', '7'),
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
                icon: Icons.play_arrow,
                onTap: () {
                  streamControlProvider.startStream(context);
                  onStartStream();
                },
                color: Colors.green,
                size: 60,
              ),
              SizedBox(width: 20),
              CustomIconButton(
                icon: Icons.stop,
                onTap: () {
                  streamControlProvider.stopStream(context);
                  onStopStream();
                },
                color: Colors.red,
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
                icon: Icons.lightbulb,
                onTap: () {
                  lightControlProvider.cycleLightState();
                  carControlProvider.sendCommand('car/led/control', lightControlProvider.command);
                },
                color: Colors.orange,
                size: 60,
              ),
            ],
          ),
        ),
      ],
    );
  }
}

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../providers/car_control_provider.dart';

class ErrorListenerWidget extends StatelessWidget {
  final Widget child;

  ErrorListenerWidget({required this.child});

  @override
  Widget build(BuildContext context) {
    return Consumer<CarControlProvider>(
      builder: (context, carControlProvider, _) {
        WidgetsBinding.instance?.addPostFrameCallback((_) {
          if (carControlProvider.errorMessage != null) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(carControlProvider.errorMessage!),
                backgroundColor: Colors.red,
              ),
            );
            carControlProvider.clearError();
          }
        });
        return child;
      },
    );
  }
}

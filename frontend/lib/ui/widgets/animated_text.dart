import 'package:flutter/material.dart';

class AnimatedText extends StatefulWidget {
  final ScrollController scrollController;
  AnimatedText({required this.scrollController});

  @override
  _AnimatedTextState createState() => _AnimatedTextState();
}

class _AnimatedTextState extends State<AnimatedText> with TickerProviderStateMixin {
  late AnimationController _typingController;
  late Animation<int> _typingAnimation;
  late AnimationController _cursorController;
  late Animation<double> _cursorAnimation;

  final String _welcomeText = """
Welcome to our Car Control project!

With this platform, you can easily control our car which is connected to an ESP microcontroller and receive real-time video streaming from it. No authentication or authorization is required—just enter your nickname and start exploring.

Features:
• Control the IoT Car: Seamlessly navigate the car from your device.
• Real-Time Video Stream: Get instant video feedback from the car’s camera.

Developed by: Mahmoud Eybo and Ahmad Amer Bakran
""";

  @override
  void initState() {
    super.initState();
    _typingController = AnimationController(
      duration: const Duration(seconds: 10),
      vsync: this,
    )..addListener(() {
      if (widget.scrollController.hasClients) {
        widget.scrollController.animateTo(
          widget.scrollController.position.maxScrollExtent,
          duration: Duration(milliseconds: 100),
          curve: Curves.easeInOut,
        );
      }
    });

    _typingAnimation = StepTween(begin: 0, end: _welcomeText.length).animate(_typingController);
    _typingController.forward();

    _cursorController = AnimationController(
      duration: const Duration(milliseconds: 500),
      vsync: this,
    )..repeat(reverse: true);

    _cursorAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(_cursorController);
  }

  @override
  void dispose() {
    _typingController.dispose();
    _cursorController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: Listenable.merge([_typingAnimation, _cursorAnimation]),
      builder: (context, child) {
        String text = _welcomeText.substring(0, _typingAnimation.value);
        return Text.rich(
          TextSpan(
            text: text,
            style: TextStyle(fontSize: 16, color: Colors.white),
            children: [
              TextSpan(
                text: _cursorAnimation.value > 0.5 ? '|' : '',
                style: TextStyle(fontSize: 16, color: Colors.greenAccent),
              ),
            ],
          ),
        );
      },
    );
  }
}

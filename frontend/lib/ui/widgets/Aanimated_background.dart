import 'package:flutter/material.dart';

class AnimatedBackground extends StatefulWidget {
  @override
  _AnimatedBackgroundState createState() => _AnimatedBackgroundState();
}

class _AnimatedBackgroundState extends State<AnimatedBackground>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<Color?> _colorTween1;
  late Animation<Color?> _colorTween2;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(seconds: 20),
      vsync: this,
    )..repeat(reverse: true);

    _colorTween1 = _controller.drive(
      TweenSequence<Color?>([
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFFFF0000), end: Color(0xFFFFA500)), // Red to Orange
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFFFFA500), end: Color(0xFFFFFF00)), // Orange to Yellow
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFFFFFF00), end: Color(0xFF00FF00)), // Yellow to Green
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFF00FF00), end: Color(0xFF00FFFF)), // Green to Cyan
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFF00FFFF), end: Color(0xFF0000FF)), // Cyan to Blue
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFF0000FF), end: Color(0xFFFF00FF)), // Blue to Magenta
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFFFF00FF), end: Color(0xFFFF0000)), // Magenta to Red
        ),
      ]),
    );

    _colorTween2 = _controller.drive(
      TweenSequence<Color?>([
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFF00FFFF), end: Color(0xFFFF0000)), // Cyan to Red
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFFFF0000), end: Color(0xFFFF00FF)), // Red to Magenta
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFFFF00FF), end: Color(0xFF0000FF)), // Magenta to Blue
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFF0000FF), end: Color(0xFF00FF00)), // Blue to Green
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFF00FF00), end: Color(0xFFFFFF00)), // Green to Yellow
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFFFFFF00), end: Color(0xFFFFA500)), // Yellow to Orange
        ),
        TweenSequenceItem(
          weight: 1.0,
          tween: ColorTween(begin: Color(0xFFFFA500), end: Color(0xFF00FFFF)), // Orange to Cyan
        ),
      ]),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, child) {
        return Container(
          decoration: BoxDecoration(
            gradient: LinearGradient(
              colors: [
                _colorTween1.value!,
                _colorTween2.value!,
              ],
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
            ),
          ),
        );
      },
    );
  }
}

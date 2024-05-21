import 'package:flutter/material.dart';

class CustomIconButton extends StatefulWidget {
  final IconData icon;
  final VoidCallback onTap;
  final void Function(TapDownDetails)? onTapDown;
  final void Function(TapUpDetails)? onTapUp;
  final Color color;
  final double size;

  CustomIconButton({
    required this.icon,
    required this.onTap,
    this.onTapDown,
    this.onTapUp,
    this.color = Colors.blue,
    this.size = 60.0,
  });

  @override
  _CustomIconButtonState createState() => _CustomIconButtonState();
}

class _CustomIconButtonState extends State<CustomIconButton> {
  bool _isHovered = false;
  bool _isPressed = false;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: widget.onTap,
      onTapDown: (details) {
        setState(() => _isPressed = true);
        if (widget.onTapDown != null) widget.onTapDown!(details);
      },
      onTapUp: (details) {
        setState(() => _isPressed = false);
        if (widget.onTapUp != null) widget.onTapUp!(details);
      },
      onTapCancel: () => setState(() => _isPressed = false),
      child: MouseRegion(
        onEnter: (_) => setState(() => _isHovered = true),
        onExit: (_) => setState(() => _isHovered = false),
        child: AnimatedContainer(
          duration: Duration(milliseconds: 200),
          height: _isPressed ? widget.size * 0.9 : (_isHovered ? widget.size * 1.1 : widget.size),
          width: _isPressed ? widget.size * 0.9 : (_isHovered ? widget.size * 1.1 : widget.size),
          decoration: BoxDecoration(
            color: widget.color,
            borderRadius: BorderRadius.circular(30),
            boxShadow: [
              BoxShadow(
                color: Colors.black26,
                blurRadius: 10,
                spreadRadius: _isPressed ? 1 : 5,
              ),
            ],
          ),
          child: Icon(widget.icon, size: 40, color: Colors.white),
        ),
      ),
    );
  }
}
import 'package:flutter/material.dart';
import 'carousel_slider_widget.dart';
import 'animated_text.dart';
import 'login_form.dart';

class LoginWidget extends StatefulWidget {
  @override
  _LoginWidgetState createState() => _LoginWidgetState();
}

class _LoginWidgetState extends State<LoginWidget> with TickerProviderStateMixin {
  final TextEditingController _nicknameController = TextEditingController();
  late ScrollController _scrollController;

  @override
  void initState() {
    super.initState();
    _scrollController = ScrollController();
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    double screenWidth = MediaQuery.of(context).size.width;
    double screenHeight = MediaQuery.of(context).size.height;
    double textContainerWidth = screenWidth > 600 ? 400 : screenWidth * 0.8;
    double textContainerHeight = screenWidth > 600 ? 200 : screenHeight * 0.25;

    return LayoutBuilder(
      builder: (context, constraints) {
        return Stack(
          children: [
            Positioned.fill(child: CarouselSliderWidget()),
            Positioned(
              top: 16.0,
              left: screenWidth <= 600 ? (screenWidth - textContainerWidth) / 2 : 16.0,
              child: Container(
                width: textContainerWidth,
                height: textContainerHeight,
                padding: EdgeInsets.all(12.0),
                decoration: BoxDecoration(
                  color: Colors.black.withOpacity(0.7),
                  borderRadius: BorderRadius.circular(10.0),
                ),
                child: SingleChildScrollView(
                  controller: _scrollController,
                  child: AnimatedText(
                    scrollController: _scrollController,
                  ),
                ),
              ),
            ),
            Positioned(
              top: screenHeight > 600 ? screenHeight * 0.35 : screenHeight * 0.3,
              left: 16.0,
              right: 16.0,
              child: Center(
                child: LoginForm(
                  nicknameController: _nicknameController,
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}

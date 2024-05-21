import 'dart:io';
import 'dart:typed_data';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:image_gallery_saver/image_gallery_saver.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:universal_html/html.dart' as html;
import 'dart:ui' as ui;
import 'package:frontend/ui/widgets/stream_widget.dart';

class StreamContainer extends StatefulWidget {
  final bool isStreaming;
  final ui.Image? currentImage;

  StreamContainer({required this.isStreaming, required this.currentImage});

  @override
  _StreamContainerState createState() => _StreamContainerState();
}

class _StreamContainerState extends State<StreamContainer> {
  final GlobalKey repaintBoundaryKey = GlobalKey();

  @override
  void initState() {
    super.initState();
    _requestPermissions();
  }

  /* We've found out that there is no need for external storage permission write
  to be able to store screenshots in gallery for android 12 and above,
  but we left it as it is as an example for requesting a permission*/
  Future<void> _requestPermissions() async {
    if (!kIsWeb && Platform.isAndroid) {
      await [
        Permission.storage,
        Permission.manageExternalStorage,
      ].request();
    }
  }

  Future<void> _takeScreenshot() async {
    try {
      RenderRepaintBoundary boundary = repaintBoundaryKey.currentContext?.findRenderObject() as RenderRepaintBoundary;
      ui.Image image = await boundary.toImage(pixelRatio: 2.0);
      ByteData? byteData = await image.toByteData(format: ui.ImageByteFormat.png);
      if (byteData != null) {
        Uint8List pngBytes = byteData.buffer.asUint8List();

        if (kIsWeb) {
          final blob = html.Blob([pngBytes], 'image/png');
          final url = html.Url.createObjectUrlFromBlob(blob);
          final anchor = html.AnchorElement(href: url)
            ..setAttribute('download', 'screenshot.png')
            ..click();
          html.Url.revokeObjectUrl(url);
        } else {
          final result = await ImageGallerySaver.saveImage(pngBytes);
          print("Screenshot saved: $result");
        }
      }
    } catch (e) {
      print("Error taking screenshot: $e");
    }
  }


  @override
  Widget build(BuildContext context) {
    return Center(
      child: Stack(
        children: [
          Container(
            width: MediaQuery.of(context).size.width * 0.47,
            height: MediaQuery.of(context).size.height * 0.47,
            decoration: BoxDecoration(
              border: Border.all(color: Colors.blueAccent),
              borderRadius: BorderRadius.circular(10),
            ),
            child: RepaintBoundary(
              key: repaintBoundaryKey,
              child: widget.isStreaming
                  ? widget.currentImage != null
                  ? StreamWidget(currentImage: widget.currentImage!)
                  : Center(child: CircularProgressIndicator())
                  : Center(child: Text('Stream not started')),
            ),
          ),
          Positioned(
            left: 16,
            top: 16,
            child: IconButton(
              icon: Icon(Icons.camera_alt),
              onPressed: _takeScreenshot,
              tooltip: 'Take Screenshot',
              color: Colors.blue,
              iconSize: 30.0,
            ),
          ),
        ],
      ),
    );
  }
}

import 'dart:async';
import 'package:client/api.dart';
import 'package:http/http.dart' as http;

class VideoService {
  Future<String> getVideoId(String url) async {
    try {
      var response = await http.get(
        Uri(
          scheme: API.scheme,
          host: API.host,
          port: API.port,
          path: API.path,
          queryParameters: {'url': url},
        ),
        headers: {
          "Access-Control-Allow-Origin": "*",
          'Content-Type': 'application/json',
          'Accept': '*/*'
        },
      );

      if (response.statusCode == 200) {
        return response.body;
      } else {
        throw Exception('Failed to get video id');
      }
    } catch (e) {
      return 'Error getting video id $e';
    }
  }
}

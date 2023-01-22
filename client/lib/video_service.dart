import 'dart:async';

//import http package
import 'package:http/http.dart' as http;
import 'dart:convert';

class VideoService {
  Future<String> getVideoId(String url) async {
    try {
      var response = await http.get(
        Uri(
          scheme: 'http',
          host: 'localhost',
          port: 5260,
          path: '/Music',
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

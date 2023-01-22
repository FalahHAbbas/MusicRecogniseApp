import 'package:client/api.dart';
import 'package:client/video_service.dart';
import 'package:flutter/material.dart';
import 'package:signalr_netcore/hub_connection.dart';
import 'package:signalr_netcore/hub_connection_builder.dart';
import 'dart:convert';

import 'package:url_launcher/url_launcher.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutter Demo',
      theme: ThemeData(
        primarySwatch: Colors.blue,
      ),
      home: const MyHomePage(title: 'Music Recognizer'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key, required this.title});

  final String title;

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  final VideoService _videoService = VideoService();
  final TextEditingController _txtController = TextEditingController();
  List<Song> _songs = [];
  HubConnection hubConnection =
      HubConnectionBuilder().withUrl(API.BASE_HUB_URL).build();
  String _message = '';

  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title),
      ),
      body: Center(
        child: Form(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: <Widget>[
              Padding(
                padding: const EdgeInsets.all(8.0),
                child: Text(
                  _message,
                  style: const TextStyle(fontSize: 20),
                ),
              ),
              Padding(
                padding: const EdgeInsets.all(8.0),
                child: TextFormField(
                  decoration: const InputDecoration(
                    label: Text('Youtube link'),
                  ),
                  controller: _txtController,
                ),
              ),
              if (_message != '') const LinearProgressIndicator(),
              Padding(
                padding: const EdgeInsets.all(8.0),
                child: ElevatedButton(
                  onPressed: () {
                    _videoService
                        .getVideoId(_txtController.text)
                        .then((value) async {
                          hubConnection.stop();
                          hubConnection = HubConnectionBuilder()
                              .withUrl(API.BASE_HUB_URL)
                              .build();
                          setState(() => _message = 'Connecting to server');
                          await hubConnection.start();
                          await hubConnection
                              .invoke('AddToGroup', args: [value]);
                          hubConnection.on(
                              'newMessage',
                              (message) => setState(() {
                                    var type = message![0].toString();
                                    if (type == 'data') {
                                      var songsString =
                                          jsonDecode(message[1].toString());
                                      setState(() {
                                        _songs = songsString
                                            .map<Song>(
                                                (song) => Song.fromJson(song))
                                            .toList();
                                        this._message = '';
                                      });
                                    } else if (type == 'error') {
                                      this._message = '';
                                      ScaffoldMessenger.of(context)
                                          .showSnackBar(SnackBar(
                                        content: Text(message[1].toString()),
                                      ));
                                    } else {
                                      this._message = message[1].toString();
                                    }

                                    print(message.toString());
                                  }));
                        })
                        .then((value) => print(value))
                        .catchError((error) => print(error));
                  },
                  child: const Text('Submit'),
                ),
              ),
              if (_songs.isNotEmpty)
                Expanded(
                  child: ListView.builder(
                    itemCount: _songs.length,
                    itemBuilder: (context, index) {
                      return ListTile(
                        title: Text(_songs[index].title),
                        subtitle: Text(_songs[index].artist),
                        leading: Image.network(_songs[index].image),
                        trailing: IconButton(
                          icon: const Icon(Icons.play_arrow),
                          onPressed: () {
                            print(_songs[index].url);
                            openUrl(_songs[index].url);
                          },
                        ),
                      );
                    },
                  ),
                )
            ],
          ),
        ),
      ),
    );
  }

  void openUrl(String url) async {
    if (await canLaunch(url)) {
      await launch(url, forceSafariVC: false, forceWebView: false);
    } else {
      throw 'Could not launch $url';
    }
  }
}

class Song {
  final String title;
  final String artist;
  final String image;
  final String url;

  Song({
    this.title = '',
    this.artist = '',
    this.image = '',
    this.url = '',
  });

  factory Song.fromJson(Map<String, dynamic> json) {
    return Song(
        title: json['title'],
        artist: json['artist'],
        image: json['image'].toString().replaceAll('https', 'http'),
        url: json['url']);
  }

  Map<String, dynamic> toJson() => {
        'title': title,
        'artist': artist,
        'image': image,
        'url': url,
      };
}

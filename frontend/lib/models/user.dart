class User {
  final String nickname;

  User({required this.nickname});

  factory User.fromJson(Map<String, dynamic> json) {
    return User(nickname: json['nickname']);
  }

  Map<String, dynamic> toJson() {
    return {'nickname': nickname};
  }
}
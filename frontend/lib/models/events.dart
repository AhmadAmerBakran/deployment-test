class BaseEvent {
  final String eventType;


  BaseEvent({required this.eventType});


  Map<String, dynamic> toJson() {
    return {
      'eventType': eventType,
    };
  }
}


class CarControlCommand extends BaseEvent {
  final String topic;
  final String command;


  CarControlCommand({
    required String eventType,
    required this.topic,
    required this.command,
  }) : super(eventType: eventType);


  factory CarControlCommand.fromJson(Map<String, dynamic> json) {
    return CarControlCommand(
      eventType: json['eventType'],
      topic: json['Topic'],
      command: json['Command'],
    );
  }


  @override
  Map<String, dynamic> toJson() {
    var map = super.toJson();
    map['Topic'] = topic;
    map['Command'] = command;
    return map;
  }
}


class SignInEvent extends BaseEvent {
  final String nickName;


  SignInEvent({required String eventType, required this.nickName})
      : super(eventType: eventType);


  factory SignInEvent.fromJson(Map<String, dynamic> json) {
    return SignInEvent(
      eventType: json['eventType'],
      nickName: json['NickName'],
    );
  }


  @override
  Map<String, dynamic> toJson() {
    var map = super.toJson();
    map['NickName'] = nickName;
    return map;
  }
}


class SignOutEvent extends BaseEvent {
  SignOutEvent({required String eventType}) : super(eventType: eventType);


  factory SignOutEvent.fromJson(Map<String, dynamic> json) {
    return SignOutEvent(
      eventType: json['eventType'],
    );
  }
}


class ReceiveNotificationsEvent extends BaseEvent {
  ReceiveNotificationsEvent({required String eventType})
      : super(eventType: eventType);


  factory ReceiveNotificationsEvent.fromJson(Map<String, dynamic> json) {
    return ReceiveNotificationsEvent(
      eventType: json['eventType'],
    );
  }
}


class GetCarLogEvent extends BaseEvent {
  GetCarLogEvent({required String eventType}) : super(eventType: eventType);


  factory GetCarLogEvent.fromJson(Map<String, dynamic> json) {
    return GetCarLogEvent(
      eventType: json['eventType'],
    );
  }
}

class ErrorResponseEvent extends BaseEvent {
  final String? errorMessage;

  ErrorResponseEvent({
    required String eventType,
    this.errorMessage,
  }) : super(eventType: eventType);

  factory ErrorResponseEvent.fromJson(Map<String, dynamic> json) {
    return ErrorResponseEvent(
      eventType: json['eventType'],
      errorMessage: json['ErrorMessage'],
    );
  }

  @override
  Map<String, dynamic> toJson() {
    var map = super.toJson();
    map['ErrorMessage'] = errorMessage;
    return map;
  }
}

{Robot, Adapter, TextMessage, EnterMessage, LeaveMessage, TopicMessage, Response} = require 'hubot'

DotNetServer = require './server'


class DotNet extends Adapter

  constructor: (robot) ->
    super robot
    @port = process.env.HUBOT_DOTNET_PORT || 8880
  

  # hubot wants to send chat
  send: (envelope, strings...) ->
    @server.sendChat envelope.message.client, str for str in strings

  # hubot wants to emote to chat
  emote: (envelope, strings...) ->
    @server.sendEmote envelope.message.client, str for str in strings

  # a somewhat more specialized form of sending chat
  reply: (envelope, strings...) ->
    @send envelope, "#{envelope.user}: #{str}" for str in strings

  # hubot wants to set the topic
  topic: (envelope, strings...) ->
    @server.sendTopic envelope.message.client, str for str in strings

  play: (envelope, strings...) ->
    # todo: do we want to support playing sounds?


  run: ->
    @server = new DotNetServer @port

    @server.on 'chat', (user, message, client) =>
      console.log user, ':', message

      msg = new TextMessage user, message
      # attach the client so we know who to reply to
      msg.client = client

      # pipe the incoming chat into hubot
      @receive msg

    @server.on 'emote', (user, message, client) =>
      console.log '*', user, message

      # todo: hubot doesn't have support for receiving emote messages yet

      # msg = new EmoteMessage user, message
      # msg.client = client

      # @receive msg

    @server.on 'topic', (user, topic, client) =>
      console.log user, 'set topic to', topic

      msg = new TopicMessage user, topic
      msg.client = client

      @receive msg

    @server.on 'enter', (user, client) =>
      console.log user, 'entered chat'

      msg = new EnterMessage user
      msg.client = client

      @receive msg

    @server.on 'leave', (user, client) =>
      console.log user, 'left chat'

      msg = new LeaveMessage user
      msg.client = client

      @receive msg

    @server.start()

    # tell hubot we're ready
    @emit 'connected'


  close: ->
    @server.stop()


exports.use = (robot) ->
  new DotNet robot

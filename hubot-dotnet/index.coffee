{Robot, Adapter, TextMessage, EnterMessage, LeaveMessage, Response} = require 'hubot'

DotNetServer = require './dotnet'


class DotNet extends Adapter

  send: (envelope, strings...) ->
    @server.sendChat envelope.message.client, str for str in strings

  emote: (envelope, strings...) ->
    @server.sendEmote envelope.message.client, str for str in strings

  reply: (envelope, strings...) ->
    @send envelope, "#{envelope.user.name}: #{str}" for str in strings


  run: ->
    @server = new DotNetServer 8880

    @server.on 'chat', (user, message, client) =>
      console.log user, ':', message

      msg = new TextMessage user, message
      # attach the client so we know who to reply to
      msg.client = client

      # pipe the incoming chat into hubot
      @receive msg
      
    @server.start()

    # tell hubot we're ready
    @emit 'connected'

exports.use = (robot) ->
  new DotNet robot

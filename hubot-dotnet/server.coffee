{EventEmitter} = require 'events'
net = require 'net'

BufferCursor = require 'buffercursor'


# helper for reading our encoded strings
BufferCursor::readString = ->
  len = @readUInt32LE 4
  return @slice( len ).toString( 'utf8' )

BufferCursor::writeString = (string) ->
  len = Buffer.byteLength( string, 'utf8' )
  @writeUInt32LE len
  @write string, len, 'utf8'


PacketType =
  Invalid: 0

  Chat: 1
  Emote: 2

  Enter: 3
  Leave: 4

  Topic: 5


class DotNetServer extends EventEmitter


  constructor: (@port) ->
    @server = net.createServer()


  start: ->
    @server.on 'connection', (client) => @newClient client
    @server.listen @port

    console.log 'listening on ', @port

  stop: ->
    console.log 'stopping server'

    @server.close()


  sendChat: (client, message) ->
    console.log '<- outgoing: ', message

    data = new BufferCursor( new Buffer( Buffer.byteLength( message ) + 4 + 1 ) )
    data.writeUInt8 1
    data.writeString message

    @sendData client, data

  sendEmote: (client, message) ->

    console.log '<- outgoing: *', message

    data = new BufferCursor( new Buffer( Buffer.byteLength( message ) + 4 + 1 ) )
    data.writeUInt8 2
    data.writeString message

    @sendData client, data

  sendTopic: (client, topic) ->
    data = new BufferCursor( new Buffer( Buffer.byteLength( message ) + 4 + 1 ) )
    data.writeUInt8 5
    data.writeString message

    @sendData client, data


  sendData: (client, data) ->
    # need to seek to 0 if we want the data copy to work
    data.seek 0

    packet = new BufferCursor( new Buffer( data.length + 4 ) )

    # write packet payload length
    packet.writeUInt32LE data.length
    # write payload
    packet.copy data

    # send packet to target client
    client.write packet.buffer


  newClient: (client) ->
    console.log 'new client'

    client.on 'readable', => @readClient client


  readClient: (client) ->

    if not @packetLen
      # if we're here, we just finished reading a packet off the stream (or haven't read any packets)
      # so lets try reading one
      header = client.read 4

      if not header
        # we have absolutely nothing available in our stream buffer somehow
        return

      # read the length from the header
      @packetLen = header.readUInt32LE 0

    # try reading it
    payload = client.read @packetLen

    if not payload
      # we haven't buffered the entire packet yet, we wait
      return

    # at this point we've read the length and the entire payload off the network
    # we'll want to read the next packet length the next time we receive any data
    @packetLen = null

    @handlePayload new BufferCursor( payload ), client

    # try reading anything else we might have buffered
    @readClient client


  handlePayload: (payload, client) ->
    type = payload.readUInt8()
    data = payload.slice()

    switch type
      when PacketType.Chat then @handleChat data, client
      when PacketType.Emote then @handleEmote data, client
      when PacketType.Enter then @handleEnter data, client
      when PacketType.Leave then @handleLeave data, client
      when PacketType.Topic then @handleTopic data, client


  handleChat: (data, client) ->
      user = data.readString()
      message = data.readString()

      @emit 'chat', user, message, client

  handleEmote: (data, client) ->
      user = data.readString()
      message = data.readString()

      @emit 'emote', user, message, client

  handleEnter: (data, client) ->
      user = data.readString()

      @emit 'enter', user, client

  handleLeave: (data, client) ->
      user = data.readString()

      @emit 'leave', user, client

  handleTopic: (data, client) ->
      user = data.readString()
      topic = data.readString()

      @emit 'topic', user, topic, client


module.exports = DotNetServer

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Reusable.Experiments;

public class CR
{
}


//
// /// <summary>
// /// Class used to wrap and interface with a Socket instance in a structured manner.
// /// </summary>
// public class TightbeamProtocol
// {
//     //Public-readonly enum used to display the general state of the protocol instance
//     public ProtocolState State { get; private set; }
//
//     //A pre-connected TCP socket, provided during construction.
//     private Socket link;
//
//     //NodeInfo for both ends of the connection.
//     private NodeInfo personalInfo;
//     private NodeInfo? partnerInfo;
//
//     //Key structs.
//     //The partner RSA public key and shared AES private key are received/generated dynamically.
//     private RSAKeypair personalKeypair;
//     private RSAKeypair partnerKeypair;
//     private AESKey sharedPrivateKey;
//
//     //Outgoing message queue.
//     private Queue<NetworkMessage> outgoinqQueue = new();
//
//     //Received message queue.
//     public Queue<NetworkMessage> ReceivedMessages = new();
//
//     //Event that fires whenever an inbound message is fully received, deserialized and enqueued.
//     public event EventHandler? MessageReceived;
//
//     //4-byte buffer used to receive the length of following data streams
//     private byte[] lengthBuffer = new byte[sizeof(int)];
//
//     //Buffer used to spool incoming serial data.
//     private byte[]? dataBuffer = null;
//
//     //Integer used to hold byte read progress.
//     private int bytesReceived = 0;
//
//     public bool anyReceivedMessages => (ReceivedMessages.Count > 0);
//
//     private bool anySpooledMessages => outgoinqQueue.Count != 0;
//
//     private int spooledMessages => outgoinqQueue.Count;
//
//     public TightbeamProtocol(Socket socket, NodeInfo info, RSAKeypair personalKeypair)
//     {
//         State = ProtocolState.StartingUp;
//
//         //Construct instance members
//         link = socket;
//         personalInfo = info;
//         this.personalKeypair = personalKeypair;
//
//         //Spin up continuous I/O tasks
//         Task.Run(this.ContinuousSend);
//         Task.Run(this.ContinuousReceive);
//
//         //Start handshake handler
//         Task.Run(this.PerformHandshake);
//         Console.WriteLine(this.GetHashCode() + " Construction complete");
//     }
//
//     private async Task PerformHandshake()
//     {
//         //Send handshake to partner
//         Handshake outboundHandshake = new Handshake(personalInfo);
//         EnqueueMessage(outboundHandshake, Encryption.None);
//
//         //Await/act upon partner handshake
//         Console.WriteLine(this.GetHashCode() + " Handshake sent...");
//         NetworkMessage handshakeMessage = await AwaitMessageOfType(typeof(Handshake));
//         Console.WriteLine(this.GetHashCode() + " Handshake RECEIVED!");
//         Handshake partnerHandshake = Serializer.DeserializeContent<Handshake>(handshakeMessage.Data)!;
//         partnerInfo = partnerHandshake.senderInfo;
//         partnerKeypair = partnerInfo.PublicKey;
//
//         //Key exchange
//         Console.WriteLine(this.GetHashCode() + " Key Exchange sent...");
//         AESKey localKey = Cryptographer.GenerateAESKey();
//         EnqueueMessage(new KeyExchange(localKey), Encryption.RSA);
//         NetworkMessage exchangeMessage = await AwaitMessageOfType(typeof(KeyExchange));
//         KeyExchange receivedExchange = Serializer.DeserializeContent<KeyExchange>(exchangeMessage.Data, personalKeypair)!;
//         sharedPrivateKey = Cryptographer.CascadingKeySelect(localKey, receivedExchange.AESKey);
//         Console.WriteLine(this.GetHashCode() + " " + string.Join(", ", sharedPrivateKey.Key));
//         Console.WriteLine(this.GetHashCode() + " Key Exchange COMPLETE!");
//
//         //Unlock protocol instance
//         ReceivedMessages.Clear();
//         State = ProtocolState.Operational;
//     }
//
//     private async Task<NetworkMessage> AwaitMessageOfType(Type type)
//     {
//         while (true)
//         {
//             var messageQuery = 
//                 from msg in ReceivedMessages
//                 where msg.ContentType == type
//                 select msg;
//
//             if (messageQuery.Any())
//             {
//                 return messageQuery.First();
//             }
//
//             await Task.Delay(100);
//         }
//     }
//
//     /// <summary>
//     /// <br>Starts the graceful shutdown process of a connected protocol instance.</br>
//     /// <br>Graceful shutdown waits for all enqueued messages to be sent, and then closes the socket connection.</br>
//     /// </summary>
//     public async Task StartShutdown()
//     {
//         State = ProtocolState.ShuttingDown;
//         //TODO queue graceful disconnect packet
//         while (anySpooledMessages)
//         {
//             await Task.Delay(50);
//         }
//
//         try
//         {
//             link.Shutdown(SocketShutdown.Both);
//         }
//         finally
//         {
//             link.Close();
//             State = ProtocolState.Inert;
//         }
//     }
//
//     /// <summary>
//     /// Wraps a given content object and header information into a NetworkMessage instance, and enqueues it accordingly.
//     /// </summary>
//     public void EnqueueMessage<T>(T content, Encryption encryption)
//     {
//         if (State > ProtocolState.Operational)
//         {
//             return;
//         }
//
//         byte[] serializedContent;
//
//         switch (encryption)
//         {
//             case Encryption.AES:
//                 serializedContent = Serializer.SerializeContent(content, sharedPrivateKey);
//                 break;
//
//             case Encryption.RSA:
//                 serializedContent = Serializer.SerializeContent(content, partnerKeypair);
//                 break;
//
//             default:
//                 serializedContent = Serializer.SerializeContent(content);
//                 break;
//         }
//
//         var newMessage = new NetworkMessage(serializedContent, typeof(T), encryption);
//         outgoinqQueue.Enqueue(newMessage);
//     }
//
//     /// <summary>
//     /// Persistent Task that continuously attempts to dequeue and send any spooled messages.
//     /// </summary>
//     private async Task ContinuousSend()
//     {
//         while (State < ProtocolState.Inert)
//         {
//             if (anySpooledMessages)
//             {
//                 NetworkMessage message = outgoinqQueue.Dequeue()!;
//                 byte[]? data = Serializer.SerializeMessage(message, true);
//                 try
//                 {
//                     Console.WriteLine(this.GetHashCode() + " Sending message of type: " + message.ContentType.ToString());
//                     await link.SendAsync(data, SocketFlags.None);
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine(ex.ToString());
//                 }
//             }
//         }
//     }
//
//     /// <summary>
//     /// Persistent Task that continuously attempts to receive inbound bytes.
//     /// </summary>
//     private async Task ContinuousReceive()
//     {
//         while (State < ProtocolState.ShuttingDown)
//         {
//             if (link.Available > 0)
//             {
//                 var data = new byte[link.Available];
//                 try
//                 {
//                     await link.ReceiveAsync(data, SocketFlags.None);
//                     SortData(data);
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine(ex.ToString());
//                 }
//             }
//
//             //Brief 100ms delay to allow bytes to accumulate in the socket.
//             await Task.Delay(100);
//         }
//     }
//
//     /// <summary>
//     /// Sorts given inbound bytes into the correct buffer.
//     /// </summary>
//     private void SortData(byte[] data)
//     {
//         var i = 0;
//         while (i != data.Length)
//         {
//             var availableBytes = data.Length - i;
//             if (dataBuffer is not null)
//             {
//                 //Data buffer has been initialized; assume length buffer has been prepared and read into data buffer.
//                 var requestedBytes = dataBuffer.Length - bytesReceived;
//
//                 var transferredBytes = Math.Min(requestedBytes, availableBytes);
//                 Array.Copy(data, i, dataBuffer, bytesReceived, transferredBytes);
//                 i += transferredBytes;
//
//                 ParseData(transferredBytes);
//             }
//             else
//             {
//                 //Data buffer is un-initialized; assume we are reading into the length buffer.
//                 var requestedBytes = lengthBuffer.Length - bytesReceived;
//
//                 var transferredBytes = Math.Min(requestedBytes, availableBytes);
//                 Array.Copy(data, i, lengthBuffer, bytesReceived, transferredBytes);
//                 i += transferredBytes;
//
//                 ParseData(transferredBytes);
//             }
//         }
//     }
//
//     /// <summary>
//     /// Acts upon received data once a buffer is full. 
//     /// </summary>
//     private void ParseData(int count)
//     {
//         bytesReceived += count;
//         if (this.dataBuffer is null)
//         {
//             //Attempt to parse length buffer.
//             if (bytesReceived != sizeof(int))
//             {
//                 /*Pass - awaiting length buffer completion.*/
//             }
//             else
//             {
//                 //Parse length buffer to determine content stream size and initialize data buffer accordingly.
//                 var dataLength = BitConverter.ToInt32(lengthBuffer, 0);
//                 if (dataLength is < 0 or > int.MaxValue)
//                 {
//                     throw new InvalidOperationException("Message length is out of bounds!");
//                 }
//                 else
//                 {
//                     dataBuffer = new byte[dataLength];
//                     bytesReceived = 0;
//                 }
//             }
//         }
//         else
//         {
//             //Attempt to parse contents of data buffer.
//             if (bytesReceived != dataBuffer.Length)
//             {
//                 /*Pass - awaiting data buffer completion.*/
//             }
//             else
//             {
//                 //Parse data buffer into a NetworkMessage.
//                 NetworkMessage? message = Serializer.DeserializeMessage(dataBuffer);
//                 if (message is null)
//                 {
//                     throw new InvalidDataException("Received corrupt or invalid data - could not deserialize to NetworkMessage.");
//                 }
//
//                 //Decrypt data if needed.
//                 if (message.Encryption == Encryption.AES)
//                 {
//                     message.DecryptData(sharedPrivateKey);
//                 }
//
//                 ReceivedMessages.Enqueue(message);
//                 Console.WriteLine(this.GetHashCode() + " Received message of type: " + ReceivedMessages.Peek().ContentType.ToString());
//                 OnMessageReceived();
//
//                 //Reset data buffer and byte reception tally for next message.
//                 dataBuffer = null;
//                 bytesReceived = 0;
//             }
//         }
//     }
//
//     private void OnMessageReceived()
//     {
//         //Alert any subscribers that we've received a new message.
//         var handler = MessageReceived;
//         handler?.Invoke(this, EventArgs.Empty);
//     }
// }
//
// public enum ProtocolState
// {
//     StartingUp,
//     Operational,
//     ShuttingDown,
//     Inert,
//     Exception
// }
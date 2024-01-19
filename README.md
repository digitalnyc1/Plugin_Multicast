# Plugin_Multicast
This plugin allows sending and receiving UDP multicast messages to Genie clients on the same network.

## Usage
Usage:
/multicast [message]

The message will be received by all Genie clients on the same LAN which have the Multicast plugin configured the same way.  Due to use of the MulticastLoopback socket option, the host that sent the message will also recieve the same message echoed back to it.  This is mitigated by assining a randomly generated GUID to each client.  When the client receives a message sent with its own GUID, the message is ignored.

The received multicast messages are passed to Genie using the #parse command.  This should allow the messages to be used in triggers, actions, etc.  The messages will not be echoed to the Game window or server.

## Example
Client 1 has the following trigger set up:<br/>
  \#trigger {#multicast teach,(.\*),(.\*)} {TEACH $2 TO $1}

Client 2 sends a request for teaching:<br/>
  /multicast teach,person,skill

Client 1 receives the following message which is passed to Genie using #parse:<br/>
  #multicast teach,person,skill

Which should fire the trigger to teach on Client 1:<br/>
  TEACH skill TO person

## FAQ
1. Why am I prompted by Windows Firewall to allow Genie access to the network?<br/>
  The Multicast plugin binds to a TCP/IP socket to listen for incoming messages on the configured multicast address and port.  This will trigger the Windows Firewall prompt to allow access to the network.

1. Why use client GUIDs instead of setting MulticastLoopback to false?<br/>
  If the MulticastLoopback socket option is set, Genie clients running on the same computer will not see each other's multicast messages.  Instead, a GUID is assigned to each Genie client.  This GUID is used to ignore the messages received by the same client that send them.

1. Not able to send/receive messages after changing settings?<br/>
  If you've changed the multicast endpoint IP address or port, you must manually reload the Multicast plugin.

Feature: MessageTransfer

4.2.1 In outline, the router shall:
- receive messages from MTAs and UAs;
- pass each message to the appropriate UA if that UA is present at the local node;
- if the message is destined for a UA which is not present at the local node, then
the router shall determine which is the most appropriate MTA to transfer the
message;
- in the event that the MTA first selected is unable to transfer the message, the
router shall select another MTA and pass the message to it, and so on.

Background: setup
  Given the router domain is loaded
	And the following current parameter table
	| name           | value | password_level |
	| brigade_number | 26    | 3              |
	| node_number    | 3     | 3              |
	| node_name      | bob   | 3              |
	And the following GD-92 bits are running
	| name    | port |
	| LAN MTA | 23   |
	| Router  | 25   |
	| NM UA   | 1    |

# Pass each message to the appropriate UA if that UA is present at the local node
Scenario: Routing message to local UA that exists
	Given the router receives the following message
	| number | source   | dest           |
	| 51     | 26:100:1 | 26:3:1,26:1:27 |
	When the router routes the message
	Then the message is forwarded to port 1

# if the message is destined for a UA which is not present at the local node, 
# then the router shall determine which is the most appropriate MTA to 
# transfer the message
Scenario: Routing message to remote UA
	Given the router receives the following message
	| number | source   | dest             |
	| 51     | 26:100:1 | 26:333:1,26:1:27 |
	And the Following router table exists
	| next_node | desination_nodes  | agent_type | preference |
	| 200       | 26:200:0,26:333:0 | 0          | 1          |
	| 200       | 26:333:0          | 1          | 2          |
	When the router routes the message
	Then the message is forwarded to port 23

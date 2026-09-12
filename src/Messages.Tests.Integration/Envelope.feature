Feature: Envelopes

  Scenario: Creating an Envelope for a Text message
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a single-block Text message containing "FIRE"
    When the Envelope is created
    Then its complete Envelope bytes are "1A191902011A191912FCD11B01010004464952453B"

  Scenario: Decoding a Text Message Envelope
    Given encoded Envelope bytes "1A191902011A191912FCD11B01010004464952453B"
    When the Envelope is decoded
    Then its decoded source is Brigade 26, Node 100, Port 25
    And it has 1 decoded destination
    And its decoded Text Message Contents are block 1 of 1 containing "FIRE"
    And its complete Envelope bytes are "1A191902011A191912FCD11B01010004464952453B"

  Scenario: Rejecting an Envelope with a mismatched Block Check Character
    Given encoded Envelope bytes "1A191902011A191912FCD11B01010004464952453A"
    When decoding the Envelope is attempted
    Then the Block Check Character mismatch is rejected

  Scenario: Rejecting an Envelope whose declared Message Length exceeds its Text Contents
    Given encoded Envelope bytes "1A191902411A191912FCD11B01010004464952453B"
    When decoding the Envelope is attempted
    Then the Message Contents length mismatch is rejected

  Scenario: Rejecting an Envelope with malformed compressed Text Contents
    Given encoded Envelope bytes "1A191901411A191912FCD11B010100011B7E"
    When decoding the Envelope is attempted
    Then the malformed Text Contents are rejected

  Scenario: Rejecting an Envelope with an invalid compressed Text run length
    Given encoded Envelope bytes "1A191901C11A191912FCD11B010100031B4103BE"
    When decoding the Envelope is attempted
    Then the malformed Text Contents are rejected

  Scenario: Rejecting an Envelope with non-empty Acknowledgement Contents
    Given encoded Envelope bytes "1A195A00411A1919127CD132008F"
    When decoding the Envelope is attempted
    Then the non-empty Acknowledgement Contents are rejected

  Scenario: Rejecting an Envelope whose Contents exceed the protocol maximum
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And Message Contents containing 1024 bytes
    When creation of the oversized Envelope is attempted
    Then the oversized Envelope is rejected

  Scenario: Decoding a Set Parameter Envelope
    Given encoded Envelope bytes "1A191902011A191912FCD13C01010004464952451C"
    When the Envelope is decoded
    Then its decoded Set Parameter identifies the non-volatile table, parameter number 1, and value bytes "000446495245"
    And its complete Envelope bytes are "1A191902011A191912FCD13C01010004464952451C"

  Scenario: Rejecting a known unsupported Envelope with a Negative Acknowledgement
    Given encoded Envelope bytes "1A191902011A191912FCD142010100044649524562"
    And the affected destination is Brigade 26, Node 101, Port 26
    When the Envelope is decoded
    Then its Contents are preserved as Message Type 66 with bytes "0101000446495245"
    When a Negative Acknowledgement Envelope is created by Brigade 26, Node 101, Port 26 using protocol version 2 and the General Reason Code "inv_mess"
    Then its complete Envelope bytes are "1A195A01811A1919127CD133011A195A010315"

  Scenario: Creating and decoding a Text Message Envelope for two destinations
    Given an Envelope source of Brigade 26, Node 100, and Port 25
    And Envelope destinations Brigade 26, Node 100, Port 25 and Brigade 26, Node 101, Port 26
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a single-block Text message containing "FIRE"
    When the Envelope is created and decoded
    Then its complete Envelope bytes are "1A191902021A19191A195A12FCD11B010100044649524561"
    And it has 2 decoded destinations in the declared order

  Scenario: Creating and decoding a Parameter Request Envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Parameter Request for the current table and parameter number 1
    When the Envelope is created and decoded
    Then its decoded Parameter Request identifies the current table and parameter number 1
    And its complete Envelope bytes are "1A191900811A191912FCD13D020180"

  Scenario: Rejecting an unacknowledged Parameter Request Envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 without requesting acknowledgement
    And a Parameter Request for the current table and parameter number 1
    When Envelope creation is attempted
    Then the Parameter Request Envelope creation is rejected

  Scenario: Requesting one Router-table entry
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Parameter Request Multiple for current-table parameter number 13 and entries 1 through 1
    When the Envelope is created and decoded
    Then its decoded Parameter Request Multiple identifies current-table parameter number 13 and entries 1 through 1
    And its complete Envelope bytes are "1A191901811A191912FCD13F020D000100018F"

  Scenario: Requesting the two most recent Router-table entries
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Parameter Request Multiple for current-table parameter number 13 requesting the 2 most recent entries
    When the Envelope is created and decoded
    Then its decoded Parameter Request Multiple requests the 2 most recent entries
    And its complete Envelope bytes are "1A191901811A191912FCD13F020D000000028D"

  Scenario: Returning the current brigade number for a Parameter Request
    Given encoded Envelope bytes "1A191900811A195A12FCD13D0201C3"
    When a Parameter Envelope is created by Brigade 26, Node 101, Port 26 using protocol version 2 returning brigade number 26
    Then its Parameter Contents contain no more values and brigade number 26
    And its complete Envelope bytes are "1A195A00811A1919127CD13E001A59"

  Scenario: Decoding an Acknowledgement Envelope
    Given encoded Envelope bytes "1A195A00011A1919127CD132CF"
    When the Envelope is decoded
    Then its Contents are an Acknowledgement
    And its complete Envelope bytes are "1A195A00011A1919127CD132CF"

  Scenario: Acknowledging a received Text Message Envelope
    Given encoded Envelope bytes "1A191902011A195A12FCD11B010100044649524578"
    When an Acknowledgement Envelope is created by Brigade 26, Node 101, Port 26 using protocol version 2
    Then its complete Envelope bytes are "1A195A00011A1919127CD132CF"

  Scenario: Decoding a Negative Acknowledgement Envelope
    Given encoded Envelope bytes "1A195A01811A1919127CD133011A1919010356"
    When the Envelope is decoded
    Then its Contents are a Negative Acknowledgement
    And it identifies Brigade 26, Node 100, Port 25 as the affected destination
    And its General Reason Code is "InvalidMessage"
    And its complete Envelope bytes are "1A195A01811A1919127CD133011A1919010356"

  Scenario: Rejecting an acknowledgement-requested Text Message with a Negative Acknowledgement
    Given encoded Envelope bytes "1A191902011A195A12FCD11B010100044649524578"
    And the affected destination is Brigade 26, Node 100, Port 25
    When a Negative Acknowledgement Envelope is created by Brigade 26, Node 101, Port 26 using protocol version 2 and the General Reason Code "inv_mess"
    Then its complete Envelope bytes are "1A195A01811A1919127CD133011A1919010356"

  Scenario: Rejecting an acknowledgement for an unacknowledged Text Message Envelope
    Given encoded Envelope bytes "1A191902011A195A127CD11B0101000446495245F8"
    When creating an Acknowledgement Envelope is attempted by Brigade 26, Node 101, Port 26 using protocol version 2
    Then the acknowledgement response is rejected

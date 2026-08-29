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

  Scenario: Rejecting a known unsupported Envelope with a Negative Acknowledgement
    Given encoded Envelope bytes "1A191902011A191912FCD13C01010004464952451C"
    And the affected destination is Brigade 26, Node 101, Port 26
    When the Envelope is decoded
    Then its Contents are preserved as Message Type 60 with bytes "0101000446495245"
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

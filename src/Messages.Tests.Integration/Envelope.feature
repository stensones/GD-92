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

  Scenario: Rejecting a known but unsupported Message Type
    Given encoded Envelope bytes "1A191902011A191912FCD132010100044649524512"
    When decoding the unsupported Envelope is attempted
    Then the unsupported Message Type is rejected

  Scenario: Creating and decoding a Text Message Envelope for two destinations
    Given an Envelope source of Brigade 26, Node 100, and Port 25
    And Envelope destinations Brigade 26, Node 100, Port 25 and Brigade 26, Node 101, Port 26
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a single-block Text message containing "FIRE"
    When the Envelope is created and decoded
    Then its complete Envelope bytes are "1A191902021A19191A195A12FCD11B010100044649524561"
    And it has 2 decoded destinations in the declared order

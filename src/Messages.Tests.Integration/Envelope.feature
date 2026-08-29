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

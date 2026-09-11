Feature: Peripheral status requests

  Scenario: Creating and decoding a peripheral status request envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Peripheral Status Request
    When the Envelope is created and decoded
    Then its decoded Contents are a Peripheral Status Request
    And its complete Envelope bytes are "1A191900011A191912FCD10937"

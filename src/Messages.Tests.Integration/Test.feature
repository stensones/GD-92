Feature: Remote tests

  Scenario: Creating and decoding a remote test envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Test with opaque test type 165
    When the Envelope is created and decoded
    Then its decoded Test preserves opaque test type 165

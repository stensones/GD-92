Feature: Database queries

  Scenario: Creating and decoding a database query envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Data Base Query with opaque query type 165 containing "SELECT STATUS"
    When the Envelope is created and decoded
    Then its decoded Data Base Query preserves opaque query type 165 and text "SELECT STATUS"

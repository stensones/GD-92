Feature: Resource statuses

  Scenario: Creating and decoding a resource status envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 without requesting acknowledgement
    And a Resource Status reporting resource "A1" as Available At Base with remarks "READY"
    When the Envelope is created and decoded
    Then its decoded Resource Status reports resource "A1" as Available At Base with remarks "READY"

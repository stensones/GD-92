Feature: Resource status requests

  Scenario: Creating and decoding a resource status request envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Resource Status Request for resource "A1"
    When the Envelope is created and decoded
    Then its decoded Resource Status Request identifies resource "A1"
    And its complete Envelope bytes are "1A191900C11A191912FCD10502413189"

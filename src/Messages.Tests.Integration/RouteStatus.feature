Feature: Route statuses

  Scenario: Creating and decoding a route status envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Route Status enabling routes to Brigade 26, Nodes 101 through 102, and Port 25
    When the Envelope is created and decoded
    Then its decoded Route Status enables routes to Brigade 26, Nodes 101 through 102, and Port 25

Feature: Formatted text

  Scenario: Creating and decoding a formatted text envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And Formatted Text containing the text table "RESOURCE A1 READY"
    When the Envelope is created and decoded
    Then its decoded Formatted Text contains the text table "RESOURCE A1 READY"
